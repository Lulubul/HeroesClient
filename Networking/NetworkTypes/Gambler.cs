namespace NetworkTypes
{
    public class Gambler : SerializableType
    {
        public int Id { get; set; }
        public int Slot { get; set; }
        public string Name { get; set; }
        public string Response { get; set; }

        public Gambler()
        {
            Response = "Succed";
            Slot = 0;
        }
    }
}
