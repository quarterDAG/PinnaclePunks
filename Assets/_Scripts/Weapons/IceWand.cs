
using UnityEngine;

public class IceWand : MonoBehaviour, IWeapon
{
    [SerializeField] private Transform player;
    private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private MouseAim mouseAim;
    [SerializeField] private LayerMask whatToHit;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject arrowPrefab;
    //[SerializeField] private Gradient bulletGradient;
    [SerializeField] private string damageThisTag;

    [SerializeField] private float fireRate = 0;
    [SerializeField] private int damage = 10;

    [SerializeField] private float effectSpawnRate = 10;
    private float timeToSpawnEffect = 0;
    private float timeToFire = 0;

    private bool canShoot = true;
    private InputManager inputManager;

    private int playerIndex;
    private AudioSource audioSource;

    [SerializeField] private AudioClip bowReleased;


    void Awake ()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponentInParent<PlayerController>();
        inputManager = GetComponentInParent<InputManager>();
    }

    private void Start ()
    {
        playerIndex = playerController.playerConfig.playerIndex;
        damageThisTag = (gameObject.tag == "TeamA") ? "TeamB" : "TeamA";
    }

    void Update ()
    {
        if (playerController.isDead) return;

        HandleRotation();

        if (canShoot)
            HandleAttack();

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
        if (fireRate == 0)
        {
            if (inputManager.IsAttackPressed)
                Shoot();
        }
        else
        {
            if (inputManager.IsAttackPressed && Time.time > timeToFire)
            {
                timeToFire = Time.time + 1 / fireRate;
                Shoot();
            }
        }

        if (!inputManager.IsAttackPressed)
            StopShoot();

    }

    private void Shoot ()
    {
        if (Time.unscaledTime >= timeToSpawnEffect)
        {
            playerAnimator.ShootAnimation(true);
            audioSource.PlayOneShot(bowReleased);
            CreateShot();
            timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
        }
    }

    private void CreateShot ()
    {
        GameObject bullet = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = bullet.GetComponent<Projectile>();
        projectile.SetTagToDamage(damageThisTag);
        projectile.SetPlayerOwnerIndex(playerIndex);
        //moveTrail.SetBulletGradient(bulletGradient);
        projectile.SetDamage(damage);
    }


    private void StopShoot ()
    {
        playerAnimator.ShootAnimation(false);
    }

    public void CanUse ( bool _canUse )
    {
        canShoot = _canUse;
    }


}
