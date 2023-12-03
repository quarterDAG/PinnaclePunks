public interface IWeapon
{
    void HandleAttack ();
    void CanUse ( bool _canUse );

    void IncreaseFireRate ( float fireRateMultiplier, float duration );

    void IncreaseFireDamage ( float fireDamageMultiplier, float duration );

    void Attack ();

    void StopAttack ();
}
