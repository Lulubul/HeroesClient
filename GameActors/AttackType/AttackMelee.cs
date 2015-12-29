using UnityEngine;
using GameActors;

namespace AttackType
{
    public class AttackMelee : AttackBehavior
    {
        public AttackMelee(double damage, GameObject attack)
        {
            Damage = damage;
            Range = 0.0f;
            AttackIcon = attack;
            Hit = false;
        }

        public override void Attack(int count, Vector3 position)
        {

        }

        public override void MoveProjectile()
        {
            Hit = true;
        }
    }
}
