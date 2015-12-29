using System;
using System.Collections.Generic;
using Assets.Scripts.Model;
using GameActors;
using NetworkTypes;

namespace Events
{
    public delegate void MovePieces(List<Point> points);
    public delegate void ChangeTurn(NextTurn turn);
    public delegate void DieCreature(Point points);
    public delegate void SyncHeroes(BoardInfo board, AbstractHero hero1, AbstractHero hero2);
    public delegate void FinishAction();
    public delegate void AttackCallback(AttackModel attackInfo);
    public abstract class MessageChannel
    {
        public MovePieces MoveCallback;
        public ChangeTurn TurnCallback;
        public SyncHeroes SyncHeroesCallback;
        public FinishAction GameIsReadyCallback;
        public AttackCallback Attack;
        public DieCreature DieCallback;

        public virtual void MovePiece(Point location, Point start, Point destination) { }
        public virtual void FinishAction() {}
        public virtual void DieCreature(CreatureComponent creature) { }
        public virtual void AttackCreature(int targetIndex, int creatureIndex) { }
        public abstract void DefenseCreature(CreatureComponent creature);
    }
}
