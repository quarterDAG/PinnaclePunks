
using System.Threading.Tasks;
using UnityEngine;

public class ThrowSlowBombs : MonoBehaviour, IWeapon
{
    [SerializeField] private Transform player;
    private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private MouseAim mouseAim;
    [SerializeField] private LayerMask whatToHit;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bombPrefab;

    [SerializeField] private float fireRate = 1;

    [SerializeField] private float effectSpawnRate = 1;
    private float timeToSpawnEffect = 0;
    private float timeToFire = 0;

    private bool canShoot = true;
    private InputManager inputManager;


    void Awake ()
    {
        playerController = GetComponentInParent<PlayerController>();
        inputManager = GetComponentInParent<InputManager>();
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

        if (inputManager.IsSecondaryPressed && Time.time > timeToFire)
        {
            timeToFire = Time.time + 1 / fireRate;
            Throw();
        }

    }

    private async void Throw ()
    {
        if (Time.unscaledTime >= timeToSpawnEffect)
        {
            playerAnimator.ThrowAnimation(true);
            await Task.Delay(500);
            CreateShot();
            timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
            playerAnimator.ThrowAnimation(false);
        }
    }

    private void CreateShot ()
    {
        Instantiate(bombPrefab, firePoint.position, firePoint.rotation);
    }


    public void CanUse ( bool _canUse )
    {
        canShoot = _canUse;
    }


}
