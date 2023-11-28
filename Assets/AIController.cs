using UnityEngine;

public class AIController : MonoBehaviour
{
    public float detectionRadius = 10f;
    private PlayerController playerController;
    private Transform currentTarget;
    private string enemyTeamTag;
    private Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float stopDistance = 2f;

    public float jumpProbability = 0.1f; // Probability of jumping each frame
    public float minJumpInterval = 1f; // Minimum time interval between jumps

    private float lastJumpTime = 0;




    void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        if (gameObject.CompareTag("TeamA"))
        {
            enemyTeamTag = "TeamB";
        }
        else
        {
            enemyTeamTag = "TeamA";
        }
    }

    void Update ()
    {
        if (playerController.isDead) return;

        DetectEnemies();
        MoveTowardsTarget();
        AttackTarget();
    }

    void DetectEnemies ()
    {
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        Collider2D closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (var target in potentialTargets)
        {
            if (target.CompareTag(enemyTeamTag))
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = target;
                }
            }
        }

        currentTarget = closestEnemy != null ? closestEnemy.transform : null;
    }
    void MoveTowardsTarget ()
    {
        float velocityX = 0f;
        if (currentTarget != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);


            if (distanceToTarget > stopDistance)
            {
                // Calculate only the X-axis direction and velocity
                float directionX = Mathf.Sign(currentTarget.position.x - transform.position.x);
                velocityX = directionX * moveSpeed;

                // Jumping logic - Jump only if the target is higher than the AI
                if (currentTarget.position.y > transform.position.y)
                {
                    // Check time and probability for jumping
                    if (Time.time > lastJumpTime + minJumpInterval && Random.value < jumpProbability)
                    {
                        playerController.JumpTriggered(); // Trigger the jump
                        lastJumpTime = Time.time;
                    }
                }
            }
            else
            {
                // Stop horizontal movement, but keep vertical movement (like falling)
                velocityX = 0;
            }
        }
        else
        {
            // Stop horizontal movement, but keep vertical movement (like falling)
            velocityX = 0;
        }

        playerController.SetFrameVelocityX(velocityX);

    }



    void AttackTarget ()
    {
        // Implement logic to attack the currentTarget if within range
    }

    private void OnDrawGizmosSelected ()
    {
        // Display the detection radius in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
