using UnityEngine;

namespace AttackType
{
    public class AttackRange : AttackBehavior
    {
        private readonly GameObject _projectile;
        private float _startTime;
        private float _journeyLength;
        public readonly float ProjectileSpeed = 50.0f;
        private Vector3 _startMarker;
        private Vector3 _endMarker;
        private Vector3 _lastPosition;

        public AttackRange(double damage, double range, GameObject attack, GameObject arrow)
        {
            Damage = damage;
            
            Range = range;
            AttackIcon = attack;
            _projectile = arrow;
            Hit = false;
        }

        public override void Attack(Vector3 position)
        {
            _projectile.SetActive(true);
            _projectile.transform.position = position;
            _lastPosition = position;
            _startTime = Time.time;
            _startMarker = _projectile.transform.position;
            _endMarker = Target.transform.position;
            _journeyLength = Vector3.Distance(position, _endMarker);
        }

        public override void MoveProjectile()
        {
            var distCovered = (Time.time - _startTime) * ProjectileSpeed;
            var fracJourney = distCovered / _journeyLength;

            if (_startMarker != _startMarker || Target == null || _projectile.transform.position.x != _projectile.transform.position.x)
            {
                _projectile.transform.position = _lastPosition;
                _projectile.SetActive(false);
                Hit = true;
                return;
            }

            if (!_projectile.activeSelf)
            {
                _projectile.transform.position = _lastPosition;
                _projectile.SetActive(true);
            }

            _projectile.transform.position = Vector3.Lerp(_startMarker, _endMarker, fracJourney);

            if (!(Vector3.Distance(_projectile.transform.position, _endMarker) < 0.05)) return;
            _projectile.SetActive(false);
            Hit = true;
        }

    }
}
