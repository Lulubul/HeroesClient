namespace NetworkTypes
{
    public class CreatureInfo : SerializableType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Damage { get; set; }
        public double Luck { get; set; }
        public double Range { get; set; }
        public int MaxHealth { get; set; }
        public int Speed { get; set; }
        public int Armor { get; set; }
        public CreatureType Type { get; set; }
        public RaceEnum RaceEnum { get; set; }
    }
}
