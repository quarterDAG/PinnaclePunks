using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, ICharacter
{

    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject bulletPrefab; // Bullet that the enemy will shoot
    [SerializeField] private Gradient bulletGradient;
    [SerializeField] private LayerMask obstacleLayers;

    private string damageThisTag = "Player";

    [SerializeField] private int damage = 1;
    [SerializeField] float attackRange = 5f; // Range within which the enemy will start shooting
    [SerializeField] float fireRate = 1f; // How often the enemy shoots
    [SerializeField] Transform firePoint; // Point from where the bullet will be shot

    private List<Transform> players = new List<Transform>(); // List of all players
    private Transform targetPlayer; // The player currently being targeted
    private float timeToFire = 0;


    [System.Serializable]
    public class EnemyStates
    {
        public int Health = 100;
    }

    public EnemyStates stats = new EnemyStates();

    private void Start ()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (var playerObject in playerObjects)
        {
            players.Add(playerObject.transform);
        }
    }

    private void Update ()
    {

        targetPlayer = GetClosestPlayer();

        if (targetPlayer != null)
        {
            HandleRotation();

            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
            if (distanceToPlayer <= attackRange)
            {

                if (Time.time >= timeToFire)
                {
                    timeToFire = Time.time + 1 / fireRate;
                    Shoot();
                }
            }
        }
    }

    private void HandleRotation ()
    {

        Vector2 playerPosition = targetPlayer.position;
        Vector2 weaponPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 difference = playerPosition - weaponPosition;

        difference.Normalize();
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        weapon.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
    }

    Transform GetClosestPlayer ()
    {
        Transform closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (Transform player in players)
        {
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer < closestDistance && ClearPathToPlayer(player))
                {
                    closestDistance = distanceToPlayer;
                    closestPlayer = player;
                }
            }
        }

        return closestPlayer;
    }

    bool ClearPathToPlayer ( Transform player )
    {
        Vector2 directionToPlayer = (player.position - firePoint.position).normalized;
        float distanceToPlayer = Vector3.Distance(firePoint.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, directionToPlayer, distanceToPlayer, obstacleLayers);

        // If the raycast hit something in the obstacle layers before reaching the player, return false.
        if (hit.collider != null)
        {
            // If the raycast hits the player before hitting an obstacle, then there's a clear path.
            if (hit.collider.transform == player)
            {
                return true;
            }
            return false;
        }
        return true;
    }

    public void TakeDamage ( int damage )
    {
        stats.Health -= damage;

        hpBar.UpdateHPFillUI(stats.Health);

        if (stats.Health <= 0)
        {
            GameMaster.KillEnemy(this);
        }

    }

    void Shoot ()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        MoveTrail moveTrail = bullet.GetComponent<MoveTrail>();
        moveTrail.SetTagToDamage(damageThisTag);
        moveTrail.SetBulletGradient(bulletGradient);
        moveTrail.SetDamage(damage);
    }


}