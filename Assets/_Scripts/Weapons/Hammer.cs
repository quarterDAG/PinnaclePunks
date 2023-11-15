using System.Threading.Tasks;
using UnityEngine;

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
    [SerializeField] private float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;
    private InputManager inputManager;

    void Awake ()
    {
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

        if (canAttack && Time.time >= nextAttackTime)
            HandleAttack();
    }

    public void HandleAttack ()
    {
        if (inputManager.IsAttackPressed)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown; // Set the next attack time
        }
    }

    private async void Attack ()
    {
        playerAnimator.HammerAnimation(true);
        await Task.Delay(350);
        PerformHit();
        playerAnimator.HammerAnimation(false);
    }

    private void PerformHit ()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(hitPoint.position, attackRange, whatToHit);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.gameObject.CompareTag(damageThisTag)) // Replace with the appropriate tag
            {
                // Implement damage logic here
                Debug.Log("Hit " + enemy.name);
                enemy.GetComponent<ICharacter>().TakeDamage(damage, ownerIndex);
            }
        }
    }

    public void CanUse ( bool _canUse )
    {
        canAttack = _canUse;
    }

    // Optional: Visualize attack range in the editor
    private void OnDrawGizmosSelected ()
    {
        if (hitPoint == null)
            return;

        Gizmos.DrawWireSphere(hitPoint.position, attackRange);
    }
}
