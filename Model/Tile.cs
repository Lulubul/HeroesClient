using System.Collections.Generic;
using System.Linq;
using NetworkTypes;
using PathFinding;

namespace Assets.Scripts.Model
{
    public class Tile : SpacialObject, IHasNeighbours<Tile>
    {
        public bool CanPass { get; set; }
        public bool CanSelect { get; set; }

        public IEnumerable<Tile> AllNeighbours { get; set; }
        public IEnumerable<Tile> Neighbours { 
            get { return AllNeighbours.Where(o => o.CanPass); } 
        }

        public Tile(int x, int y) : base(x, y)
        {
            CanPass = true;
            CanSelect = false;
        }

        public void FindNeighbours(Tile[,] gameBoard)
        {
            var neighbours = new List<Tile>();

            var possibleExits = Y % 2 == 0 ? EvenNeighbours : OddNeighbours;

            foreach (var vector in possibleExits)
            {
                var neighbourX = X + vector.X;
                var neighbourY = Y + vector.Y;

                if (neighbourX >= 0 && neighbourX < gameBoard.GetLength(0) && neighbourY >= 0 && neighbourY < gameBoard.GetLength(1))
                    neighbours.Add(gameBoard[neighbourX, neighbourY]);
            }

            AllNeighbours = neighbours;
        }

        public static List<Point> EvenNeighbours
        {
            get
            {
                return new List<Point>
                {
                    new Point(0, 1),
                    new Point(1, 1),
                    new Point(1, 0),
                    new Point(0, -1),
                    new Point(-1, 0),
                    new Point(1, -1),
                };
            }
        }

        public static List<Point> OddNeighbours
        {
            get
            {
                return new List<Point>
                {
                    new Point(0, 1),
                    new Point(1, 0),
                    new Point(-1, 1),
                    new Point(0, -1),
                    new Point(-1, 0),
                    new Point(-1, -1),
                };
            }
        }
    }
}
