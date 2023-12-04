using Pathfinding;
using System.Collections;

using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Pathfinding")]
    [SerializeField] private Transform target;
    [SerializeField] private float activateDistance = 50f;
    [SerializeField] private float pathUpdateSeconds = 0.5f;
    [SerializeField] private float stopDistance = 2f;

    [Header("Physics")]
    [SerializeField] private float speed = 200f, jumpForce = 100f;
    [SerializeField] private float nextWaypointDistance = 3f;
    [SerializeField] private float jumpNodeHeightRequirement = 0.8f;
    [SerializeField] private float jumpModifier = 0.3f;
    [SerializeField] private float jumpCheckOffset = 0.1f;
    [SerializeField] private float groundedRaycastLength = 0.05f;
    [SerializeField] private Vector3 startOffset;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private LayerMask ignoreLayers;

    [Header("Custom Behavior")]
    [SerializeField] private bool followEnabled = true;
    [SerializeField] private bool jumpEnabled = true, isJumping;
    [SerializeField] private bool directionLookEnabled = true;

    [Header("Weapon")]
    [SerializeField] private IWeapon weapon;
    private Aim aim;


    private Path path;
    private int currentWaypoint = 0;
    private RaycastHit2D isGrounded;
    private Seeker seeker;
    private Rigidbody2D rb;
    private bool isOnCoolDown;

    public void Start ()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        weapon = GetComponentInChildren<IWeapon>();
        aim = GetComponentInChildren<Aim>();

        isJumping = false;
        isOnCoolDown = false;

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    private void FixedUpdate ()
    {
        if (TargetInDistance() && followEnabled)
        {
            PathFollow();
            GroundCheck();
        }
    }


    private void UpdatePath ()
    {
        if (followEnabled && TargetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }



    private void GroundCheck ()
    {
        // See if colliding with anything
        startOffset = transform.position - new Vector3(0f, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset, transform.position.z);
        isGrounded = Physics2D.Raycast(startOffset, -Vector3.up, groundedRaycastLength, groundLayer);

        Debug.DrawLine(startOffset, startOffset - new Vector3(0f, groundedRaycastLength, 0f), Color.red);
    }

    private void PathFollow ()
    {
        if (path == null) return;

        if (currentWaypoint >= path.vectorPath.Count) return;

        bool clearPath = ClearPathToTarget();

        // Shoot
        if (clearPath) weapon.Attack();
        else weapon.StopAttack();

        // Calculate horizontal distance to the target
        float horizontalDistanceToTarget = Mathf.Abs(transform.position.x - target.position.x);

        // Determine if AI should stop based on horizontal distance and clear path
        if (horizontalDistanceToTarget <= stopDistance && clearPath)
        {
            // Stop horizontal movement
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            Vector2 force = direction * speed;
            rb.velocity = new Vector2(force.x, rb.velocity.y);

            // Handle Jumping
            HandleJumping();
        }

        UpdateNextWaypoint();
    }

    private void HandleJumping ()
    {
        if (jumpEnabled && isGrounded && !isOnCoolDown)
        {
            float verticalDistanceToNextWaypoint = path.vectorPath[currentWaypoint].y - transform.position.y;
            if (verticalDistanceToNextWaypoint > jumpNodeHeightRequirement)
            {
                isJumping = true;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                StartCoroutine(JumpCoolDown());
            }
        }
        if (isGrounded)
        {
            isJumping = false;
        }
    }

    private void UpdateDirectionGraphics ( Vector2 _targetDirection )
    {

        if (_targetDirection.x <= 0)
        {
            transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

    }

    private void UpdateNextWaypoint ()
    {
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    public bool ClearPathToTarget ()
    {
        Vector2 aiPosition = new Vector2(transform.position.x, transform.position.y + 1f);
        Vector2 targetPosition = new Vector2(target.position.x, target.position.y + 1f);
        Vector2 directionToPlayer = (targetPosition - aiPosition).normalized;

        float distanceToPlayer = Vector3.Distance(aiPosition, targetPosition);

        LayerMask raycastMask = obstacleLayers & ~ignoreLayers;


        RaycastHit2D hit = Physics2D.Raycast(aiPosition, directionToPlayer, distanceToPlayer, raycastMask);

        Debug.DrawLine(aiPosition, targetPosition, hit.collider != null ? Color.red : Color.green, 0.1f);

        UpdateDirectionGraphics(directionToPlayer);
        aim.UpdateAimPostion(directionToPlayer);

        return hit.collider == null || hit.collider.transform == target;
    }


    private bool TargetInDistance ()
    {
        return Vector2.Distance(transform.position, target.transform.position) < activateDistance;
    }

    private void OnPathComplete ( Path p )
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    IEnumerator JumpCoolDown ()
    {
        isOnCoolDown = true;
        yield return new WaitForSeconds(1f);
        isOnCoolDown = false;
    }




}
