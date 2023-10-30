using System;
using System.Collections;
using System.Collections.Generic;
using TarodevController;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private MouseAim mouseAim;
    [SerializeField] private LayerMask whatToHit;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject arrowPrefab;
    //[SerializeField] private Gradient bulletGradient;
    [SerializeField] private string damageThisTag;


    [SerializeField] private float fireRate = 0;
    [SerializeField] private int damage = 10;
    [SerializeField] private float shotDistance = 50f;

    [SerializeField] private float effectSpawnRate = 10;
    private float timeToSpawnEffect = 0;
    private float timeToFire = 0;

    public event Action<bool> ShootEvent;



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

        if (Input.GetButtonUp("Fire1"))
        {
            StopShoot();
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
    }

    private void TrailEffect ()
    {
        playerAnimator.ShootAnimation(true);
        GameObject bullet = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        MoveTrail moveTrail = bullet.GetComponent<MoveTrail>();
        moveTrail.SetTagToDamage(damageThisTag);
        //moveTrail.SetBulletGradient(bulletGradient);
        moveTrail.SetDamage(damage);
    }

    private void StopShoot()
    {
        playerAnimator.ShootAnimation(false);

    }


}
