using Spine.Unity;
using UnityEngine;
using Spine;
using System.Collections.Generic;


public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SkeletonMecanim skeletonMecanim;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Weapon weapon;
    [SerializeField] private MouseAim mouseAim;
    [SerializeField] private bool isShooting;


    private Rigidbody2D rb;
    [SerializeField] private Transform aim; // Reference to the aim gameObject
    [SerializeField] private MeshRenderer meshRenderer; // Reference to the Spine's MeshRenderer

    private bool isFlipped = false;


    private float previousVerticalVelocity;


    private void Awake ()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController.GroundedChanged += OnGroundedChanged;
        weapon.ShootEvent += ShootAnimation;
    }

    private void OnDestroy ()
    {
        playerController.GroundedChanged -= OnGroundedChanged;
        weapon.ShootEvent -= ShootAnimation;
    }

    private void Update ()
    {

        HandleFlip();
        HandleRunningAnimation();

        HandleWeaponRotation();
        previousVerticalVelocity = rb.velocity.y;

    }

    private void LateUpdate ()
    {

    }


    // Flip the MeshRenderer based on aim position
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
            difference.y = -difference.y; // Invert the y-axis difference

        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        if (isFlipped)
            rotZ += 70;
        else
            rotZ -= 110;

        Skeleton skeleton = skeletonMecanim.skeleton;

        // List of bones you want to manipulate
       // List<string> weaponBone = new List<string> { "Hand_B", "Arrow" };

        foreach (Slot slot in skeleton.Slots)
        {

            if (slot.Bone.Data.Name.Contains("Hand_B"))
                slot.Bone.Rotation = rotZ;

            if (slot.Bone.Data.Name.Contains("Arrow"))
                slot.A = 0; // The last value (alpha) is set to 0 to make it transparent

            if (slot.Bone.Data.Name.Contains("Shade"))
                slot.A = 0;

        }
        skeleton.UpdateWorldTransform();

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

    public void ShootAnimation ( bool _isShooting )
    {
        animator.SetBool("IsShooting", _isShooting);
        isShooting = _isShooting;
    }


}
