using UnityEngine;
public class GameOfLife : ProcessingLite.GP21
{
    GameCell[,] cells; //Our game grid matrix
    float cellSize = 0.25f; //Size of our cells
    int numberOfColums;
    int numberOfRows;
    int spawnChancePercentage = 15;

    GameCell[,] newCells;

    void Start()
    {
        //Lower framerate makes it easier to test and see whats happening.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 2;

        //Calculate our grid depending on size and cellSize
        numberOfColums = (int)Mathf.Floor(Width / cellSize);
        numberOfRows = (int)Mathf.Floor(Height / cellSize);

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
                //Random check to see if it should be alive
                if (Random.Range(0, 100) < spawnChancePercentage)
                {
                    cells[x, y].alive = true;
                }
            }
        }
    }

    void Update()
    {
        //Clear screen
        Background(0);

        //TODO: Calculate next generation
        calculateNewCells();

        //TODO: update buffer
        updateNewCells();

        //Draw all cells.
        for (int y = 0; y < numberOfRows; ++y)
        {
            for (int x = 0; x < numberOfColums; ++x)
            {
                //Draw current cell
                cells[x, y].Draw();
            }
        }
    }

    void calculateNewCells()
    {
        int amountOfAlivePartners = 0;
        
        for (int y = 0; y < numberOfRows; ++y)
        {
            for (int x = 0; x < numberOfColums; ++x)
            {
                if (cells[x, y].alive)
                {
                    amountOfAlivePartners = scanAroundCell(x, y);
                    if (amountOfAlivePartners < 2)
                    {
                        newCells[x,y].alive = false;
                        newCells[x, y].recentlyDied = true;
                    }
                    else if (amountOfAlivePartners == 2 || amountOfAlivePartners == 3)
                    {
                        newCells[x,y].alive = true;
                        newCells[x, y].recentlyDied = false;
                    }
                    else if (amountOfAlivePartners > 3)
                    {
                        newCells[x,y].alive = false;
                        newCells[x, y].recentlyDied = true;
                    }
                }

                if (!cells[x, y].alive)
                {
                        
                    amountOfAlivePartners = scanAroundCell(x, y);
                    if (amountOfAlivePartners == 3)
                    {
                        newCells[x,y].alive = true;
                        newCells[x, y].recentlyDied = false;
                    }
                }
            }
        }
    }

    int scanAroundCell(int xCell, int yCell)
    {
        int amountOfAlivePartners = 0;

        for (int x = -1; x < 2; x++)
        {
            if (xCell + x < 0 || xCell + x > numberOfColums-1)
            {
                continue;
            }
            for (int y = -1; y < 2; y++)
            {
                if (yCell + y < 0 || yCell + y > numberOfRows-1)
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

    void updateNewCells()
    {
        for (int y = 0; y < numberOfRows; ++y)
        {
            for (int x = 0; x < numberOfColums; ++x)
            {
                cells[x, y].alive = newCells[x,y].alive;
                cells[x, y].recentlyDied = newCells[x,y].recentlyDied;

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
        }else if (recentlyDied)
        {
            DecayDeadCell();
        }
    }
    public void DecayDeadCell()
    {
        
        
        float timeCount = 0;
        for (float i = 6; i > 0; i--)
        {
            timeCount += Time.deltaTime;
            if (timeCount >= 1)
            {
                alphaCount -= 40;
                timeCount = 0;
            }
            Mathf.Clamp(alphaCount, 0, 255);
        }
            Fill(255, 0, 0, alphaCount);
            Stroke(255, 0, 0, alphaCount);
            Circle(x, y, size);
    }
}