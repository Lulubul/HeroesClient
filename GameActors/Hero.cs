using System.Collections.Generic;
using NetworkTypes;
using UnityEngine;

namespace GameActors
{
    public class Hero : MonoBehaviour
    {
        public AbstractHero Attributes;
        public RaceEnum RaceEnum;
        public HeroType Type;
        public List<Spell> Spells;
        public List<GameObject> InstanceCreatures;
        public List<Artifact> Artifacts;

        public void ThrowMagic()
        {

        }

        public void ThrowAttack()
        {

        }
    }
}
