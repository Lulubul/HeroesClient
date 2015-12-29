namespace Assets.Scripts.Model
{
    public interface IBattleBasicActions
    {
        void Move();
        void HeroAttack();
        void Attack();
        void Defense();
        void Die();
        void ReceiveDamage(float damage);
    }
}
