using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private MouseAim mouseAim;
    [SerializeField] private LayerMask whatToHit;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform BulletTrailPrefab;

    [SerializeField] private float fireRate = 0;
    [SerializeField] private float damage = 10;
    [SerializeField] private float shotDistance = 50f;

    [SerializeField] private float effectSpawnRate = 10;
    private float timeToSpawnEffect = 0;
    private float timeToFire = 0;
    //[SerializeField] private int rotationOffset = 0;

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
        Vector2 mouseAimPosition = mouseAim.GetAimPosition();
        Vector2 weaponPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 difference = mouseAimPosition - weaponPosition;

        difference.Normalize();
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ /*+ rotationOffset*/);

        HandleFire();
        HandleRope();

    }

    private void HandleRope()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShootRope();
        }
    }

    private void ShootRope()
    {
        Vector2 aimPosition = mouseAim.GetAimPosition();
        Vector2 firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);
        RaycastHit2D hit = Physics2D.Raycast(firePointPosition, aimPosition - firePointPosition, ropeDistance);

        if (Time.time >= timeToSpawnRope)
        {
            InstantiateRope();
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

        if (Time.time >= timeToSpawnEffect)
        {
            TrailEffect();
            timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
        }

        Debug.DrawLine(firePointPosition, (aimPosition - firePointPosition) * 100, Color.cyan);
        if (hit.collider != null)
        {
            Debug.DrawLine(firePointPosition, hit.point, Color.red);
        }

    }

    private void TrailEffect ()
    {
        Instantiate(BulletTrailPrefab, firePoint.position, firePoint.rotation);
    }

    private void InstantiateRope()
    {
        DestroyCurrentRope();

        currentRope = Instantiate(ropePrefab, firePoint.position, firePoint.rotation);
     
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

    public bool ConnectedToRope ()
    {
        return currentRope.GetComponent<Rope>().IsLastLinkConnected();
    }
}
