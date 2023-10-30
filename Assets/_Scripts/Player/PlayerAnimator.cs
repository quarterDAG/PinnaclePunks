using Spine.Unity;
using UnityEngine;
using Spine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SkeletonMecanim skeletonMecanim;
    [SerializeField] private Weapon weapon;
    [SerializeField] private MouseAim mouseAim;


    private Rigidbody2D rb;
    [SerializeField] private Transform aim;
    [SerializeField] private MeshRenderer meshRenderer;

    private bool isFlipped = false;
    private bool isHit = false;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    private void Update ()
    {
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

    public void JumpAnimation ()
    {
        animator.SetBool("IsJumping", true);
    }

    public void Landed ()
    {
        animator.SetBool("IsJumping", false);
    }

    public void ShootAnimation ( bool _isShooting )
    {
        animator.SetBool("IsShooting", _isShooting);
    }


}
