using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class DashSkill : MonoBehaviour
{

    private InputManager inputManager;

    public enum DashType
    {
        Regular,
        Teleport
    }

    [Header("Dash Type Settings")]
    [SerializeField] private DashType dashType = DashType.Regular;
    [SerializeField] private Animator teleportEffect;
    private SpriteRenderer teleportSpriteRenderer;

    [SerializeField] float dashSpeed = 10f;
    [SerializeField] float dashDuration = 2f;
    private PlayerAnimator playerAnimator;
    private PlayerController playerController;
    private IWeapon weapon;
    private Rigidbody2D rb;


    [Header("Damage Settings")]
    [SerializeField] private bool isDamage;
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private int damage;
    private int playerIndex;
    private string teamTag;

    [SerializeField] private bool yAxisActivated;
    [SerializeField] private float manaCost = 10f;

    private AudioSource audioSource;

    [SerializeField] private AudioClip audioClip;

    private bool canDash = true;

    private void Awake ()
    {
        if (teleportEffect != null)
            teleportSpriteRenderer = teleportEffect.GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        weapon = GetComponentInChildren<IWeapon>();
        playerController = GetComponent<PlayerController>();
        inputManager = GetComponent<InputManager>();
    }

    private void Start ()
    {

        playerIndex = playerController.playerConfig.playerIndex;
        teamTag = gameObject.tag;
    }

    private void Update ()
    {
        if (playerController.isDead) return;

        if (canDash)
            HandleDash();
    }

    private void HandleDash ()
    {
        if (inputManager.IsDashPressed)
        {
            if (playerController.manaBar != null)
                if (playerController.manaBar.currentValue < manaCost) return;

            Dash(GetDashDirectionForGamepad());
            inputManager.ResetDash(); // Reset the dash press state
        }
    }


    private Vector2 GetDashDirectionForGamepad ()
    {
        // Assuming the dash direction is determined by the horizontal axis of the gamepad's left stick
        Vector2 gamepadDirection = inputManager.InputVelocity;
        return gamepadDirection;
    }

    private void Dash ( Vector2 direction )
    {
        switch (dashType)
        {
            case DashType.Regular:
                PerformRegularDash(direction);
                break;
            case DashType.Teleport:
                PerformTeleportDash(direction);
                break;
        }

        playerController.UpdateManaBar(-manaCost);
        canDash = false;
    }


    private void PerformRegularDash ( Vector2 direction )
    {
        // Existing dash logic
        Vector2 startDashPosition = rb.position;
        Vector2 targetDashPosition = CalculateTargetPosition(direction);

        StartCoroutine(DashMovementCoroutine(startDashPosition, targetDashPosition, dashDuration));

    }

    private Vector2 CalculateTargetPosition ( Vector2 direction )
    {
        Vector2 targetPosition = new Vector2(rb.position.x + (dashSpeed * direction.x), rb.position.y);
        if (yAxisActivated)
            targetPosition = new Vector2(rb.position.x + (dashSpeed * direction.x), rb.position.y + (dashSpeed * direction.y));
        return targetPosition;
    }

    private void PerformTeleportDash ( Vector2 direction )
    {
        Vector2 targetDashPosition = CalculateTargetPosition(direction);
        StartCoroutine(TeleportCoroutine(targetDashPosition));
    }

    private IEnumerator TeleportCoroutine ( Vector2 targetPosition )
    {
        audioSource.PlayOneShot(audioClip);
        teleportSpriteRenderer.enabled = true;
        teleportEffect.SetBool("Teleport", true);

        yield return new WaitForSeconds(0.1f);

        rb.position = targetPosition;
        gameObject.tag = teamTag;
        weapon.CanUse(true);
        playerAnimator.DashAnimation(false);

        yield return new WaitForSeconds(0.5f);
        teleportEffect.SetBool("Teleport", false);
        teleportSpriteRenderer.enabled = false;
        canDash = true;
    }


    private IEnumerator DashMovementCoroutine ( Vector2 start, Vector2 end, float duration )
    {
        audioSource.PlayOneShot(audioClip);
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

        yield return new WaitForSeconds(0.5f);
        playerAnimator.DashAnimation(false);
        canDash = true;
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
