using UnityEngine;
using UnityEngine.UI;
public class GameOfLife : ProcessingLite.GP21
{
    GameCell[,] cells; //Our game grid matrix
    GameCell[,] newCells;

    float cellSize = 0.15f; //Size of our cells
    int numberOfColums;
    int numberOfRows;

    bool startGame = false;
    int generations;
    int simulationSpeed;

    public Text generationsText;
    public Text simulationSpeedText;
    public Slider simulationSpeedSlider;

    //Image input
    public Texture2D imageToGenerate;
    int imgWidth;
    int imgHeight;
    bool generationComplete = false;
    void Start()
    {
        //Lower framerate makes it easier to test and see whats happening.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 20;
        simulationSpeed = Application.targetFrameRate;
        simulationSpeedSlider.value = simulationSpeed;
        //Calculate our grid depending on size and cellSize
        numberOfColums = (int)Mathf.Floor(Width / cellSize);
        numberOfRows = (int)Mathf.Floor(Height / cellSize);
        //Get widht and height of img
        imgWidth = imageToGenerate.width;
        imgHeight = imageToGenerate.height;

        //Initiate our matrix array
        cells = new GameCell[numberOfColums, numberOfRows];
        newCells = new GameCell[numberOfColums, numberOfRows];

        //Create all objects

        //For each row
        for (int y = 0; y < numberOfRows; ++y)
        {
            //for each column in each row
            for (int x = 0; x < numberOfColums; ++x)
            {
                //Create our game cell objects, multiply by cellSize for correct world placement
                cells[x, y] = new GameCell(x * cellSize, y * cellSize, cellSize);
                newCells[x, y] = new GameCell(x * cellSize, y * cellSize, cellSize);
            }
        }
    }

    void Update()
    {
        //Clear screen
        Background(0);
        //Generate cell base on image
        if (!generationComplete)
        {
            CreateCellsByImage(imageToGenerate, imgWidth, imgHeight);
            generationComplete = true;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            startGame = true;
        }
        if (startGame)
        {
            //Calculate next generation
            CalculateNewCells();

            //Update buffer
            UpdateNewCells();

        }
        else
        {
            for (int y = 0; y < numberOfRows; ++y)
            {
                for (int x = 0; x < numberOfColums; ++x)
                {
                    //Draw current cell if mouse is over it
                    cells[x, y].DrawOnMouseHover(cellSize);
                }
            }
        }
        //Draw all cells.
        for (int y = 0; y < numberOfRows; ++y)
        {
            for (int x = 0; x < numberOfColums; ++x)
            {
                //Draw current cell
                cells[x, y].Draw();
            }
        }
        ChangeSimulationSpeed();
    }

    void CalculateNewCells()
    {
        int amountOfAlivePartners = 0;

        for (int y = 0; y < numberOfRows; ++y)
        {
            for (int x = 0; x < numberOfColums; ++x)
            {
                if (cells[x, y].alive)
                {
                    amountOfAlivePartners = ScanAroundCell(x, y);
                    if (amountOfAlivePartners < 2 || amountOfAlivePartners > 3)
                    {
                        newCells[x, y].alive = false;
                        newCells[x, y].recentlyDied = true;
                    }
                    else if (amountOfAlivePartners == 2 || amountOfAlivePartners == 3)
                    {
                        newCells[x, y].alive = true;
                    }
                }
                else
                {

                    amountOfAlivePartners = ScanAroundCell(x, y);
                    if (amountOfAlivePartners == 3)
                    {
                        newCells[x, y].alive = true;
                        UpdateAndDrawGenereationsText();
                    }
                }
            }
        }
    }

    int ScanAroundCell(int xCell, int yCell)
    {
        int amountOfAlivePartners = 0;
        //check around the cell for alive partners
        for (int x = -1; x < 2; x++)
        {
            if (xCell + x < 0 || xCell + x > numberOfColums - 1)
            {
                continue;
            }
            for (int y = -1; y < 2; y++)
            {
                if (yCell + y < 0 || yCell + y > numberOfRows - 1)
                {
                    continue;
                }
                else if (xCell + x == xCell && yCell + y == yCell)
                {
                    continue;
                }
                if (cells[xCell + x, yCell + y].alive)
                {
                    amountOfAlivePartners++;
                }

            }
        }

        return amountOfAlivePartners;
    }

    void UpdateNewCells()
    {
        for (int y = 0; y < numberOfRows; ++y)
        {
            for (int x = 0; x < numberOfColums; ++x)
            {
                cells[x, y].alive = newCells[x, y].alive;
                cells[x, y].recentlyDied = newCells[x, y].recentlyDied;

            }
        }
    }
    void UpdateAndDrawGenereationsText()
    {
        generations++;
        generationsText.text = "Generations: " + generations;
    }
    void ChangeSimulationSpeed()
    {
        simulationSpeedText.text = "Simulation Speed: " + Application.targetFrameRate;
        Application.targetFrameRate = (int)simulationSpeedSlider.value;
    }
    public void CreateCellsByImage(Texture2D imageToGenerate, int imgWidth, int imgHeight)
    {
        for (int pixelX = 0; pixelX <= imgWidth; pixelX++)
        {
            for (int pixelY = 0; pixelY <= imgHeight; pixelY++)
            {
                if (imageToGenerate.GetPixel(pixelX, pixelY) == Color.white)
                {
                    for (int y = 0; y < numberOfRows; ++y)
                    {
                        for (int x = 0; x < numberOfColums; ++x)
                        {
                            if (pixelX * cellSize > x - cellSize && pixelX * cellSize < x + cellSize
                                && pixelY * cellSize > y - cellSize && pixelY * cellSize < y + cellSize)
                            {
                                cells[x, y].alive = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
//You will probebly need to keep track of more things in this class
public class GameCell : ProcessingLite.GP21
{
    public float x, y; //Keep track of our position
    float size; //our size
    int alphaCount = 255;
    //If we recently died, will fade away
    public bool recentlyDied = false;
    //Keep track if we are alive
    public bool alive = false;

    //Constructor
    public GameCell(float x, float y, float size)
    {
        //Our X is equal to incoming X, and so forth
        //adjust our draw position so we are centered
        this.x = x + size / 2;
        this.y = y + size / 2;

        //diameter/radius draw size fix
        this.size = size / 2;
    }

    public void Draw()
    {
        //If we are alive, draw our dot.
        if (alive)
        {
            Fill(255, 255, 255);
            Stroke(255, 255, 255);
            //draw our dots
            Circle(x, y, size);
            alphaCount = 255;
        }
        else
        {
            if (recentlyDied)
            {
                alphaCount -= 40;

                if (alphaCount <= 0)
                {
                    recentlyDied = false;
                    alphaCount = 0;
                }

                Fill(255, 0, 0, alphaCount);
                Stroke(255, 0, 0, alphaCount);
                Circle(x, y, size);
            }
        }

    }
    public void DrawOnMouseHover(float cellSize)
    {
        float mouseX = MouseX;
        float mouseY = MouseY;
        cellSize = cellSize / 2;
        //check mouse pos is within the cells position, in a box
        if (mouseX > x - cellSize && mouseX < x + cellSize && mouseY > y - cellSize && mouseY < y + cellSize)
        {
            Fill(255, 255, 255);
            Stroke(255, 255, 255);
            Circle(x, y, size);

            if (Input.GetMouseButton(0))
            {
                alive = true;
            }
        }
    }

}