using Spine.Unity;
using UnityEngine;
using Spine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private SkeletonMecanim skeletonMecanim;
    private MouseAim mouseAim;
    private PlayerController playerController;


    private Rigidbody2D rb;
    [SerializeField] private Transform aim;
    [SerializeField] private MeshRenderer meshRenderer;

    private bool isFlipped = false;
    private bool isHit = false;

    private void Awake ()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        playerController = GetComponentInParent<PlayerController>();
        skeletonMecanim = GetComponent<SkeletonMecanim>();  
        animator = GetComponent<Animator>();
        mouseAim = playerController.GetComponentInChildren<MouseAim>();
    }


    private void Update ()
    {
        if (playerController.isDead) return;

        HandleFlip();
        HandleRunningAnimation();
        HandleWeaponRotation();
    }

    public async void GetHitAnimation ()
    {
        if (!isHit)
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

    private void HandleWeaponRotation ()
    {
        Vector2 mouseAimPosition = mouseAim.GetAimPosition();
        Vector2 weaponPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 difference = mouseAimPosition - weaponPosition;

        difference.Normalize();

        if (isFlipped)
            difference.y = -difference.y;

        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        if (isFlipped)
            rotZ += 70;
        else
            rotZ -= 110;

        Skeleton skeleton = skeletonMecanim.skeleton;

        foreach (Slot slot in skeleton.Slots)
        {

            if (slot.Bone.Data.Name.Contains("Hand_B"))
                slot.Bone.Rotation = rotZ;

            if (slot.Bone.Data.Name.Contains("Arrow"))
                slot.A = 0;

            if (slot.Bone.Data.Name.Contains("Shade"))
                slot.A = 0;

        }
        skeleton.UpdateWorldTransform();

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
    }

    public void DashAnimation ( bool _isDashing )
    {
        animator.SetBool("IsDashing", _isDashing);
    }

    public void DeathAnimation ( bool _isDead )
    {
        animator.SetBool("IsDead", _isDead);
    }


}
