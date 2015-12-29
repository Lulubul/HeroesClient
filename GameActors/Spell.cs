using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameActors
{
    public enum SpellType {Active, Passive};
    public class Spell
    {
        public SpellType Type;
        #region Fields
        public string Name { get; set; }
        #endregion

        public Spell(string name, SpellType type)
        {
            Name = name;
            Type = type;
        }


        public void Activate()
        {

        }
    }
}
