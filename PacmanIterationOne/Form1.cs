using System.Threading;
using System;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace PacmanIterationOne
{
    public partial class Form1 : Form
    {
        //define empty maze
        int[,] arrMaze;

        //declare empty integers to define using cellsize
        private static int
            intPlayerX,
            intPlayerY,
            intPlayerSpeed,
            intMazeX,
            intMazeY,
            intCellSize = 40;

        //define different directions as named constants in enum
        enum Direction
        {
            Up,
            Down,
            Left,
            Right,
            None
        }

        //declare current and next direction variables
        private static Direction 
            dirCurrent = Direction.None,
            dirNext = Direction.None;

        Random rnd = new Random();

        //create system resources
        Thread thrdGameLoop;
        Rectangle rectPlayer;

        public Form1()
        {
            InitializeComponent();

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

            //create and start game loop thread
            thrdGameLoop = new Thread(GameLoop);
            thrdGameLoop.Start();

            //attach keydown event handler and add double buffering for smoother rendering
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
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

        private void MazeCreate()
        {
            //maze generation logic to be implemented
            SetMazeValue();
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
                        g.FillRectangle(Brushes.Blue, col * intCellSize, row * intCellSize, intCellSize, intCellSize);
                    else
                        g.FillRectangle(Brushes.Black, col * intCellSize, row * intCellSize, intCellSize, intCellSize);
                }
            }
            //paint player afterwards in order to be on top of maze
            g.FillRectangle(Brushes.Yellow, rectPlayer);
        }

        private void GameLoop()
        {
            MazeCreate();
            MessageBox.Show("GameLoop thread is running!");
        }
    }
}
