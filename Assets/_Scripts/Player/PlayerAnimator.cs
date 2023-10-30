using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;

    private Rigidbody2D rb;
    [SerializeField] private Transform aim; // Reference to the aim gameObject
    [SerializeField] private MeshRenderer meshRenderer; // Reference to the Spine's MeshRenderer

    private float previousVerticalVelocity;


    private void Awake ()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController.GroundedChanged += OnGroundedChanged;
    }

    private void OnDestroy ()
    {
        playerController.GroundedChanged -= OnGroundedChanged;
    }

    private void Update ()
    {
        HandleFlip();
        HandleRunningAnimation();

        HandleFallingAnimation();

        previousVerticalVelocity = rb.velocity.y;

    }

    private void HandleFallingAnimation ()
    {

       /* // If the player was jumping and is now moving downwards, set the falling state
        if (!animator.GetBool("IsGrounded") && previousVerticalVelocity >= 0 && rb.velocity.y < 0)
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", true);
        }

        previousVerticalVelocity = rb.velocity.y;*/
    }

    // Flip the MeshRenderer based on aim position
    private void HandleFlip ()
    {
        if (aim.position.x > transform.position.x && transform.localScale.x < 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
        }
        else if (aim.position.x < transform.position.x && transform.localScale.x > 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
        }
    }

    // Switch between idle and run animations based on player movement
    private void HandleRunningAnimation ()
    {
        if (Mathf.Abs(rb.velocity.x) > 0.1f) 
        {
            animator.SetBool("IsRunning", true);
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }
    }

    private void OnGroundedChanged ( bool isGroundedNow, float velocityY )
    {

        Debug.Log("GROUND CHANGED EVENT");

        if (isGroundedNow)
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
        }
        else if (previousVerticalVelocity > 0.1f) // Player was moving upwards in the previous frame
        {
            animator.SetBool("IsJumping", true);
        }
    }


}
