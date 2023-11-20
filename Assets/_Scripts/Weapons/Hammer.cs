using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Hammer : MonoBehaviour, IWeapon
{
    [SerializeField] private Transform player;
    private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private LayerMask layerToHit;
    [SerializeField] private string damageThisTag;
    [SerializeField] private Transform hitPoint;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Melee Attack")]
    [SerializeField] private int meleeAttackDamage = 50;
    [SerializeField] private float meleeAttackRange = 2.5f;

    [Header("StoneSkill")]
    [SerializeField] private float stoneSkillRange = 15f;
    [SerializeField] private int stoneSkillDamage = 30;
    private Vector3 attackDirection;
    private Vector3 extendedEndPoint;

    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private float stoneSpacing = 0.5f; // Spacing between stones
    [SerializeField] private float secondsBetweenEachStone = 0.1f;
    
    
    private int ownerIndex;

    private bool canAttack = true;
    private InputManager inputManager;

    private AudioSource audioSource;

    [SerializeField] private AudioClip whoosh;
    [SerializeField] private AudioClip hammerHit;

    private bool isAttackInProgress = false;
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
        damageThisTag = (gameObject.tag == "TeamA") ? "TeamB" : "TeamA";
    }

    void Update ()
    {
        if (playerController.isDead) return;

        HandleAttack();
    }

    public void HandleAttack ()
    {
        if (canAttack && !isAttackInProgress)
        {
            if (inputManager.IsAttackPressed)
            {
                Attack();
            }
        }
    }


    private void Attack ()
    {
        if (isAttackInProgress) return;

        audioSource.PlayOneShot(whoosh);
        playerAnimator.HammerAnimation(true);

        if (playerController._grounded)
        {
            TriggerStoneSkill();
        }
        else
        {
            PerformMeleeAttack();
        }

        isAttackInProgress = true; 
        StartCoroutine(ResetAttackFlagAfterDelay());
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
        hitEnemies.Clear(); // Clear the HashSet at the start of each attack

        Collider2D[] detectedEnemies = Physics2D.OverlapCircleAll(hitPoint.position, meleeAttackRange, layerToHit);
        foreach (Collider2D enemyCollider in detectedEnemies)
        {
            if (!hitEnemies.Contains(enemyCollider) && enemyCollider.gameObject.CompareTag(damageThisTag))
            {
                ICharacter enemyCharacter = enemyCollider.GetComponent<ICharacter>();
                if (enemyCharacter != null)
                {
                    enemyCharacter.TakeDamage(meleeAttackDamage, ownerIndex);
                    hitEnemies.Add(enemyCollider); // Add to the HashSet to prevent multiple hits
                }

                audioSource.PlayOneShot(hammerHit); // Play hit sound for each enemy
            }
        }
        await Task.Delay(300);
        playerAnimator.HammerAnimation(false); // Stop the hammer animation
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

            StoneSkill skill = stone.GetComponent<StoneSkill>();
            skill.SetDamage(stoneSkillDamage);
            skill.SetShooterIndex(ownerIndex);
            skill.SetTagToDamage(damageThisTag);

            distanceCovered += stoneSpacing;
            yield return new WaitForSeconds(secondsBetweenEachStone);
        }

        audioSource.PlayOneShot(hammerHit);
        playerAnimator.HammerAnimation(false);


        // Hold the line when fully extended
        yield return new WaitForSeconds(0.25f);  // Adjust time as needed

        // Clear the line by destroying the stones
        foreach (var stone in spawnedStones)
        {
            Destroy(stone);
            yield return new WaitForSeconds(secondsBetweenEachStone); // Adjust delay as needed
        }

        isAttackInProgress = false;
    }


    private IEnumerator ResetAttackFlagAfterDelay ()
    {
        yield return new WaitForSeconds(attackCooldown); // Set a cooldown duration
        isAttackInProgress = false;
    }

    public void CanUse ( bool _canUse )
    {
        canAttack = _canUse;
    }


}
