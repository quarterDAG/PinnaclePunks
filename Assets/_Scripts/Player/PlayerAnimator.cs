using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private MouseAim mouseAim;
    private PlayerController playerController;


    private Rigidbody2D rb;
    [SerializeField] private Transform aim;
    [SerializeField] private SpriteRenderer slashFX;

    public bool isFlipped { get; private set; }
    private bool isHit = false;

    private Color transparent;

    private void Awake ()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        playerController = GetComponentInParent<PlayerController>();
        animator = GetComponent<Animator>();
        mouseAim = playerController.GetComponentInChildren<MouseAim>();

        if (slashFX != null)
            transparent = slashFX.color;
    }


    private void Update ()
    {
        if (playerController.isDead) return;

        HandleFlip();
        HandleRunningAnimation();
    }

    public async void GetHitAnimation ()
    {

        if (!isHit && animator != null)
        {
            isHit = true;
            animator.SetBool("GetHit", true);

            await Task.Delay(1000);

            animator.SetBool("GetHit", false);
            isHit = false;
        }

    }


    private void HandleFlip ()
    {
        if (aim.position.x > transform.position.x && transform.localScale.x < 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
            isFlipped = false;
        }
        else if (aim.position.x < transform.position.x && transform.localScale.x > 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
            isFlipped = true;
        }
    }


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

    public void JumpAnimation ( bool _isJumping )
    {
        animator.SetBool("IsJumping", _isJumping);
    }


    public void ShootAnimation ( bool _isShooting )
    {
        animator.SetBool("IsShooting", _isShooting);

        if (slashFX != null)
        {
            if (_isShooting)
                slashFX.color = Color.white;
            else
                slashFX.color = transparent;
        }
    }

    public void ThrowAnimation ( bool _isThrowing )
    {
        animator.SetBool("IsThrowing", _isThrowing);

    }


    public void DashAnimation ( bool _isDashing )
    {
        animator.SetBool("IsDashing", _isDashing);
    }

    public void DeathAnimation ( bool _isDead )
    {
        animator.SetBool("IsDead", _isDead);
    }

    public void FallAnimation ( bool _isFalling )
    {
        animator.SetBool("IsFalling", _isFalling);
    }


}
