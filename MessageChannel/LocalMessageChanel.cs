using GameActors;
using NetworkTypes;

namespace Events
{
    public class LocalMessageChannel : MessageChannel
    {
        public override void MovePiece(Point location, Point start, Point destination) { }
        public override void FinishAction() { }
        public override void DieCreature(CreatureComponent creatureComponent) { }
        public override void DefenseCreature(CreatureComponent creatureComponent) { }
        public override void AttackCreature(int index, int creatureIndex) { }
    }
}
