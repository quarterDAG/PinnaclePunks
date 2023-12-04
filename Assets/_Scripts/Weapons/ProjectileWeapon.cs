
using System.Collections;
using UnityEngine;

public class ProjectileWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private Transform player;
    private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private Aim mouseAim;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject arrowPrefab;
    //[SerializeField] private Gradient bulletGradient;
    [SerializeField] private string damageThisTag;

    private float originalFireRate;
    [SerializeField] private float fireRate = 0;
    private float timeToFire = 0;

    private float originalDamage;
    [SerializeField] private float damage = 10;

    [SerializeField] private float effectSpawnRate = 10;
    private float timeToSpawnEffect = 0;

    private bool canShoot = true;
    private InputManager inputManager;

    private int playerIndex = -1;
    private AudioSource audioSource;

    [SerializeField] private AudioClip bowReleased;

    private EnemyAI enemyAI;



    void Awake ()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponentInParent<PlayerController>();
        inputManager = playerController.GetComponent<InputManager>();
        enemyAI = playerController.GetComponent<EnemyAI>();
    }

    private void Start ()
    {
        if (playerController != null)
            playerIndex = playerController.playerConfig.playerIndex;

        SetDamageTag();

        originalFireRate = fireRate;
        originalDamage = damage;
    }

    private void SetDamageTag ()
    {

        switch (gameObject.tag)
        {
            case "TeamA":
                damageThisTag = "TeamB";
                break;
            case "TeamB":
                damageThisTag = "TeamA";
                break;
            case "FreeForAll":
                damageThisTag = "FreeForAll";
                break;
        }
    }

    void Update ()
    {
        HandleRotation();

        if (playerController != null)
        {
            if (playerController.isDead) return;
            if (canShoot)
                HandleAttack();
        }

    }

    private void HandleRotation ()
    {
        Vector2 mouseAimPosition = mouseAim.GetAimPosition();
        Vector2 weaponPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 difference = mouseAimPosition - weaponPosition;

        difference.Normalize();
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
    }


    public void HandleAttack ()
    {
        if (inputManager != null)
        {
            if (inputManager.IsAttackPressed)
            {
                Attack();
            }

            if (!inputManager.IsAttackPressed)
                StopAttack();
        }
    }


    public void Attack ()
    {
        if (playerController.isDead) return;

        if (Time.time > timeToFire)
        {
            timeToFire = Time.time + 1 / fireRate;

            if (Time.unscaledTime >= timeToSpawnEffect)
            {
                playerAnimator.ShootAnimation(true);
                audioSource.PlayOneShot(bowReleased);
                CreateShot();
                timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
            }
        }
    }

    private void CreateShot ()
    {
        GameObject bullet = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = bullet.GetComponent<Projectile>();
        projectile.SetTagToDamage(damageThisTag);
        projectile.SetPlayerOwnerIndex(playerIndex);
        projectile.SetPlayerController(playerController);
        //moveTrail.SetBulletGradient(bulletGradient);
        projectile.SetDamage(damage);
    }


    public void StopAttack ()
    {
        playerAnimator.ShootAnimation(false);
    }

    public void CanUse ( bool _canUse )
    {
        canShoot = _canUse;
    }

    #region Power Ups

    public void IncreaseFireRate ( float fireRateMultiplier, float duration )
    {
        StartCoroutine(IncreareFireRateCoroutine(fireRateMultiplier, duration));
    }

    IEnumerator IncreareFireRateCoroutine ( float fireRateMultiplier, float duration )
    {
        fireRate *= fireRateMultiplier;

        yield return new WaitForSeconds(duration);

        fireRate = originalFireRate;
    }

    public void IncreaseFireDamage ( float fireDamageMultiplier, float duration )
    {
        StartCoroutine(IncreaseFireDamageCoroutine(fireDamageMultiplier, duration));
    }

    IEnumerator IncreaseFireDamageCoroutine ( float fireDamageMultiplier, float duration )
    {
        damage *= fireDamageMultiplier;

        yield return new WaitForSeconds(duration);

        damage = originalDamage;
    }

    #endregion
}
