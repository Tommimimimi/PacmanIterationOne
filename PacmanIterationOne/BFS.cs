using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pIterationOne
{
    public static class BFS
    {
        //allows for an input of an entities current position
        //and then destination, returning the point of the next tile.
        public static Point GetNextTileBFS(int[,] pMaze, Point paraStart, Point paraEnd)
        {
            int rows = pMaze.GetLength(0);
            int cols = pMaze.GetLength(1);
            //stores visited cells as booleans in a 2 dimensional array
            //visited cells marked true
            bool[,] arrVisitedCells = new bool[rows, cols];

            //does the same but stores parent points
            //helps reconstruct paths
            Point[,] pntArrOrigin = new Point[rows, cols];

            //the queue for the travel of the fastest path
            Queue<Point> pntQueue = new Queue<Point>();


            //queues the start point then marks it as visited
            //note: since arrMaze is stored as [row, col] = [Y, X],
            //we use [paraStart.Y, paraStart.X]
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

                    //safety check in case start == end or no valid parent
                    if (pntStep == paraStart)
                        return paraStart;

                    //backtrack through parents until the first step after start
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

                    //check boundaries before accessing arrays
                    if (intStepX >= 0 && intStepX < cols && intStepY >= 0 && intStepY < rows)
                    {
                        //checks that the cell is not visited and is not a wall
                        //pellets (2) and empty space (0) are both traversable
                        if (!arrVisitedCells[intStepY, intStepX] && pMaze[intStepY, intStepX] != 1)
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
            }

            //if path not found returns back to the start (no move)
            return paraStart;
        }
    }
}
