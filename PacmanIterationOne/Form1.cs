using System.Threading;
using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Diagnostics;
using pIterationOne;

namespace pIterationOne
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

        Dictionary<string, Color> GhostColors = new Dictionary<string, Color>
        {
            { "Blinky", Color.Red },
            { "Pinky", Color.Pink },
            { "Inky", Color.Cyan },
            { "Clyde", Color.Orange }
        };

        Dictionary<string, bool> ghostReleased = new Dictionary<string, bool>
{
            { "Blinky", false },
            { "Pinky", false },
            { "Inky", false },
            { "Clyde", false }
        };


        //declare empty integers to define using cellsize
        private int
            intPlayerX,
            intPlayerY,
            intPlayerSpeed,
            intMazeX,
            intMazeY,
            intScore,
            R,
            G,
            B,
            intCellSize = 40,
            intArrayOfStrLen = 60,
            intGhostPhaCount,
            intPlayerLives,
            intPelletCount,
            intDisposeCount;

        //define empty maze
        int[,] arrMaze;

        //mouth angle
        float fltMouthAngle = 0;
        float fltDeathAngle = 0;

        //guard for ghost list
        private readonly object ghostLock = new();


        //declare current and next direction variables
        Direction
            dirCurrent = Direction.None,
            dirNext = Direction.None;

        //declare booleans
        bool
            threadRunning = true,
            boolChase = false,
            boolGhosts = false,
            boolDeath = false,
            boolCollision = true,
            boolReady = false,
            restarted = false;

        //declare threads
        Thread
            thrdGameLoop,
            thrdGarbageDispose,
            thrdGhostPhases;

        //declare labels
        Label
            lblScore = new Label(),
            lblInterface = new Label();

        //declare rectangles
        Rectangle
            rectPlayer,
            rectSpawnPoint;

        //set up resources
        List<Ghost> listGhosts = new List<Ghost>();
        Brush brush = new SolidBrush(Color.FromArgb(200, 20, 20, 20));
        Stopwatch swMouthTime = new Stopwatch();
        Form Interface = new Form();
        Queue<string> interfaceStrings;
        Random rnd = new Random();

        //cancellation token to stop previous ghosts releasing
        private CancellationTokenSource ghostReleaseCTS;

        public Form1()
        {
            InitializeComponent();
            StartGame();
        }

        private void StartGame()
        {
            this.DoubleBuffered = true;
            interfaceStrings = new Queue<string>(intArrayOfStrLen);

            InitializeComponent();

            //choose random numbers for maze size
            intMazeX = rnd.Next(6, 8);
            intMazeY = rnd.Next(12, 16);

            //make sure maze dimensions are odd numbers in order for maze pathing
            intMazeX = intMazeX * 2 + 1;
            intMazeY = intMazeY * 2 + 1;

            //initialize maze array and form size
            arrMaze = new int[intMazeX, intMazeY];
            ClientSize = new Size(intMazeY * intCellSize, intMazeX * intCellSize);

            //initialize player position and speed
            intPlayerX = intCellSize;
            intPlayerY = intCellSize;
            intPlayerSpeed = intCellSize / 2;
            rectPlayer = new Rectangle(intPlayerX, intPlayerY, intCellSize, intCellSize);
            intPlayerLives = 3;

            ghostReleaseCTS = new CancellationTokenSource();

            ghostReleaseCTS = new CancellationTokenSource();

            //creating the label and setting attributes
            lblScore.Size = new Size(intCellSize * 10, intCellSize);
            lblScore.Font = new Font("Comic Sans MS", 20);
            lblScore.BackColor = Color.Transparent;
            this.Controls.Add(lblScore);
            lblScore.Location = new Point((intCellSize * intMazeY) - (lblScore.Width * 2), 0);

            //set point of form to right
            this.Location = new Point(Screen.FromControl(this).Bounds.Right - this.Width, 0);

            //interface set up
            Interface.Text = "Interface";
            Interface.Size = new Size(400, this.Size.Height);
            Interface.StartPosition = FormStartPosition.Manual;
            Interface.BackColor = Color.Black;
            Interface.ControlBox = false;
            Interface.FormBorderStyle = FormBorderStyle.None;
            Interface.Shown += (s, e) =>
            {
                Interface.Location = new Point(this.Left - Interface.Width, this.Top);
            };

            //setting label for interface to add text
            lblInterface = new Label();
            lblInterface.Location = new Point(0, 0);
            lblInterface.Size = new Size(Interface.ClientSize.Width, Interface.ClientSize.Height);
            lblInterface.Font = new Font("Consolas", 10);
            lblInterface.ForeColor = Color.Lime;
            Interface.Controls.Add(lblInterface);

            Interface.Show();
            this.Move += (s, e) =>
            {
                Interface.Location = new Point(this.Left - Interface.Width, this.Top);
            };

            //initialize threads 
            thrdGameLoop = new Thread(GameLoop);
            thrdGameLoop.Start();

            thrdGarbageDispose = new Thread(DisposeGarbage);
            thrdGarbageDispose.Start();

            thrdGhostPhases = new Thread(PhaseSwitch);
            thrdGhostPhases.Start();

            //bring and focus front
            this.BringToFront();
            this.Focus();
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            //exit threads for smooth shutdown
            threadRunning = false;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //focus and bring form to front for keyboard activation
            this.Focus();
            this.Select();
            BringFormToFront();
        }

        private void BringFormToFront()
        {
            this.Interface.TopMost = true;
            this.Interface.TopMost = false;
            this.TopMost = true;
            this.TopMost = false;
            this.Activate();
        }


        private void KeyDownEvent(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //movement up, down, left, right respectively with WASD
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

                //reset game button set to R
                case Keys.R:
                    ResetGame();
                    break;

                //arrow key support
                case Keys.Up:
                    dirNext = Direction.Up;
                    break;
                case Keys.Down:
                    dirNext = Direction.Down;
                    break;
                case Keys.Left:
                    dirNext = Direction.Left;
                    break;
                case Keys.Right:
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
            //mark current cell empty
            arrMaze[paraRow, paraCol] = 0;

            //create list of all possible directions (4-directional)
            List<Direction> mazeDirections = new List<Direction>
            {
                Direction.Up, Direction.Down, Direction.Left, Direction.Right
            };

            //Fischer Yates shuffle to ensure all recursions are randomly ordered
            for (int i = 0; i < mazeDirections.Count; i++)
            {
                int swapIndex = rnd.Next(i, mazeDirections.Count);
                var temp = mazeDirections[i];
                mazeDirections[i] = mazeDirections[swapIndex];
                mazeDirections[swapIndex] = temp;
            }

            //loops through each direction in list
            foreach (var direction in mazeDirections)
            {
                //creates values to check for out of bounds / already checked cells
                int testRow = paraRow;
                int testCol = paraCol;

                switch (direction)
                {
                    //sets test values according to current direction
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

                //checks if cell being looked at is valid and a wall
                if (testRow > 0 && testRow < intMazeX && testCol > 0 && testCol < intMazeY
                    && arrMaze[testRow, testCol] == 1)
                {
                    //continues if it is
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
                    //set walls check to 0
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

                    //checks for deadend
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
            //go through each dimension setting 0 values to 1
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
            //iterate through maze empty slots to set value to 2(pellets)
            for (int x = 0; x < intMazeX; x++)
            {
                for (int y = 0; y < intMazeY; y++)
                {
                    if (arrMaze[x, y] == 0)
                    {
                        arrMaze[x, y] = 2;
                        intPelletCount++;
                    }
                }
            }
            AddStringToQueue($"There are: {intPelletCount} pellets");
        }

        private void FindSuitableGhostSpawn()
        {
            int spawnX = intMazeX / 2 + 3;
            int spawnY = intMazeY / 2 + 3;
            bool suitableSpawn = false;
            while (!suitableSpawn)
            {
                if (arrMaze[spawnX, spawnY] == 0)
                {
                    arrMaze[spawnX, spawnY] = -1;
                    rectSpawnPoint = new Rectangle(spawnY * intCellSize, spawnX * intCellSize, intCellSize, intCellSize);
                    suitableSpawn = true;
                }
                else
                {
                    spawnX += 1;
                    spawnY += 1;
                }
            }

        }

        private void MazeCreate()
        {
            SetMazeValue();
            MazePathing(1, 1);
            DeadEndRemove();
            BoundaryReadd();
            FindSuitableGhostSpawn();
            PelletAdd();
            RandomBrushColours();
            AddStringToQueue($"Maze Generated: {intMazeX} x {intMazeY} at {DateTime.Now.ToLongTimeString()}");
        }

        private void RandomBrushColours()
        {
            R = rnd.Next(50, 220);
            G = rnd.Next(50, 220);
            B = rnd.Next(50, 220);
            AddStringToQueue($"Brush created with R {R} G {G} B {B} at {DateTime.Now.ToLongTimeString()}");
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
            g.FillRectangle(Brushes.Black, rectSpawnPoint);
            float mouthAngle;
            if (boolDeath)
            {
                //angles for death animation
                mouthAngle = fltDeathAngle;
            }
            else
            {
                //regular waka waka animation
                mouthAngle = (MathF.Sin(fltMouthAngle * 3 + float.Pi / 6) + 0.9f) * 20;
            }

            //draw pacman on top
            g.FillPie(Brushes.Yellow, rectPlayer, directionAngle[dirCurrent] + mouthAngle, 360 - (2 * mouthAngle));


            Ghost[] ghosts = [.. listGhosts];
            lock (ghostLock)
            {
                foreach (Ghost ghost in ghosts)
                {
                    ghost.DrawAsPacman(g, ghost.dirCurrent, mouthAngle);
                }
            }
            g.FillEllipse(Brushes.FloralWhite, rectSpawnPoint);
            lblScore.Text = "Score: " + Convert.ToString(intScore);
        }

        private void DeathAnimation()
        {
            while (fltDeathAngle < 180)
            {
                fltDeathAngle += 15;
                dirCurrent = Direction.Up;
                Invalidate();
                Thread.Sleep(150);
            }
            boolDeath = false;
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
            if (IsValidMove(tryX, tryY, rectPlayer, true))
            {
                intPlayerX = tryX;
                intPlayerY = tryY;
                dirCurrent = dirNext;
                directionAngle[Direction.None] = directionAngle[dirCurrent];
                if (!swMouthTime.IsRunning)
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
                if (IsValidMove(tryX, tryY, rectPlayer, true))
                {
                    intPlayerX = tryX;
                    intPlayerY = tryY;
                }
                else
                {
                    dirCurrent = Direction.None;
                    AddStringToQueue($"Player collision in ({tryX / intCellSize}, {tryY / intCellSize}) at {DateTime.Now.ToLongTimeString()}");
                    swMouthTime.Reset();
                }
            }
            //paint new player rectangle
            rectPlayer = new Rectangle(intPlayerX, intPlayerY, intCellSize, intCellSize);
            //force refresh
            Invalidate();
        }


        public bool IsValidMove(int newX, int newY, Rectangle pEntity, bool consumePellets)
        {
            //test rectangle for collision
            Rectangle rectNewEntity = new Rectangle(newX, newY, pEntity.Width, pEntity.Height);

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
                        if (rectNewEntity.IntersectsWith(mazeWall))
                        {
                            return false;
                        }
                    }
                    else if (arrMaze[row, col] == 2 && consumePellets)
                    {
                        Rectangle pellet = new Rectangle(col * intCellSize + (intCellSize / 5 * 2),
                            row * intCellSize + (intCellSize / 5 * 2),
                            intCellSize / 5, intCellSize / 5);
                        //using IntersectsWith method to check for collision
                        //returning IsValidMove as false if the intersect is true
                        if (rectNewEntity.IntersectsWith(pellet))
                        {
                            arrMaze[row, col] = 0;
                            intPelletCount--;
                            intScore += 10;
                        }
                    }
                }
            }
            //checks for collision with newX and newY on each side
            //to make sure player can not go out of bounds at all
            if (newX < 0 || newY < 0 || newX + pEntity.Width > ClientSize.Width || newY + pEntity.Height > ClientSize.Height)
                return false;

            //if none of the checks are activated
            //then it is returned as a valid move
            return true;
        }

        public bool GhostCanMoveTo(int newX, int newY, Rectangle rectGhost)
        {
            return IsValidMove(newX, newY, rectGhost, false);
        }

        private void MoveGhosts()
        {
            Point playerTile = new Point(intPlayerX / intCellSize, intPlayerY / intCellSize);

            Ghost[] ghosts = [.. listGhosts];
            lock (ghostLock)
            {
                foreach (Ghost ghost in ghosts)
                {
                    Point ghostTile = new Point(ghost.X / intCellSize, ghost.Y / intCellSize);

                    // Check if ghost is stuck (position didn't change)
                    bool stuck = (ghost.X == ghost.prevX && ghost.Y == ghost.prevY);

                    // Only pick a new tile if reached next tile or stuck
                    if (stuck)
                    {
                        ghost.nextTile = BFS.GetNextTileBFS(arrMaze, ghostTile, ghost.chasePoint);
                    }

                    int targetX = ghost.nextTile.X * intCellSize;
                    int targetY = ghost.nextTile.Y * intCellSize;

                    //save previous position
                    ghost.prevX = ghost.X;
                    ghost.prevY = ghost.Y;

                    //move toward target tile
                    if (Math.Abs(targetX - ghost.X) > Math.Abs(targetY - ghost.Y))
                    {
                        ghost.X += (int)(Math.Sign(targetX - ghost.X) * ghost.ghostSpeed);
                        if (!GhostCanMoveTo(ghost.X, ghost.Y, ghost.rectGhost))
                            ghost.X = ghost.prevX;
                    }
                    else
                    {
                        ghost.Y += (int)(Math.Sign(targetY - ghost.Y) * ghost.ghostSpeed);
                        if (!GhostCanMoveTo(ghost.X, ghost.Y, ghost.rectGhost))
                            ghost.Y = ghost.prevY;
                    }

                    if (ghost.X > ghost.prevX) ghost.dirCurrent = Direction.Right;
                    else if (ghost.X < ghost.prevX) ghost.dirCurrent = Direction.Left;
                    else if (ghost.Y > ghost.prevY) ghost.dirCurrent = Direction.Down;
                    else if (ghost.Y < ghost.prevY) ghost.dirCurrent = Direction.Up;

                    ghost.UpdateRectangle();
                }
            }
        }

        private void UpdateGhostChasePoints()
        {
            //adds player point for parameter passing
            Point playerTile = new(intPlayerX / intCellSize, intPlayerY / intCellSize);

            lock (ghostLock)
            {
                foreach (Ghost ghost in listGhosts)
                {
                    ghost.chasePoint = ghost.currPhase 
                        switch
                    {
                        //checks if ghost is in chase or scatter and gets relevant point
                        Ghost.Phases.Chase => GetChaseTarget(ghost, playerTile),
                        Ghost.Phases.Scatter => GetScatterTarget(ghost),

                        //else get current chase point
                        _ => ghost.chasePoint
                    };
                }
            }
        }

        private Point GetChaseTarget(Ghost ghost, Point playerTile)
        {
            return ghost.name switch
            {
                //sets chase points for the ghosts
                "Blinky" => playerTile,
                "Pinky" => OffsetTarget(playerTile, dirCurrent, 4),
                "Inky" => OffsetTarget(playerTile, dirCurrent, 2),
                "Clyde" => GetClydeTarget(ghost, playerTile),
                _ => playerTile
            };
        }

        private Point GetScatterTarget(Ghost ghost)
        {
            return ghost.name switch
            {
                "Blinky" => new Point(intMazeY - 3, 1),
                "Pinky" => new Point(1, 1),
                "Inky" => new Point(intMazeY - 2, intMazeX - 2),
                "Clyde" => new Point(intPlayerX / intCellSize, intPlayerY / intCellSize),
                //else new point (0, 0) to prevent errors (ensures target always has a value)
                _ => new Point(0, 0)
            };
        }

        private Point OffsetTarget(Point basePoint, Direction dir, int offset)
        {
            //sets and declares base to target(player)
            Point target = basePoint;
            switch (dir)
            {
                //adds or subtracts to axis respectively by offset amount
                case Direction.Up: target.Y -= offset; break;
                case Direction.Down: target.Y += offset; break;
                case Direction.Left: target.X -= offset; break;
                case Direction.Right: target.X += offset; break;
            }

            //ensure target is within bounds of array 
            target.X = Math.Clamp(target.X, 0, intMazeY - 1);
            target.Y = Math.Clamp(target.Y, 0, intMazeX - 1);

            //return the target
            return target;
        }

        private Point GetClydeTarget(Ghost ghost, Point playerTile)
        {
            int distX = Math.Abs(ghost.X / intCellSize - playerTile.X);
            int distY = Math.Abs(ghost.Y / intCellSize - playerTile.Y);

            //returns bottom left corner point if less than 8 away from player
            return (distX + distY > 8) ? playerTile : new Point(1, intMazeX - 2);
        }


        private void SwitchGhostPhase()
        {
            lock (ghostLock)
            {
                foreach (Ghost ghost in listGhosts)
                {
                    switch (ghost.currPhase)
                    {
                        case Ghost.Phases.Chase:
                            ghost.currPhase = Ghost.Phases.Scatter;
                            boolChase = false;
                            AddStringToQueue($"{ghost.name} phase is now " +
                                $"{ghost.currPhase} at {DateTime.Now.ToLongTimeString()}");
                            break;

                        case Ghost.Phases.Scatter:
                            ghost.currPhase = Ghost.Phases.Chase;
                            boolChase = true;
                            AddStringToQueue($"{ghost.name} phase is now " +
                                $"{ghost.currPhase} at {DateTime.Now.ToLongTimeString()}");
                            break;

                    }
                }
            }
        }

        public void AddStringToQueue(string pStr)
        {
            if (interfaceStrings.Count >= intArrayOfStrLen)
                interfaceStrings.Dequeue();
            interfaceStrings.Enqueue(pStr);
        }

        public void UpdateTerminal()
        {
            string combined = string.Join("\n", interfaceStrings.ToArray());

            try
            { Interface.Invoke(() => { lblInterface.Text = combined; }); }

            catch
            { return; }


        }

        public void DisposeGarbage()
        {
            while (threadRunning)
            {
                Thread.Sleep(1000);
                if (++intDisposeCount >= 7)
                {
                    GC.Collect();
                    intDisposeCount = 0;
                    AddStringToQueue($"Garbage Collected at {DateTime.Now.ToLongTimeString()}");
                }
            }
        }

        public void PhaseSwitch()
        {
            while (threadRunning)
            {
                Thread.Sleep(1000);
                intGhostPhaCount++;
                switch (boolChase)
                {
                    case true:
                        {
                            //count for how many seconds ghosts are in chase
                            if (intGhostPhaCount >= 7)
                            {
                                SwitchGhostPhase();
                                intGhostPhaCount = 0;
                            }
                        }
                        break;
                    case false:
                        {
                            //count for how many seconds ghosts are in scatter
                            if (intGhostPhaCount >= 15)
                            {
                                SwitchGhostPhase();
                                intGhostPhaCount = 0;
                            }
                        }
                        break;
                }

            }
        }

        private void TryRelease(string name)
        {
            //spawnned already
            if (ghostReleased[name]) return;

            listGhosts.Add(new Ghost(rectSpawnPoint.X, rectSpawnPoint.Y,
                GhostColors[name], arrMaze, intCellSize, name, this,
                new Point(1, 1), Ghost.Phases.Chase));

            //mark released
            ghostReleased[name] = true; 
        }


        private async void ReleaseGhosts(CancellationToken token)
        {
            //return if already attempted
            if (boolGhosts) return;

            //ghosts have attempted to release
            boolGhosts = true;

            try
            {
                await Task.Delay(2000, token); // blinky
                TryRelease("Blinky");

                await Task.Delay(3000, token); //pinky
                TryRelease("Pinky");

                await Task.Delay(5000, token); //inky
                TryRelease("Inky");

                await Task.Delay(7000, token); //clyde
                TryRelease("Clyde");
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }


        private void OriginalPos()
        {
            lock (ghostLock)
            {
                //clear ghosts and set keys to false for ghosts
                listGhosts.Clear();

                foreach (var key in ghostReleased.Keys.ToList())
                    ghostReleased[key] = false;
            }

            //cancel any ongoing release
            ghostReleaseCTS?.Cancel();
            ghostReleaseCTS = new CancellationTokenSource();

            //reset player attributes and create a new rectangle for them
            intPlayerX = intCellSize;
            intPlayerY = intCellSize;
            intPlayerSpeed = intCellSize / 8;
            dirCurrent = Direction.Right;
            rectPlayer = new Rectangle(intPlayerX, intPlayerY, intCellSize, intCellSize);

            //1.5 second delay
            ShowGetReady(1500);

            //release ghost sequence activation
            ReleaseGhosts(ghostReleaseCTS.Token);
        }

        private void GhostCollisionCheck()
        {
            lock (ghostLock)
            {
                foreach (Ghost ghost in listGhosts)
                {
                    //creates rectangle for each ghost
                    Rectangle rectGhost = new Rectangle(ghost.X, ghost.Y, intCellSize, intCellSize);
                    //check collision then set flag true
                    if (rectPlayer.IntersectsWith(rectGhost))
                    {
                        AddStringToQueue($"Collision with {ghost.name} at {DateTime.Now.ToLongTimeString()}");
                        AddStringToQueue($"Lives are now {intPlayerLives}");
                        boolDeath = true;
                        break;
                    }
                }
            }
            //kill player after foreach to prevent list modification during loop
            if (boolDeath)
                PlayerDeath();
        }

        private void PlayerDeath()
        {
            //death = true to play animation and turn off
            //collision so only one life is removed
            boolDeath = true;
            boolCollision = false;
            fltDeathAngle = 0;
            swMouthTime.Reset();

            //play animation
            DeathAnimation();

            //remove life and check to see if game over
            if (--intPlayerLives <= 0)
            {
                ResetGame();
                return;
            }

            //reset original positions and make death false
            //for normal animation and add back collision
            OriginalPos();
            boolDeath = false;
            boolCollision = true;
            //allow ghosts to respawn
            boolGhosts = false;
        }



        private void ResetGame()
        {
            if (!restarted)
            {
                //ensures restart only occurs once
                restarted = true;

                //restarts the whole of the form
                Application.Restart();

                //focuses and emphasises the form giving keyboard control
                this.BringToFront();
                this.Focus();
            }
        }

        private void SpawnGhosts()
        {
            //checks if the list is empty and also checks to see that the procedure is not called
            if (listGhosts.Count == 0 && !boolGhosts)
            {
                ReleaseGhosts(ghostReleaseCTS.Token);
            }
        }

        private void ShowGetReady(int milliseconds)
        {
            //freeze movement
            boolReady = false;
            AddStringToQueue("GET READY!");
            
            //force redraw
            Invalidate();

            //block movement for this period
            Thread.Sleep(milliseconds);

            // unfreeze movement
            boolReady = true; 
        }


        private void GameLoop()
        {
            MazeCreate();
            //1.5 second delay
            ShowGetReady(1500);

            while (threadRunning)
            {           
                MovePlayer();
                SpawnGhosts();
                MoveGhosts();
                if (boolCollision) GhostCollisionCheck();
                UpdateGhostChasePoints();
                lock (ghostLock)
                {
                    foreach (Ghost ghost in listGhosts)
                    {
                        if (ghost.X / intCellSize == ghost.chasePoint.X && ghost.Y / intCellSize == ghost.chasePoint.Y
                        && ghost.currPhase == Ghost.Phases.Scatter)
                        {
                            ghost.currPhase = Ghost.Phases.Chase;
                            AddStringToQueue($"{ghost.name} reached their scatter corner, phase is now " +
                                $"{ghost.currPhase} at {DateTime.Now.ToLongTimeString()}");
                        }
                    }
                }
                UpdateTerminal();
                fltMouthAngle = (float)swMouthTime.Elapsed.TotalSeconds * 7;
                if (intPelletCount <= 150)
                {
                    //death = true to play animation and turn off
                    //collision so only one life is removed
                    boolDeath = true;
                    boolCollision = false;
                    fltDeathAngle = 0;
                    swMouthTime.Reset();

                    intPelletCount = 0;
                    MazeCreate();

                    //play animation
                    DeathAnimation();

                    //reset original positions and make death false
                    //for normal animation and add back collision
                    OriginalPos();
                    boolDeath = false;
                    boolCollision = true;
                    //allow ghosts to respawn
                    boolGhosts = false;
                }
                Thread.Sleep(20);
                Invalidate();
            }
            return;
        }
    }
}