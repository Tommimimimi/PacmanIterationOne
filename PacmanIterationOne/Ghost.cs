using pIterationOne;

public class Ghost
{
    public Dictionary<Direction, float> directionAngle = new()
        {
            { Direction.Up, 270 },
            { Direction.Down, 90 },
            { Direction.Left, 180 },
            { Direction.Right, 0 },
            { Direction.None, 0 }
        };

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
    public enum Phases
    {
        Chase,
        Scatter
    }
    public Phases currPhase;

    public Ghost(int startX, int startY, Color c, int[,] mazeArr, int cellSize,
        string name, Form1 f, Point initialChase, Phases pCurrPhase)
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
        this.currPhase = pCurrPhase;
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
    
    public void DrawAsPacman(Graphics g, Direction dir, float mouthAngle)
    {
        g.FillPie(new SolidBrush(color), rectGhost, directionAngle[dir] + mouthAngle + MathF.Sin(mouthAngle), 360 - (2 * mouthAngle) + MathF.Sin(mouthAngle));
    }
}
