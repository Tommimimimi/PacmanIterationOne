using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacmanIterationOne
{
    public class Ghost
    {
        public int intX { get; private set; }
        public int intY { get; private set; }
        public int intSpeed { get; private set; }
        public Color colGhost { get; private set; }
        public Rectangle bounds => new Rectangle(intX, intY, intCellSize, intCellSize);
        Direction dirGhostCurrent;
        public enum Phase
        { 
            Chase,                          
            Scatter,                            
            Frightened
        }
        public Phase phaGhostPhase { get; private set; }
        private int[,] arrMaze;
        private int intCellSize;

        public Ghost(int paraX, int paraY, Color paraColour, int[,] paraMaze, int paraCellSize)
        {
            intX = paraX;
            intY = paraY;
            colGhost = paraColour;
            arrMaze = paraMaze;
            intCellSize = paraCellSize;
            dirGhostCurrent = Direction.Left;
            phaGhostPhase = Phase.Chase;
        }
        public void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(colGhost), intX, intY, intCellSize, intCellSize);
        }
    }
}
