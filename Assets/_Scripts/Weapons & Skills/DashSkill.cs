using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DashSkill : MonoBehaviour
{
    private const float DOUBLE_CLICK_TIME = .2f;

    private InputManager inputManager;

    [SerializeField] float dashSpeed = 10f;
    [SerializeField] float dashDuration = 2f;
    private PlayerAnimator playerAnimator;
    private PlayerController playerController;
    private Weapon weapon;

    private bool isDashing;

    private Rigidbody2D rb;


    private int lastTapDirection = 0; // -1 for left, 1 for right


    private float lastClickTime;

    private void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        weapon = GetComponentInChildren<Weapon>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update ()
    {
        if (playerController.isDead) return;
        CheckForDoubleClickAndDash();
    }

    private void CheckForDoubleClickAndDash ()
    {

        if (!isDashing)
        {
            int currentTapDirection = 0;

            if (Input.GetButtonDown("Horizontal"))
                currentTapDirection = (int)Mathf.Sign(Input.GetAxisRaw("Horizontal"));

            if (currentTapDirection != 0)
            {
                float timeSinceLastClick = Time.time - lastClickTime;

                if (timeSinceLastClick <= DOUBLE_CLICK_TIME && currentTapDirection == lastTapDirection)
                {
                    Dash(currentTapDirection);
                    lastClickTime = 0;
                }
                else
                {
                    lastClickTime = Time.time;
                    lastTapDirection = currentTapDirection;
                }
            }
        }
    }

    private void Dash ( int direction )
    {
        Vector2 startDashPosition = rb.position;
        Vector2 targetDashPosition = new Vector2(rb.position.x + (dashSpeed * direction), rb.position.y);
        StartCoroutine(DashMovementCoroutine(startDashPosition, targetDashPosition, dashDuration));
    }

    private IEnumerator DashMovementCoroutine ( Vector2 start, Vector2 end, float duration )
    {
        playerAnimator.DashAnimation(true);

        gameObject.tag = "Dodge";
        weapon.CanShoot(false);

        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp(elapsed / duration, 0, 1);
            rb.position = Vector2.Lerp(start, end, normalizedTime);
            yield return null;
        }

        rb.position = end;
        //yield return new WaitForSeconds(0.1f); // Equivalent to await Task.Delay(200); but in coroutine context

        gameObject.tag = "Player";
        weapon.CanShoot(true);
        playerAnimator.DashAnimation(false);
    }




}
