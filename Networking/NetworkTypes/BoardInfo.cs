
namespace NetworkTypes
{
    public class BoardInfo : SerializableType
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double Spacing { get; set; }
        public double HexWidth { get; set; }
    }
}
