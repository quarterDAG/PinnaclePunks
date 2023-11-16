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
    private IWeapon weapon;
    private Rigidbody2D rb;

    private int lastTapDirection = 0; // -1 for left, 1 for right

    private float lastClickTime;

    [Header("Damage Settings")]
    [SerializeField] private bool isDamage;
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private int damage;
    private int playerIndex;
    private string teamTag;

    [SerializeField] private bool yAxisActivated;



    private void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        weapon = GetComponentInChildren<IWeapon>();
        playerController = GetComponent<PlayerController>();
        inputManager = GetComponent<InputManager>();

        playerIndex = playerController.playerConfig.playerIndex;
        teamTag = gameObject.tag;
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
        lastDashTime = Time.time; // Set the dash time after a successful dash
        Dash(inputManager.InputVelocity);
        lastClickTime = 0;
    }


    private Vector2 GetDashDirectionForGamepad ()
    {
        // Assuming the dash direction is determined by the horizontal axis of the gamepad's left stick
        Vector2 gamepadDirection = inputManager.InputVelocity;
        return gamepadDirection;
    }


    private void Dash ( Vector2 direction )
    {
        Vector2 startDashPosition = rb.position;
        Vector2 targetDashPosition = new Vector2(rb.position.x + (dashSpeed * direction.x), rb.position.y);

        if (yAxisActivated)
            targetDashPosition = new Vector2(rb.position.x + (dashSpeed * direction.x), rb.position.y + (dashSpeed * direction.y)); ;

        StartCoroutine(DashMovementCoroutine(startDashPosition, targetDashPosition, dashDuration));
    }

    private IEnumerator DashMovementCoroutine ( Vector2 start, Vector2 end, float duration )
    {
        playerAnimator.DashAnimation(true);

        gameObject.tag = "Dodge";
        weapon.CanUse(false);

        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp(elapsed / duration, 0, 1);
            Vector2 nextPosition = Vector2.Lerp(start, end, normalizedTime);


            if (isDamage)
            {
                CheckForCollisions(nextPosition);
            }

            rb.position = Vector2.Lerp(start, end, normalizedTime);
            yield return null;
        }

        rb.position = end;

        gameObject.tag = teamTag;
        weapon.CanUse(true);
        playerAnimator.DashAnimation(false);
    }

    private void CheckForCollisions ( Vector2 nextPosition )
    {
        // Check for collisions with a small area around the next position
        Collider2D[] hits = Physics2D.OverlapCircleAll(nextPosition, 0.1f, damageableLayers);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject) // Prevent self damage
            {
                hit.GetComponent<ICharacter>()?.TakeDamage(damage, playerIndex);
            }
        }
    }


}
