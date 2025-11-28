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
        //define empty maze
        int[,] arrMaze;

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
            intDisposeCount;

        //declare current and next direction variables
        Direction
            dirCurrent = Direction.None,
            dirNext = Direction.None;

        Random rnd = new Random();

        //create system resources

        Thread thrdGameLoop;
        Thread thrdGarbageDispose;
        Thread thrdGhostPhases;
        Rectangle rectPlayer;
        Rectangle rectSpawnPoint;

        List<Ghost> listGhosts = new List<Ghost>();
        Brush brush = new SolidBrush(Color.FromArgb(200, 20, 20, 20));

        float fltMouthAngle = 0;

        Stopwatch swMouthTime = new Stopwatch();
        Label lblScore = new Label();
        bool threadRunning = true;
        bool boolChase = false;
        Label lblInterface;
        Form Interface = new Form();
        Queue<string> interfaceStrings;


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
            //ResetGame();
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
            intPlayerLives = 3;
            

            //creating the label and setting attributes
            lblScore.Location = new Point(ClientSize.Width - lblScore.Width * 2, 0);
            lblScore.Size = new Size(intCellSize * 10, intCellSize);
            lblScore.Font = new Font("Comic Sans MS", 20);
            lblScore.BackColor = Color.Transparent;
            this.Controls.Add(lblScore);

            //create and start game loop thread



            this.Location = new Point(Screen.FromControl(this).Bounds.Right - this.Width, 0);

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

            thrdGameLoop = new Thread(GameLoop);
            thrdGameLoop.Start();

            thrdGarbageDispose = new Thread(DisposeGarbage);
            thrdGarbageDispose.Start();

            thrdGhostPhases = new Thread(PhaseSwitch);
            thrdGhostPhases.Start();

            this.BringToFront();
            this.Focus();
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            threadRunning = false;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
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

                case Keys.R:
                    ResetGame();
                    break;

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
                    if (arrMaze[row, col] == -1)
                        return;
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

        private void FindSuitableGhostSpawn()
        {
            int spawnX = intMazeX / 2 - 1;
            int spawnY = intMazeY / 2 + 1;
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
            float MouthAngle = (MathF.Sin(fltMouthAngle * 3 + float.Pi / 6) + 0.9f) * 20;
            g.FillPie(Brushes.Yellow, rectPlayer, directionAngle[dirCurrent] + (MouthAngle), 360 - (2 * MouthAngle));

            Ghost[] ghosts = [.. listGhosts];
            foreach (Ghost ghost in ghosts)
            {
                ghost.Draw(g);
            }
            g.FillEllipse(Brushes.FloralWhite, rectSpawnPoint);
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



        private int BreadthDifference(int paraValueOne, int paraValueTwo)
        {
            //returns distance between greatest point and lowest point ensuring that
            //a positive value will be returned(due to neither coordinates being negative)
            if (paraValueOne > paraValueTwo)
                return paraValueOne - paraValueTwo;
            return paraValueTwo - paraValueOne;

        }


        private void MoveGhosts()
        {
            Point playerTile = new Point(intPlayerX / intCellSize, intPlayerY / intCellSize);

            Ghost[] ghosts = [.. listGhosts];
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

                // Save previous position
                ghost.prevX = ghost.X;
                ghost.prevY = ghost.Y;

                // Move toward target tile
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

                // Update direction
                if (ghost.X < targetX) ghost.dirCurrent = Direction.Right;
                else if (ghost.X > targetX) ghost.dirCurrent = Direction.Left;
                else if (ghost.Y < targetY) ghost.dirCurrent = Direction.Down;
                else if (ghost.Y > targetY) ghost.dirCurrent = Direction.Up;
                else ghost.dirCurrent = Direction.None;
                ghost.UpdateRectangle();
            }
        }

        private void UpdateGhostChasePoints()
        {
            // Player's tile
            Point playerTile = new Point(intPlayerX / intCellSize, intPlayerY / intCellSize);
            foreach (Ghost ghost in listGhosts)
            {
                switch (ghost.currPhase)
                {
                    case Ghost.Phases.Chase:
                        switch (ghost.name)
                        {
                            case "Blinky":
                                ghost.chasePoint = playerTile;
                                break;

                            case "Pinky":
                                Point pinkyTarget = playerTile;
                                switch (dirCurrent)
                                {
                                    case Direction.Up: pinkyTarget.Y -= 4; break;
                                    case Direction.Down: pinkyTarget.Y += 4; break;
                                    case Direction.Left: pinkyTarget.X -= 4; break;
                                    case Direction.Right: pinkyTarget.X += 4; break;
                                }
                                pinkyTarget.X = Math.Clamp(pinkyTarget.X, 0, intMazeY - 1);
                                pinkyTarget.Y = Math.Clamp(pinkyTarget.Y, 0, intMazeX - 1);
                                ghost.chasePoint = pinkyTarget;
                                break;

                            case "Inky":
                                Point InkyTarget = playerTile;
                                switch (dirCurrent)
                                {
                                    case Direction.Up: InkyTarget.Y -= 2; break;
                                    case Direction.Down: InkyTarget.Y += 2; break;
                                    case Direction.Left: InkyTarget.X -= 2; break;
                                    case Direction.Right: InkyTarget.X += 2; break;
                                }
                                InkyTarget.X = Math.Clamp(InkyTarget.X, 0, intMazeY - 1);
                                InkyTarget.Y = Math.Clamp(InkyTarget.Y, 0, intMazeX - 1);
                                ghost.chasePoint = InkyTarget;
                                break;

                            case "Clyde":
                                int distX = Math.Abs(ghost.X / intCellSize - playerTile.X);
                                int distY = Math.Abs(ghost.Y / intCellSize - playerTile.Y);
                                if (distX + distY > 8)
                                {
                                    ghost.chasePoint = playerTile;
                                }
                                else
                                {
                                    ghost.chasePoint = new Point(1, intMazeX - 2);
                                }
                                break;
                        }
                        break;
                    case Ghost.Phases.Scatter:
                        switch (ghost.name)
                        {
                            case "Blinky":
                                ghost.chasePoint = new Point(intMazeY - 3, 1);
                                break;

                            case "Pinky":
                                ghost.chasePoint = new Point(1, 1);
                                break;

                            case "Inky":
                                ghost.chasePoint = new Point(intMazeY - 2, intMazeX - 2);
                                break;

                            case "Clyde":
                                ghost.chasePoint = playerTile;
                                break;
                        }
                        break;

                }

            }
        }

        private void SwitchGhostPhase()
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
                if (++intDisposeCount >= 50)
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
                Thread.Sleep(100);
                intGhostPhaCount++;
                switch (boolChase)
                {
                    case true:
                        {
                            if (intGhostPhaCount >= 70)
                            {
                                SwitchGhostPhase();
                                intGhostPhaCount = 0;
                            }
                        }
                        break;
                    case false:
                        {
                            if (intGhostPhaCount >= 150)
                            {
                                SwitchGhostPhase();
                                intGhostPhaCount = 0;
                            }
                        }
                        break;
                }

            }
        }

        Dictionary<string, Color> GhostColors = new Dictionary<string, Color>
        {
            bool boolBlinky = false;
            bool boolPinky = false;
            bool boolInky = false;
            bool boolClyde = false;
            foreach (Ghost ghost in listGhosts)
            {
                if (ghost.name == "Blinky")
                {
                    boolBlinky = true;
                }
                if (ghost.name == "Pinky")
                {
                    boolPinky = true;
                }
                if (ghost.name == "Inky")
                {
                    boolInky = true;
                }
                if (ghost.name == "Clyde")
                {
                    boolClyde = true;
                }
            }

            await Task.Delay(2000); //blinky leaves
            if (!boolBlinky)
            {
                listGhosts.Add(new Ghost(rectSpawnPoint.X, rectSpawnPoint.Y, Color.Red, arrMaze, intCellSize, "Blinky", this, new Point(1, 1), Ghost.Phases.Chase));
                boolBlinky = false;
            }

            
            await Task.Delay(3000); //pinky leaves
            if (!boolPinky)
            { 
                listGhosts.Add(new Ghost(rectSpawnPoint.X, rectSpawnPoint.Y, Color.Pink, arrMaze, intCellSize, "Pinky", this, new Point(1, 1), Ghost.Phases.Chase));
                boolPinky = true;
            
            }
            
            await Task.Delay(5000); //inky leaves
            if (!boolInky)
            { 
                listGhosts.Add(new Ghost(rectSpawnPoint.X, rectSpawnPoint.Y, Color.Cyan, arrMaze, intCellSize, "Inky", this, new Point(1, 1), Ghost.Phases.Chase));
                boolInky = true;
            }
            
            await Task.Delay(7000); //clyde leaves
            if (!boolClyde)
                { 
                listGhosts.Add(new Ghost(rectSpawnPoint.X, rectSpawnPoint.Y, Color.Orange, arrMaze, intCellSize, "Clyde", this, new Point(1, 1), Ghost.Phases.Chase));
                boolClyde = true;
            }
            { "Blinky", Color.Red },
            { "Pinky", Color.Pink },
            { "Inky", Color.Cyan },
            { "Clyde", Color.Orange }
        };
        private void TryRelease(string name)
        {
            foreach (Ghost g in listGhosts)
                if (g.name == name) return;

            listGhosts.Add(new Ghost(rectSpawnPoint.X, rectSpawnPoint.Y, GhostColors[name], arrMaze, intCellSize, name, this, new Point(1, 1), Ghost.Phases.Chase));
        }

        private async void ReleaseGhosts()
        {
            await Task.Delay(2000); //blinky leaves
            TryRelease(GhostColors.Keys.ToArray()[0]);
            
            await Task.Delay(3000); //pinky leaves
            TryRelease(GhostColors.Keys.ToArray()[1]);

            await Task.Delay(5000); //inky leaves
            TryRelease(GhostColors.Keys.ToArray()[2]);

            await Task.Delay(7000); //clyde leaves
            TryRelease(GhostColors.Keys.ToArray()[3]);
        }

        private void OriginalPos()
        {
            listGhosts.Clear();
            ReleaseGhosts();
            intPlayerX = intCellSize;
            intPlayerY = intCellSize;
            intPlayerSpeed = intCellSize / 8;
            rectPlayer = new Rectangle(intPlayerX, intPlayerY, intCellSize, intCellSize);
        }
        bool boolDeath = false;

        private void GhostCollisionCheck()
        {
            foreach (Ghost ghost in listGhosts)
            {
                Rectangle rectGhost = new Rectangle(ghost.X, ghost.Y, intCellSize, intCellSize);
                if (rectPlayer.IntersectsWith(rectGhost))
                {
                    AddStringToQueue($"Collision with {ghost.name} at {DateTime.Now.ToLongTimeString()}");
                    AddStringToQueue($"Lives are now {intPlayerLives}");
                    boolDeath = true;
                    break;
                }
            }
            if (boolDeath)
                PlayerDeath();
        }

        private void PlayerDeath()
        {
            if (--intPlayerLives <= 0)
            {
                ResetGame();
            }
            else
            {
                OriginalPos();
                boolDeath = false;
            }
        }
        private bool restarted = false;
        private void ResetGame()
        {
            if (!restarted)
            {
                restarted = true;
                Application.Restart();
                this.BringToFront();
                this.Focus();
            }
        }


        private void GameLoop()
        {
            MazeCreate();
            swMouthTime.Start();
            while (threadRunning)
            {
                MovePlayer();
                ReleaseGhosts();
                MoveGhosts();
                GhostCollisionCheck();
                UpdateGhostChasePoints();
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
                UpdateTerminal();
                fltMouthAngle = (float)swMouthTime.Elapsed.TotalSeconds * 7;
                Thread.Sleep(20);
                Invalidate();
            }
            return;
        }        
    }
}