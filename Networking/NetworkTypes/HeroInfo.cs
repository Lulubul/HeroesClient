
namespace NetworkTypes
{
    public class HeroInfo : SerializableType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Damage { get; set; }
        public RaceEnum RaceEnum { get; set; }
        public HeroType Type { get; set; }
    }
}
