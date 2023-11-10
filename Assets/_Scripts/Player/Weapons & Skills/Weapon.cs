
using UnityEngine;

public class Weapon : MonoBehaviour
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

    [SerializeField] private Bar slowmotionBar;
    [SerializeField] private float slowmotionIncrement = 5f;


    [SerializeField] private float fireRate = 0;
    [SerializeField] private int damage = 10;
    [SerializeField] private float shotDistance = 50f;

    [SerializeField] private float effectSpawnRate = 10;
    private float timeToSpawnEffect = 0;
    private float timeToFire = 0;

    private bool canShoot = true;
    private InputManager inputManager;

    private SlowmotionController slowmotionController;

    private int playerIndex;


    void Awake ()
    {
        playerController = GetComponentInParent<PlayerController>();
        inputManager = GetComponentInParent<InputManager>();
        slowmotionController = GetComponentInParent<SlowmotionController>();
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
            HandleFire();

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


    private void HandleFire ()
    {
        if (fireRate == 0)
        {
            if (inputManager.IsShootPressed)
                Shoot();
        }
        else
        {
            if (inputManager.IsShootPressed && Time.time > timeToFire)
            {
                timeToFire = Time.time + 1 / fireRate;
                Shoot();
            }
        }

        if (!inputManager.IsShootPressed)
            StopShoot();

    }

    private void Shoot ()
    {
        Vector2 aimPosition = mouseAim.GetAimPosition();
        Vector2 firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);
        RaycastHit2D hit = Physics2D.Raycast(firePointPosition, aimPosition - firePointPosition, shotDistance, whatToHit);

        if (Time.unscaledTime >= timeToSpawnEffect)
        {
            CreateShot();
            timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
        }
    }

    private void CreateShot ()
    {
        playerAnimator.ShootAnimation(true);
        GameObject bullet = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        MoveTrail moveTrail = bullet.GetComponent<MoveTrail>();
        moveTrail.SetTagToDamage(damageThisTag);
        moveTrail.SetPlayerOwnerIndex(playerIndex);
        //moveTrail.SetBulletGradient(bulletGradient);
        moveTrail.SetDamage(damage);

        IncreaseSlowmotionBar();

    }

    private void IncreaseSlowmotionBar ()
    {
        if (slowmotionBar == null)
        {
            slowmotionBar = slowmotionController.slowmotionBar;
        }
        else
        {
            slowmotionBar.UpdateValue(slowmotionIncrement);
        }
    }

    private void StopShoot ()
    {
        playerAnimator.ShootAnimation(false);
    }

    public void CanShoot ( bool _canShoot )
    {
        canShoot = _canShoot;
    }


}
