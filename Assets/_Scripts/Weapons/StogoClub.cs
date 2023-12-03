using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class StogoClub : MonoBehaviour, IWeapon
{
    [SerializeField] private Transform player;
    private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private LayerMask obsticleLayer;
    [SerializeField] private string damageThisTag;
    [SerializeField] private Transform hitPoint;
    //[SerializeField] private float attackCooldown = 0.5f;

    private float originalFireRate;
    [SerializeField] private float fireRate = 0;
    private float timeToFire = 0;

    [Header("Melee Attack")]
    private float originalMeleeAttackDamage;
    [SerializeField] private float meleeAttackDamage = 50;
    [SerializeField] private float meleeAttackRange = 2.5f;

    [Header("StoneSkill")]
    private float originalStoneSkillDamage;
    [SerializeField] private float stoneSkillRange = 15f;
    [SerializeField] private float stoneSkillDamage = 30;
    private Vector3 attackDirection;
    private Vector3 extendedEndPoint;

    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private float stoneSpacing = 0.5f;
    [SerializeField] private float secondsBetweenEachStone = 0.1f;


    private int ownerIndex;

    private bool canAttack = true;
    private InputManager inputManager;

    private AudioSource audioSource;

    [SerializeField] private AudioClip whoosh;
    [SerializeField] private AudioClip hammerHit;

    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();


    void Awake ()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponentInParent<PlayerController>();
        inputManager = GetComponentInParent<InputManager>();
    }

    private void Start ()
    {
        ownerIndex = playerController.playerConfig.playerIndex;
        SetDamageTag();
        originalFireRate = fireRate;
        originalMeleeAttackDamage = meleeAttackDamage;
        originalStoneSkillDamage = stoneSkillDamage;
    }

    void Update ()
    {
        if (playerController.isDead) return;

        HandleAttack();
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

    public void HandleAttack ()
    {
        if (canAttack)
        {
            if (inputManager.IsAttackPressed && Time.time > timeToFire)
            {
                timeToFire = Time.time + 1 / fireRate;
                Attack();
            }
        }
    }

    public void Attack ()
    {
        if (!canAttack) return;

        audioSource.PlayOneShot(whoosh);
        playerAnimator.ShootAnimation(true);

        if (playerController._grounded)
        {
            TriggerStoneSkill();
        }
        else
        {
            PerformMeleeAttack();
        }

    }

    public void StopAttack ()
    {

    }


    private void TriggerStoneSkill ()
    {
        // Existing logic to trigger StoneSkill
        Vector3 hitPointEnd = hitPoint.position;
        attackDirection = (hitPointEnd - player.position).normalized;
        extendedEndPoint = hitPointEnd + attackDirection * stoneSkillRange;

        StartCoroutine(StoneSkill());
    }

    private async void PerformMeleeAttack ()
    {
        hitEnemies.Clear();

        Collider2D[] detectedEnemies = Physics2D.OverlapCircleAll(hitPoint.position, meleeAttackRange, obsticleLayer);
        foreach (Collider2D enemyCollider in detectedEnemies)
        {
            if (!hitEnemies.Contains(enemyCollider) && enemyCollider.gameObject.CompareTag(damageThisTag))
            {
                ICharacter enemyCharacter = enemyCollider.GetComponent<ICharacter>();
                var _playerController = enemyCollider.GetComponent<PlayerController>();

                if (enemyCharacter != null)
                {
                    if (_playerController == null || _playerController != this.playerController)
                    {
                        enemyCharacter.TakeDamage(meleeAttackDamage, ownerIndex);
                        hitEnemies.Add(enemyCollider);
                    }
                }

                audioSource.PlayOneShot(hammerHit);
            }
        }
        await Task.Delay(300);
        playerAnimator.ShootAnimation(false);
    }



    private IEnumerator StoneSkill ()
    {
        Vector3 start = player.position;
        Vector3 direction = (extendedEndPoint - start).normalized;

        List<GameObject> spawnedStones = new List<GameObject>();
        float distanceCovered = 0f;
        float totalDistance = Vector3.Distance(start, extendedEndPoint);

        while (distanceCovered < totalDistance)
        {
            Vector3 spawnPoint = start + direction * distanceCovered;
            GameObject stone = Instantiate(stonePrefab, spawnPoint, Quaternion.identity);
            spawnedStones.Add(stone);

            Projectile projectile = stone.GetComponent<Projectile>();
            projectile.SetTagToDamage(damageThisTag);
            projectile.SetPlayerOwnerIndex(ownerIndex);
            projectile.SetPlayerController(playerController);
            //moveTrail.SetBulletGradient(bulletGradient);
            projectile.SetDamage(stoneSkillDamage);



            distanceCovered += stoneSpacing;
            yield return new WaitForSeconds(secondsBetweenEachStone);
        }

        audioSource.PlayOneShot(hammerHit);
        playerAnimator.ShootAnimation(false);


        // Hold the line when fully extended
        yield return new WaitForSeconds(0.25f);

        // Clear the line by destroying the stones
        foreach (var stone in spawnedStones)
        {
            Destroy(stone);
            yield return new WaitForSeconds(secondsBetweenEachStone);
        }
    }


    public void CanUse ( bool _canUse )
    {
        canAttack = _canUse;
    }


    public void IncreaseFireRate ( float fireRateValue, float effectTime )
    {
        StartCoroutine(IncreareFireRateCoroutine(fireRateValue, effectTime));
    }

    IEnumerator IncreareFireRateCoroutine ( float fireRateValue, float effectTime )
    {
        fireRate *= fireRateValue;

        yield return new WaitForSeconds(effectTime);

        fireRate = originalFireRate;
    }

    public void IncreaseFireDamage ( float fireDamageMultiplier, float duration )
    {
        StartCoroutine(IncreaseFireDamageCoroutine(fireDamageMultiplier, duration));
    }

    IEnumerator IncreaseFireDamageCoroutine ( float fireDamageMultiplier, float duration )
    {
        stoneSkillDamage *= fireDamageMultiplier;
        meleeAttackDamage *= fireDamageMultiplier;

        yield return new WaitForSeconds(duration);

        stoneSkillDamage = originalStoneSkillDamage;
        meleeAttackDamage = originalMeleeAttackDamage;
    }

}
