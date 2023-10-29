using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private Transform player;
    [SerializeField] private MouseAim mouseAim;
    [SerializeField] private LayerMask whatToHit;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject BulletTrailPrefab;
    [SerializeField] private string damageThisTag;


    [SerializeField] private float fireRate = 0;
    //[SerializeField] private int damage = 10;
    [SerializeField] private float shotDistance = 50f;

    [SerializeField] private float effectSpawnRate = 10;
    private float timeToSpawnEffect = 0;
    private float timeToFire = 0;

    [SerializeField] private GameObject ropePrefab;
    private GameObject currentRope;
    [SerializeField] private float ropeDistance = 20f;
    [SerializeField] private float ropeSpawnRate = 10;
    private float timeToSpawnRope = 0;

    void Awake ()
    {
        if (firePoint == null)
        {
            Debug.LogError("No Fire Point");
        }
    }

    // Update is called once per frame
    void Update ()
    {
        HandleRotation();

        HandleFire();
        HandleRope();
        HandleSlowMotion();

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

    private void HandleRope ()
    {
        if (Input.GetButton("Fire2"))
        {
            if (mouseAim.IsShootable())
            {
                InstantiateRope();
            }
            else
                ShootRope();
        }
    }

    private void HandleSlowMotion ()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            timeManager.DoSlowMotion();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            timeManager.StopSlowMotion();
        }
    }

    private void ShootRope ()
    {
        Vector2 aimPosition = mouseAim.GetAimPosition();
        Vector2 firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);
        RaycastHit2D hit = Physics2D.Raycast(firePointPosition, aimPosition - firePointPosition, ropeDistance);

        if (Time.time >= timeToSpawnRope)
        {
            DestroyCurrentRope();

            currentRope = Instantiate(ropePrefab, firePoint.position, firePoint.rotation);

            PlayerRope playerRope = player.GetComponent<PlayerRope>();
            currentRope.GetComponent<Rope>().SetPlayerRope(playerRope);

            Rope ropeScript = currentRope.GetComponent<Rope>();

            timeToSpawnRope = Time.time + 1 / ropeSpawnRate;
        }

    }

    private void HandleFire ()
    {
        if (fireRate == 0)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }

        }
        else
        {
            if (Input.GetButton("Fire1") && Time.time > timeToFire)
            {
                timeToFire = Time.time + 1 / fireRate;
                Shoot();
            }
        }
    }

    private void Shoot ()
    {
        Vector2 aimPosition = mouseAim.GetAimPosition();
        Vector2 firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);
        RaycastHit2D hit = Physics2D.Raycast(firePointPosition, aimPosition - firePointPosition, shotDistance, whatToHit);

        if (Time.unscaledTime >= timeToSpawnEffect)
        {
            TrailEffect();
            timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
        }

     /*   Debug.DrawLine(firePointPosition, (aimPosition - firePointPosition) * 100, Color.cyan);
        if (hit.collider != null)
        {
            Debug.DrawLine(firePointPosition, hit.point, Color.red);
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.DamageEnemy(damage);
                Debug.Log("Damage Enemy");
            }
        }*/

    }

    private void TrailEffect ()
    {
        GameObject bullet = Instantiate(BulletTrailPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<MoveTrail>().SetTagToDamage(damageThisTag);

    }

    private void InstantiateRope ()
    {
        DestroyCurrentRope();

        Vector2 aimPosition = mouseAim.GetAimPosition();

        // Instantiate a new Rope prefab at the aim position
        currentRope = Instantiate(ropePrefab, aimPosition, Quaternion.identity);

        PlayerRope playerRope = player.GetComponent<PlayerRope>();
        currentRope.GetComponent<Rope>().SetPlayerRope(playerRope);

        Rope ropeScript = currentRope.GetComponent<Rope>();
    }


    public void DestroyCurrentRope ()
    {
        if (currentRope)
        {

            Destroy(currentRope);
            currentRope = null;
        }
    }
/*
    public bool ConnectedToRope ()
    {
        return currentRope.GetComponent<Rope>().IsLastLinkConnected();
    }*/
}
