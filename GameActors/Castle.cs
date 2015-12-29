using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTypes;

namespace GameActors
{
    public class Castle
    {
        public int Id { get; set; }
        public int Level { get; set; }

        public List<Build> Buildings;
        public List<AbstractCreature> Garrison;

        public Castle()
        {}
    }
}
