using Assets.Scripts.Model;

namespace NetworkTypes
{
    public class AbstractCreature : SerializableType
    {
        #region Fields
        public double Damage { get; set; }
        public double Luck { get; set; }
        public string Name { get; set; }
        public double Range { get; set; }
        public double Health { get; set; }
        public int MaxHealth { get; set; }
        public int Speed { get; set; }
        public int Count { get; set; }
        public int Armor { get; set; }
        #endregion
        public CreatureType Type { get; set; }
        public Team Team { get; set; }
        public CreatureStatus Status { get; set; }
        public int Index { get; set; }
        public Point Piece { get; set; }
    }
}
