using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Hammer : MonoBehaviour, IWeapon
{
    [SerializeField] private Transform player;
    private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private LayerMask whatToHit;
    [SerializeField] private string damageThisTag;
    [SerializeField] private Transform hitPoint;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int damage = 30;
    private int ownerIndex;

    private bool canAttack = true;
    private InputManager inputManager;

    private AudioSource audioSource;

    [SerializeField] private AudioClip whoosh;
    [SerializeField] private AudioClip hammerHit;

    private bool isAttackInProgress = false;

    private Vector3 attackDirection;
    private Vector3 extendedEndPoint;

    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private float stoneSpacing = 0.5f; // Spacing between stones
    [SerializeField] private float secondsBetweenEachStone = 0.1f;





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

        if (canAttack)
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

        Vector3 hitPointEnd = hitPoint.position;
        attackDirection = (hitPointEnd - player.position).normalized;
        extendedEndPoint = hitPointEnd + attackDirection * attackRange;

        isAttackInProgress = true;
        playerAnimator.HammerAnimation(true);
        audioSource.PlayOneShot(whoosh);
        StartCoroutine(DrawAndClearHitLine());
    }

    private IEnumerator DrawAndClearHitLine ()
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
            skill.SetDamage(damage);
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




    public void CanUse ( bool _canUse )
    {
        canAttack = _canUse;
    }


}
