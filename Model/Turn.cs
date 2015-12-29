using System.Collections.Generic;
using NetworkTypes;

namespace Assets.Scripts.Model
{
    public class Turn : SerializableType
    {
        public bool IsFirstHero { get; set; }
        public int CreatureIndex { get; set; }

        public Turn(bool isFirstHero, AbstractCreature creature)
        {
            IsFirstHero = isFirstHero;
            CreatureIndex = creature.Index;
        }
    }
}
