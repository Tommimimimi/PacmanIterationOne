using pIterationOne;

public class Ghost
{
    public int X;
    public int Y;
    public float ghostSpeed;
    public Rectangle rectGhost;
    public Color color;
    public string name;
    public Direction dirCurrent;
    public Point chasePoint;      
    public Point nextTile;        
    public int prevX, prevY;  
    private int[,] maze;
    private int cellSize;
    private Form1 form;

    public Ghost(int startX, int startY, Color c, int[,] mazeArr, int cellSize, string name, Form1 f, Point initialChase)
    {
        this.X = startX;
        this.Y = startY;
        this.prevX = startX;
        this.prevY = startY;
        this.color = c;
        this.name = name;
        this.maze = mazeArr;
        this.cellSize = cellSize;
        this.form = f;
        this.ghostSpeed = cellSize / 8f;
        this.rectGhost = new Rectangle(X, Y, cellSize, cellSize);
        this.chasePoint = initialChase;
        this.nextTile = new Point(X / cellSize, Y / cellSize);
        this.dirCurrent = Direction.None;
    }

    public void UpdateRectangle()
    {
        rectGhost.X = X;
        rectGhost.Y = Y;
        rectGhost.Width = cellSize;
        rectGhost.Height = cellSize;
    }

    public void Draw(Graphics g)
    {
        g.FillEllipse(new SolidBrush(color), rectGhost);
    }

    public void UpdateChasePoint(Point playerTile, Direction playerDir, List<Ghost> allGhosts)
    {
        switch (name)
        {
            case "blinky":
                chasePoint = playerTile;
                ghostSpeed = cellSize / 2f;
                break;

            case "Pinky":
                chasePoint = new Point(
                    playerTile.X + 4 * (playerDir == Direction.Right ? 1 : playerDir == Direction.Left ? -1 : 0),
                    playerTile.Y + 4 * (playerDir == Direction.Down ? 1 : playerDir == Direction.Up ? -1 : 0)
                );
                ghostSpeed = cellSize / 8f;
                break;

            case "Inky":
                var blinky = allGhosts.Find(g => g.name == "Blinky");
                Point vector = new Point(
                    playerTile.X - blinky.X / cellSize,
                    playerTile.Y - blinky.Y / cellSize
                );
                chasePoint = new Point(playerTile.X + vector.X * 2, playerTile.Y + vector.Y * 2);
                ghostSpeed = cellSize / 8f;
                break;

            case "Clyde":
                int distance = Math.Abs(X / cellSize - playerTile.X) + Math.Abs(Y / cellSize - playerTile.Y);
                if (distance > 8)
                    chasePoint = playerTile;
                else
                    chasePoint = new Point(1, maze.GetLength(0) - 2);
                ghostSpeed = cellSize / 8f;
                break;
        }

        chasePoint.X = Math.Max(1, Math.Min(chasePoint.X, maze.GetLength(1) - 2));
        chasePoint.Y = Math.Max(1, Math.Min(chasePoint.Y, maze.GetLength(0) - 2));
    }
}
