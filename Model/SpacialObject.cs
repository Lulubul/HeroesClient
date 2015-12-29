using NetworkTypes;

namespace Assets.Scripts.Model
{

    public class SpacialObject : SerializableType
    {
        private Point _location;
        public Point Location
        {
            get { return _location; }
            set { X = value.X; Y = value.Y; _location = value; }
        }
        public int X { get; set; }
        public int Y { get; set; }

        public SpacialObject(int x, int y) : this(new Point(x, y))
        {
        }

        public SpacialObject(Point location)
        {
            Location = location;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", X, Y);
        }
    }
}
