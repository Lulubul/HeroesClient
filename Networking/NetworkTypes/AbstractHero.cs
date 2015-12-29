using System.Collections.Generic;
using GameActors;

namespace NetworkTypes
{
    public class AbstractHero : SerializableType
    {
        #region Fields
        public int Id { get; set; }
        public double Damage { get; set; }
        public string Name { get; set; }
        #endregion
        public RaceEnum RaceEnum { get; set; }
        public HeroType Type { get; set; }
        public Team HeroTeam { get; set; }
        public List<AbstractCreature> Creatures { get; set; }
        public AbstractHero()
        {
            Creatures = new List<AbstractCreature>();
        }
    }
}
