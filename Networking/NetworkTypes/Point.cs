namespace NetworkTypes
{
    public class Point : SerializableType
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point() { }
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
