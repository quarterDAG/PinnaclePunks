using Spine.Unity;
using Spine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst.Intrinsics;
using UnityEngine;
using Unity.VisualScripting;

public class Enemy : MonoBehaviour, ICharacter
{

    [SerializeField] Bar hpBar;
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

    private SkeletonMecanim skeletonMecanim;
    private Animator animator;
    private bool isFlipped;
    private bool isDead;


    [System.Serializable]
    public class EnemyStates
    {
        public int Health = 100;
    }

    public EnemyStates stats = new EnemyStates();

    private void Start ()
    {

        animator = GetComponent<Animator>();
        skeletonMecanim = GetComponent<SkeletonMecanim>();

        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (var playerObject in playerObjects)
        {
            players.Add(playerObject.transform);
        }
    }

    private void Update ()
    {
        if (isDead) return;
        SearchPlayerAndShoot();

    }
    void LateUpdate ()
    {

    }

    private void SearchPlayerAndShoot ()
    {
        targetPlayer = GetClosestPlayer();

        if (targetPlayer != null)
        {
            HandleFlip();
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

    private void HandleFlip ()
    {
        if (GetClosestPlayer().position.x > transform.position.x && transform.localScale.x < 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
            isFlipped = false;
        }
        else if (GetClosestPlayer().position.x < transform.position.x && transform.localScale.x > 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
            isFlipped = true;
        }
    }


    private void HandleRotation ()
    {

        Vector2 playerPosition = targetPlayer.position;
        Vector2 weaponPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 difference = playerPosition - weaponPosition;

        difference.Normalize();
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        //Rotate weapon's fire point
        weapon.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);

        //Rotate the weapon's bone (spine)
        if (isFlipped)
            rotZ += 70;
        else
            rotZ -= 110;

        Skeleton skeleton = skeletonMecanim.skeleton;

        foreach (Slot slot in skeleton.Slots)
        {

            if (slot.Bone.Data.Name.Contains("BackHand"))
            {
                slot.Bone.Rotation = rotZ;
            }

        }
        skeleton.UpdateWorldTransform();

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

        hpBar.UpdateValue(-damage);

        if (stats.Health <= 0)
        {
            //GameMaster.KillEnemy(this);
            animator.SetBool("IsDead", true);
            isDead = true;
        }

    }

    private async void Shoot ()
    {
        animator.SetBool("IsShooting", true);
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        MoveTrail moveTrail = bullet.GetComponent<MoveTrail>();
        moveTrail.SetTagToDamage(damageThisTag);
        moveTrail.SetBulletGradient(bulletGradient);
        moveTrail.SetDamage(damage);
        int animationDelay = (int)fireRate * 100;
        await Task.Delay(animationDelay);
        animator.SetBool("IsShooting", false);

    }


}
