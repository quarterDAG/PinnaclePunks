public interface ICharacter
{
    void TakeDamage ( float damageAmount, int shooterIndex );
    void Die (int killerIndex);
    void Freeze ( float duration );

}
