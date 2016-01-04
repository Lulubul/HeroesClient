using UnityEngine;
using GameActors;

namespace AttackType
{
    public class AttackMelee : AttackBehavior
    {
       private readonly GameObject _projectile;
        private float _startTime;
        private float _journeyLength;
        public readonly float ProjectileSpeed = 50.0f;
        private Vector3 _startMarker;
        private Vector3 _endMarker;

        public AttackMelee(double damage, GameObject attack, GameObject arrow)
        {
            Damage = damage;
            AttackIcon = attack;
            _projectile = arrow;
            Hit = false;
        }

        public override void Attack(int count, Vector3 position)
        {
            _projectile.SetActive(true);
            _projectile.transform.position = position;
            _startTime = Time.time;
            _startMarker = _projectile.transform.position;
            _endMarker = Target.transform.position;
            _journeyLength = Vector3.Distance(position, _endMarker);
        }

        public override void MoveProjectile()
        {
            var distCovered = (Time.time - _startTime) * ProjectileSpeed;
            var fracJourney = distCovered / _journeyLength;
            _projectile.transform.position = Vector3.Lerp(_startMarker, _endMarker, fracJourney);

            if (!(Vector3.Distance(_projectile.transform.position, _endMarker) < 0.01)) return;
            _projectile.SetActive(false);
            Hit = true;
        }
    }
}
