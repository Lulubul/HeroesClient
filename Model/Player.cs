using System.Collections.Generic;
using GameActors;
using UnityEngine;

namespace Model
{
    public abstract class Player
    {
        public List<Hero> Heroes;
        public List<Castle> Castles;
        public Color Color;
        public string Name { get; set; }
        public int Id { get; set; }
        public int Slot;

        protected Player() { }
        protected Player(string name, int id)
        { 
            Name = name;
            Id = id;
        }
        protected Player(List<Hero> heroes, List<Castle> castles, Color color, string name, int id) {}

        public virtual void AddCastle() {}
        public virtual void RemoveCastle() {}
        public virtual void AddMine() { }
        public virtual void RemoveMine() { }
    }
}
