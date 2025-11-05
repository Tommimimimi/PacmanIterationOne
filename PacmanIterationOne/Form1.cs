using System.Threading;
using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Diagnostics;

namespace PacmanIterationOne
{
    //define different directions as named constants in enum
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        None
    }
    
    public partial class Form1 : Form
    {
        public Dictionary<Direction, float> directionAngle = new()
        {
            { Direction.Up, 270 },
            { Direction.Down, 90 },
            { Direction.Left, 180 },
            { Direction.Right, 0 },
            { Direction.None, 0 }
        };
        //define empty maze
        int[,] arrMaze;

        //declare empty integers to define using cellsize
        private static int
            intPlayerX,
            intPlayerY,
            intPlayerSpeed,
            intMazeX,
            intMazeY,
            intScore,
            R,
            G,
            B,
            intCellSize = 40;

        //declare current and next direction variables
        static Direction 
        dirCurrent = Direction.None,
        dirNext = Direction.None;

        Random rnd = new Random();

        //create system resources
        Thread thrdGameLoop;
        Rectangle rectPlayer;

        List<Ghost> listGhosts = new List<Ghost>();
        Brush brush = new SolidBrush(Color.FromArgb(200, 20, 20, 20));

        float fltMouthAngle = 0;

        Stopwatch swMouthTime = new Stopwatch();
        Label lblScore = new Label();

        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false;

            //choose random numbers for maze size
            intMazeX = rnd.Next(11, 14);
            intMazeY = rnd.Next(16, 20);
            //make sure maze dimensions are odd numbers in order for maze pathing
            intMazeX = intMazeX * 2 + 1;
            intMazeY = intMazeY * 2 + 1;

            //initialize maze array and form size
            arrMaze = new int[intMazeX, intMazeY];
            ClientSize = new Size(intMazeY * intCellSize, intMazeX * intCellSize);

            //initialize player position and speed
            intPlayerX = intCellSize;
            intPlayerY = intCellSize;
            intPlayerSpeed = intCellSize / 8;
            rectPlayer = new Rectangle(intPlayerX, intPlayerY, intCellSize, intCellSize);

            //create the four ghosts
            listGhosts.Add(new Ghost(400, 400, Color.Red, arrMaze, intCellSize));
            listGhosts.Add(new Ghost(300, 300, Color.Pink, arrMaze, intCellSize));
            listGhosts.Add(new Ghost(100, 100, Color.Cyan, arrMaze, intCellSize));
            listGhosts.Add(new Ghost(200, 200, Color.Orange, arrMaze, intCellSize));

            //creating the label and setting attributes
            lblScore.Location = new Point(ClientSize.Width - lblScore.Width * 2, 0);
            lblScore.Size = new Size(intCellSize * 10, intCellSize);
            lblScore.Font = new Font("Comic Sans MS", 20);
            lblScore.BackColor = Color.Transparent;
            this.Controls.Add(lblScore);

            //create and start game loop thread
            thrdGameLoop = new Thread(GameLoop);
            thrdGameLoop.Start();

            this.DoubleBuffered = true;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //movement up, down, left, right respectively
                case Keys.W:
                    dirNext = Direction.Up;
                    break;
                case Keys.S:
                    dirNext = Direction.Down;
                    break;
                case Keys.A:
                    dirNext = Direction.Left;
                    break;
                case Keys.D:
                    dirNext = Direction.Right;
                    break;
            }
        }

        private void SetMazeValue()
        {
            //loop through maze array and set all values to 1 (wall)
            for (int x = 0; x < intMazeX; x++)
            {
                for (int y = 0; y < intMazeY; y++)
                {
                    arrMaze[x, y] = 1;
                }
            }
        }

        private void MazePathing(int paraRow, int paraCol)
        {
            arrMaze[paraRow, paraCol] = 0;

            List<Direction> mazeDirections = new List<Direction>
            {
                Direction.Up, Direction.Down, Direction.Left, Direction.Right
            };

            for (int i = 0; i < mazeDirections.Count; i++)
            {
                int swapIndex = rnd.Next(i, mazeDirections.Count);
                var temp = mazeDirections[i];
                mazeDirections[i] = mazeDirections[swapIndex];
                mazeDirections[swapIndex] = temp;
            }

            foreach (var direction in mazeDirections)
            {
                int testRow = paraRow;
                int testCol = paraCol;

                switch (direction)
                {
                    case Direction.Up:
                        testRow = paraRow - 2;
                        break;
                    case Direction.Down:
                        testRow = paraRow + 2;
                        break;
                    case Direction.Left:
                        testCol = paraCol - 2;
                        break;
                    case Direction.Right:
                        testCol = paraCol + 2;
                        break;
                }
                if (testRow > 0 && testRow < intMazeX && testCol > 0 && testCol < intMazeY
                    && arrMaze[testRow, testCol] == 1)
                {
                    arrMaze[(paraRow + testRow) / 2, (paraCol + testCol) / 2] = 0;
                    MazePathing(testRow, testCol);
                }
            }
        }
        private void DeadEndRemove()
        {
            for (int row = 1; row < intMazeX - 1; row++)
            {
                for (int col = 1; col < intMazeY - 1; col++)
                {
                    int walls = 0;
                    //stores the cell opposite the single open cell within a deadend
                    Direction? dirOpenCell = null;

                    if (arrMaze[row - 1, col] == 1) { walls++; }
                    else { dirOpenCell = Direction.Down; }

                    if (arrMaze[row + 1, col] == 1) { walls++; }
                    else { dirOpenCell = Direction.Up; }

                    if (arrMaze[row, col - 1] == 1) { walls++; }
                    else { dirOpenCell = Direction.Right; }

                    if (arrMaze[row, col + 1] == 1) { walls++; }
                    else { dirOpenCell = Direction.Left; }

                    if (walls == 3)
                    {
                        switch (dirOpenCell)
                        {
                            case Direction.Left:
                                arrMaze[row, col - 1] = 0;
                                break;
                            case Direction.Right:
                                arrMaze[row, col + 1] = 0;
                                break;
                            case Direction.Up:
                                arrMaze[row - 1, col] = 0;
                                break;
                            case Direction.Down:
                                arrMaze[row + 1, col] = 0;
                                break;
                        }
                    }
                }
            }
        }
        private void BoundaryReadd()
        {
            for (int row = 0; row < intMazeX; row++)
            {
                arrMaze[row, 0] = 1;
            }
            for (int col = 0; col < intMazeY; col++)
            {
                arrMaze[intMazeX - 1, col] = 1;
            }
            for (int col = 0; col < intMazeY; col++)
            {
                arrMaze[0, col] = 1;
            }
            for (int row = 0; row < intMazeX; row++)
            {
                arrMaze[row, intMazeY - 1] = 1;
            }
        }

        private void PelletAdd()
        {
            for (int x = 0; x < intMazeX; x++)
            {
                for (int y = 0; y < intMazeY; y++)
                {
                    if (arrMaze[x, y] == 0)
                        arrMaze[x, y] = 2;                 
                }
            }
        }

        private void MazeCreate()
        {
            SetMazeValue();
            MazePathing(1, 1);
            DeadEndRemove();
            BoundaryReadd();
            PelletAdd();
            RandomBrushColours();
        }

        private void RandomBrushColours()
        {
            R = rnd.Next(150, 220);
            G = rnd.Next(50, 220);
            B = rnd.Next(50, 220);
            brush = new SolidBrush(Color.FromArgb(200, R, G, B));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            //loop through each cell in the maze array and draw walls and paths
            for (int row = 0; row < intMazeX; row++)
            {
                for (int col = 0; col < intMazeY; col++)
                {
                    if (arrMaze[row, col] == 1)
                        g.FillRectangle(brush, col * intCellSize, row * intCellSize, intCellSize, intCellSize);

                    else if (arrMaze[row, col] == 0)
                        g.FillRectangle(Brushes.Black, col * intCellSize, row * intCellSize, intCellSize, intCellSize);
                    //draw pellets for every 2 in the array
                    else if (arrMaze[row, col] == 2)
                    {
                        //empty space is black rectangle
                        g.FillRectangle(Brushes.Black, col * intCellSize, row * intCellSize, intCellSize, intCellSize);
                        //pellet is a small yellow circle in the center of the cell
                        g.FillEllipse(Brushes.Yellow, 
                            col * intCellSize + (intCellSize / 5 * 2), row * intCellSize + (intCellSize / 5 * 2), 
                            intCellSize / 5, intCellSize / 5);
                    }
                }
            }

            //paint player and ghosts afterwards in order to be on top of maze
            float MouthAngle = (MathF.Sin(fltMouthAngle * 3 + float.Pi / 6) + 0.9f) * 20;
            g.FillPie(Brushes.Yellow, rectPlayer, directionAngle[dirCurrent] + (MouthAngle), 360 - (2 * MouthAngle));
            foreach (Ghost ghost in listGhosts)
            {
                ghost.Draw(g);
            }
            lblScore.Text = "Score: " + Convert.ToString(intScore);
        }
        private void MovePlayer()
        {

            //movement attempt variables
            int tryX = intPlayerX;
            int tryY = intPlayerY;

            switch (dirNext)
            {
                //up and down
                case Direction.Up:
                    tryY -= intPlayerSpeed;
                    break;
                case Direction.Down:
                    tryY += intPlayerSpeed;
                    break;
                //left and right
                case Direction.Left:
                    tryX -= intPlayerSpeed;
                    break;
                case Direction.Right:
                    tryX += intPlayerSpeed;
                    break;
            }

            //checks if the new x or y is valid then sets
            //the players new coordinates and direction
            if (IsValidMove(tryX, tryY))
            {
                intPlayerX = tryX;
                intPlayerY = tryY;
                dirCurrent = dirNext;
                directionAngle[Direction.None] = directionAngle[dirCurrent];
                if(!swMouthTime.IsRunning)
                {
                    swMouthTime.Start();
                }
            }
            else
            {
                //reset test variables if they were invalid
                tryX = intPlayerX;
                tryY = intPlayerY;
                //continue going in the current direction 
                switch (dirCurrent)
                {
                    case Direction.Up:
                        tryY -= intPlayerSpeed;
                        break;
                    case Direction.Down:
                        tryY += intPlayerSpeed;
                        break;
                    case Direction.Left:
                        tryX -= intPlayerSpeed;
                        break;
                    case Direction.Right:
                        tryX += intPlayerSpeed;
                        break;
                }
                //checking for collision on current direction
                if (IsValidMove(tryX, tryY))
                {
                    intPlayerX = tryX;
                    intPlayerY = tryY;
                }
                else
                {
                    dirCurrent = Direction.None;
                    swMouthTime.Reset();
                }
            }
            //paint new player rectangle
            rectPlayer = new Rectangle(intPlayerX, intPlayerY, intCellSize, intCellSize);
            //force refresh
            Invalidate();
        }
        

        private bool IsValidMove(int newX, int newY)
        {
            //test rectangle for collision
            Rectangle rectNewPlayer = new Rectangle(newX, newY, rectPlayer.Width, rectPlayer.Height);

            //goes through every cell wall and
            //creates a rectangle for every one
            for (int row = 0; row < intMazeX; row++)
            {
                for (int col = 0; col < intMazeY; col++)
                {
                    if (arrMaze[row, col] == 1)
                    {
                        Rectangle mazeWall = new Rectangle(col * intCellSize, row * intCellSize, intCellSize, intCellSize);
                        //using IntersectsWith method to check for collision
                        //returning IsValidMove as false if the intersect is true
                        if (rectNewPlayer.IntersectsWith(mazeWall))
                        {
                            return false;
                        }
                    }
                    else if (arrMaze[row, col] == 2)
                    {
                        Rectangle pellet = new Rectangle(col * intCellSize + (intCellSize / 5 * 2),
                            row * intCellSize + (intCellSize / 5 * 2),
                            intCellSize / 5, intCellSize / 5);
                        //using IntersectsWith method to check for collision
                        //returning IsValidMove as false if the intersect is true
                        if (rectNewPlayer.IntersectsWith(pellet))
                        {
                            arrMaze[row, col] = 0;
                            intScore += 10;
                        }
                    }
                }
            }
            //checks for collision with newX and newY on each side
            //to make sure player can not go out of bounds at all
            if (newX < 0 || newY < 0 || newX + rectPlayer.Width > ClientSize.Width || newY + rectPlayer.Height > ClientSize.Height)
                return false;
            //if none of the checks are activated
            //then it is returned as a valid move
            return true;
        }
        //allows for an input of an entities current position
        //and then destination, returning the point of the next tile.
        private Point GetNextTileBFS(Point paraStart, Point paraEnd)
        {
            //stores visited cells as booleans in a 2 dimensional array
            //visited cells marked true
            bool[,] arrVisitedCells = new bool[intMazeX, intMazeY];

            //does the same but stores parent points
            //helps reconstruct paths
            Point[,] pntArrOrigin = new Point[intMazeX, intMazeY];

            //the queue for the travel of the fastest path
            Queue<Point> pntQueue = new Queue<Point>();


            //queues the start point then marks it as visited
            pntQueue.Enqueue(paraStart);
            arrVisitedCells[paraStart.Y, paraStart.X] = true;

            //defines the different directions using points, which
            //i can then use to add to the current position to check
            Point[] directions = { new Point(0, -1), new Point(0, 1), new Point(-1, 0), new Point(1, 0) };

            //loops until all directions are explored
            while (pntQueue.Count > 0)
            {
                //dequeue current point then check if the current point has reached the end
                Point pntCurrent = pntQueue.Dequeue();

                if (pntCurrent == paraEnd)
                {
                    //backtrack to find first step then
                    //follows the origin chain using this
                    Point pntStep = paraEnd;
                    while (pntArrOrigin[pntStep.Y, pntStep.X] != paraStart)
                    {
                        pntStep = pntArrOrigin[pntStep.Y, pntStep.X];
                    }
                    //returns first move
                    return pntStep;
                }
                //goes through each direction
                foreach (Point p in directions)
                {
                    //create test values using each points X and Y value(0, 1, -1)
                    int intStepX = pntCurrent.X + p.X;
                    int intStepY = pntCurrent.Y + p.Y;

                    //checks that the cell is not visited and is not a wall
                    if (!arrVisitedCells[intStepY, intStepX] && arrMaze[intStepY, intStepX] == 0)
                    {
                        //marks cell as visited
                        arrVisitedCells[intStepY, intStepX] = true;
                        //store current point as origin
                        pntArrOrigin[intStepY, intStepX] = pntCurrent;
                        //creates new point for exploration
                        pntQueue.Enqueue(new Point(intStepX, intStepY));
                    }
                }
            }
            //if path not found returns back to the start (no move)
            return paraStart;
        }

        private int BreadthDifference(int paraValueOne, int paraValueTwo)
        {
            //returns distance between greatest point and lowest point ensuring that
            //a positive value will be returned(due to neither coordinates being negative)
            if (paraValueOne > paraValueTwo)
                return paraValueOne - paraValueTwo;
            return paraValueTwo - paraValueOne;

        }

        private int MoveTowards(int current, int target, int speed)
        {
            //checks if current is more left/above target
            if (current < target)
            {
                int next = current + speed;

                if (next > target) return target;
                return next;
            }

            //checks if current is more right/below target
            else if (current > target)
            {
                int next = current - speed;

                if (next < target) return target;
                return next;
            }
            //otherwise the current position = target thus will remain
            return current;
        }
        private void MoveObjToTarget(Rectangle paraObject, Rectangle paraTarget, int paraPursuerSpeed, Point paraTargetPosition)
        {
            
            //calculate how far away enemy is in both X and Y coordinates
            if (BreadthDifference(paraObject.X, paraTargetPosition.X) < paraPursuerSpeed &&
                BreadthDifference(paraObject.Y, paraTargetPosition.Y) < paraPursuerSpeed)
            {
                //creates points for both enemy and player
                Point enemyCell = new Point(paraObject.X / intCellSize, paraObject.Y / intCellSize);
                Point playerCell = new Point(paraTarget.X / intCellSize, paraTarget.Y / intCellSize);

                //creates point for enemy to move to using current fastest path
                Point nextCell = GetNextTileBFS(enemyCell, playerCell);

                //sets target as the new cell, makes sure they can only move on snaps
                paraTargetPosition = new Point(nextCell.X * intCellSize, nextCell.Y * intCellSize);
            }

            //if nothings changes enemy continues moving towards player
            paraObject.X = MoveTowards(paraObject.X, paraTargetPosition.X, paraPursuerSpeed);
            paraObject.Y = MoveTowards(paraObject.Y, paraTargetPosition.Y, paraPursuerSpeed);
            
        }

        private void GameLoop()
        {
            MazeCreate();
            swMouthTime.Start();
            while (true)
            {
                MovePlayer();
                fltMouthAngle = (float)swMouthTime.Elapsed.TotalSeconds * 7;
                Thread.Sleep(20);
            }
        }
    }
}
