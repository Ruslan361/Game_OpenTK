using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple3DGame.Core.Utils
{
    public class MazeGenerator
    {
        private readonly Random rnd = new Random();

        private struct Point
        {
            public int X { get; }
            public int Y { get; }
            public Point(int x, int y) { X = x; Y = y; }
        }

        public int[,] GenerateMazeGrid(int height, int width)
        {
            if (width % 2 == 0 || height % 2 == 0)
            {
                // For this DFS algorithm to work nicely and create clear corridors and walls,
                // odd dimensions are preferred (e.g., so cells (1,1), (1,3) are paths, (1,2) is wall).
                // You can adjust this or handle even dimensions if necessary, but odd is typical.
                Console.WriteLine("Warning: MazeGenerator works best with odd dimensions for height and width.");
                // Optionally, adjust them: e.g., width = width % 2 == 0 ? width + 1 : width;
            }

            int[,] maze = new int[height, width];
            // Initialize grid with walls (1)
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    maze[r, c] = 1;
                }
            }

            Stack<Point> stack = new Stack<Point>();
            // Start carving from cell (1,1) - assuming 0-indexed grid, (0,0) is border wall.
            Point start = new Point(1, 1);
            maze[start.Y, start.X] = 0; // Mark as path
            stack.Push(start);

            while (stack.Count > 0)
            {
                Point current = stack.Peek();
                List<Point> unvisitedNeighbors = new List<Point>();

                // Define potential neighbors (2 cells away)
                // (dx, dy) represent the direction to the wall to carve, then to the next cell
                int[] dx = { 0, 0, 1, -1 }; // For wall: (0,1), (0,-1), (1,0), (-1,0)
                int[] dy = { 1, -1, 0, 0 }; // For cell: (0,2), (0,-2), (2,0), (-2,0)

                // Shuffle directions
                var shuffledIndices = Enumerable.Range(0, 4).OrderBy(x => rnd.Next()).ToArray();

                for(int i = 0; i < 4; i++)
                {
                    int randomIndex = shuffledIndices[i];
                    // Next potential cell is 2 steps away
                    int nextX = current.X + dx[randomIndex] * 2;
                    int nextY = current.Y + dy[randomIndex] * 2;

                    // Check bounds (ensure nextX/Y are within the actual maze, not border walls)
                    if (nextX > 0 && nextX < width -1 && nextY > 0 && nextY < height -1 && maze[nextY, nextX] == 1)
                    {
                        unvisitedNeighbors.Add(new Point(nextX, nextY));
                    }
                }

                if (unvisitedNeighbors.Count > 0)
                {
                    Point chosenNeighbor = unvisitedNeighbors[rnd.Next(unvisitedNeighbors.Count)];

                    // Carve wall between current and chosenNeighbor
                    int wallX = current.X + (chosenNeighbor.X - current.X) / 2;
                    int wallY = current.Y + (chosenNeighbor.Y - current.Y) / 2;
                    maze[wallY, wallX] = 0;

                    // Mark chosenNeighbor as path
                    maze[chosenNeighbor.Y, chosenNeighbor.X] = 0;

                    stack.Push(chosenNeighbor);
                }
                else
                {
                    stack.Pop(); // Backtrack
                }
            }
            return maze;
        }
    }
}
