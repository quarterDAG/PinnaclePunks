using Spine.Unity;
using Spine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst.Intrinsics;
using UnityEngine;
using Unity.VisualScripting;
using System;

public class Minion : MonoBehaviour, ICharacter
{

    [SerializeField] Bar hpBar;
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject bulletPrefab; // Bullet that the enemy will shoot
    [SerializeField] private Gradient bulletGradient;
    [SerializeField] private LayerMask obstacleLayers;

    [SerializeField] private int damage = 1;
    [SerializeField] float attackRange = 5f; // Range within which the enemy will start shooting
    [SerializeField] float fireRate = 1f; // How often the enemy shoots
    [SerializeField] Transform firePoint; // Point from where the bullet will be shot

    private List<Transform> players = new List<Transform>(); // List of all players
    private float timeToFire = 0;

    private SkeletonMecanim skeletonMecanim;
    private Animator animator;
    private bool isFlipped;
    public bool IsDead { get; private set; } = false;

    public event Action OnDeath;

    [SerializeField] private string tagToAttack;

    private Transform closestPlayer;
    private float checkPlayerInterval = 1.0f; // Check every second
    private float lastCheckTime = 0;


    [System.Serializable]
    public class MinionStates
    {
        public int Health = 99;
    }

    public MinionStates stats = new MinionStates();

    private void Start ()
    {
        animator = GetComponent<Animator>();
        skeletonMecanim = GetComponent<SkeletonMecanim>();
    }

    private void Update ()
    {
        if (IsDead) return;

        if (Time.time >= lastCheckTime + checkPlayerInterval)
        {
            lastCheckTime = Time.time;
            SearchPlayersFromEnemyTeam();
            closestPlayer = GetClosestPlayer();
        }

        if (closestPlayer != null)
        {
            HandleFlip();
            HandleRotation();

            float distanceToPlayer = Vector3.Distance(transform.position, closestPlayer.position);
            if (distanceToPlayer <= attackRange && Time.time >= timeToFire)
            {
                timeToFire = Time.time + 1 / fireRate;
                Shoot();
            }
        }
    }

    private void SearchPlayersFromEnemyTeam ()
    {
        GameObject[] otherTeamObjects = new GameObject[0];

        otherTeamObjects = GameObject.FindGameObjectsWithTag(tagToAttack);
        foreach (var otherTeamObject in otherTeamObjects)
        {
            Debug.Log(otherTeamObject.layer);

            if (otherTeamObject.layer == 9) // 9 = player layer
            {
                players.Add(otherTeamObject.transform);
                Debug.Log(players);
            }
        }
    }

    Transform GetClosestPlayer ()
    {
        Transform closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (Transform player in players)
        {

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer < closestDistance && ClearPathToPlayer(player))
            {
                closestDistance = distanceToPlayer;
                closestPlayer = player;
                Debug.Log(closestPlayer);
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

    private void HandleFlip ()
    {
        if (closestPlayer.position.x > transform.position.x && transform.localScale.x < 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
            isFlipped = false;
        }
        else if (closestPlayer.position.x < transform.position.x && transform.localScale.x > 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
            isFlipped = true;
        }
    }


    private void HandleRotation ()
    {

        Vector2 playerPosition = closestPlayer.position;
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



    public void TakeDamage ( int damage, int shooterIndex )
    {
        stats.Health -= damage;

        hpBar.UpdateValue(-damage);
        PlayerStatsManager.Instance.AddDamageToPlayerState(damage, shooterIndex);


        if (stats.Health <= 0)
        {
            Die(shooterIndex);
        }
    }

    public void Die ( int killerIndex )
    {
        if (!IsDead)
        {
            animator.SetBool("IsDead", true);
            OtherTeamInventory(tagToAttack).AddMinion();
            IsDead = true;
            OnDeath?.Invoke();

        }
    }

    public Inventory OtherTeamInventory ( string othereamTag )
    {
        Inventory[] inventories = FindObjectsOfType<Inventory>();

        foreach (Inventory inventory in inventories)
        {
            if (inventory.gameObject.tag == othereamTag)
            {
                return inventory;
            }
        }

        return null;
    }



    private async void Shoot ()
    {
        animator.SetBool("IsShooting", true);
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = bullet.GetComponent<Projectile>();
        projectile.SetTagToDamage(tagToAttack);
        projectile.SetBulletGradient(bulletGradient);
        projectile.SetDamage(damage);
        int animationDelay = (int)fireRate * 100;
        await Task.Delay(animationDelay);
        animator.SetBool("IsShooting", false);
    }

    public void SetTagToAttack ( string _tagToAttack )
    {
        tagToAttack = _tagToAttack;
    }

}
