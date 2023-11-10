using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class DashSkill : MonoBehaviour
{
    private const float DOUBLE_CLICK_TIME = .5f;
    private const float DASH_COOLDOWN = 1.0f; // Cooldown time in seconds
    private float lastDashTime = -DASH_COOLDOWN;

    private InputManager inputManager;

    [SerializeField] float dashSpeed = 10f;
    [SerializeField] float dashDuration = 2f;
    private PlayerAnimator playerAnimator;
    private PlayerController playerController;
    private Weapon weapon;

    //private bool isDashing;

    private Rigidbody2D rb;


    private int lastTapDirection = 0; // -1 for left, 1 for right


    private float lastClickTime;

    private void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        weapon = GetComponentInChildren<Weapon>();
        playerController = GetComponent<PlayerController>();
        inputManager = GetComponent<InputManager>();
    }

    private void Update ()
    {
        CheckForDoubleClickAndDash();
    }

    private void CheckForDoubleClickAndDash ()
    {
        if (playerController.isDead || Time.time < lastDashTime + DASH_COOLDOWN)
            return;

        if (inputManager.IsDashPressed)
        {
            Dash(GetDashDirectionForGamepad());
            lastDashTime = Time.time; // Set the dash time after a successful dash
            inputManager.ResetDash(); // Reset the dash press state
        }

        if (inputManager.currentControlScheme == "Keyboard")
        {
            HandleKeyboardDash();
        }
    }

    private void HandleKeyboardDash ()
    {
        if (inputManager.InputVelocity.x != 0)
        {
            int currentTapDirection = (int)Mathf.Sign(inputManager.InputVelocity.x);

            if (currentTapDirection != 0)
            {
                float timeSinceLastTap = Time.time - lastClickTime;

                if (timeSinceLastTap <= DOUBLE_CLICK_TIME && currentTapDirection == lastTapDirection)
                {
                    lastDashTime = Time.time; // Set the dash time after a successful dash
                    Dash(currentTapDirection);
                    lastClickTime = 0;
                    lastTapDirection = 0; // Reset the last tap direction after a successful dash
                }
                else
                {
                    lastClickTime = Time.time;
                    lastTapDirection = currentTapDirection;
                }
            }
        }
    }


    private int GetDashDirectionForGamepad ()
    {
        // Assuming the dash direction is determined by the horizontal axis of the gamepad's left stick
        float gamepadDirection = inputManager.InputVelocity.x;
        return (int)Mathf.Sign(gamepadDirection);
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

        gameObject.tag = "Player";
        weapon.CanShoot(true);
        playerAnimator.DashAnimation(false);
    }




}
