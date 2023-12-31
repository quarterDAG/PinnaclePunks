using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour, IPlayerController, ICharacter
{
    [SerializeField] private ScriptableStats movementStates;

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    public FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    private Vector2 _colOffsetDefault;
    private float _currentSpeed;
    private Coroutine speedModifierCoroutine;

    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public event Action Hit;

    #endregion


    [System.Serializable]
    public class PlayerStates
    {
        public float Health = 99;
        public float MaxHealth = 99;
        public float Shield = 0;
        public float MaxShield = 99;
        public bool spawnWithShield;
        public float MaxMana = 100;
    }

    public PlayerStates stats = new PlayerStates();


    private float _time;

    private PlayerRope playerRope;

    [Header("Status Avatar")]
    [SerializeField] private LivesManager livesManager;
    [SerializeField] private Bar hpBar;
    [SerializeField] private Bar shieldBar;
    public Bar manaBar;

    public bool canMove { get; private set; }
    private float originalGravityScale;

    private PlayerAnimator playerAnimator;
    public bool isDead { get; private set; }
    [SerializeField] private int lives = 3; // Each player starts with 3 lives
    private CountdownUI respawnCountdownUI;
    private string teamTag;

    private InputManager inputManager;

    [Header("Misc.")]
    [SerializeField] private SpriteRenderer bubble;
    public PowerUpImage powerUpImageScript;
    [SerializeField] private SpriteRenderer iceCube;

    private EnemyAI enemyAI;

    [Header("Player Config")]
    public PlayerConfig playerConfig;

    private void Awake ()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        playerRope = GetComponentInChildren<PlayerRope>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        respawnCountdownUI = GetComponentInChildren<CountdownUI>();
        inputManager = GetComponent<InputManager>();
        powerUpImageScript = GetComponentInChildren<PowerUpImage>();
        enemyAI = GetComponent<EnemyAI>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        _colOffsetDefault = _col.offset;

        _currentSpeed = movementStates.MaxSpeed;
        originalGravityScale = _rb.gravityScale;

        canMove = true;
    }


    public void SetPlayerConfig ( PlayerConfig _playerConfig )
    {
        playerConfig = _playerConfig;
        teamTag = playerConfig.team.ToString();

        SetTagRecursively(transform, teamTag);
    }

    void SetTagRecursively ( Transform parent, string tag )
    {
        parent.gameObject.tag = tag;
        foreach (Transform child in parent)
        {
            SetTagRecursively(child, tag); // Recursive call for all children
        }
    }


    public void AssignHPBar ( Bar _hpBar )
    {
        hpBar = _hpBar;
        livesManager = _hpBar.GetComponentInChildren<LivesManager>();
    }

    public void AssignShieldBar ( Bar _shield )
    {
        shieldBar = _shield;
        shieldBar.SetValue(stats.Shield);
    }

    public void AssignManaBar ( Bar _manaBar )
    {
        manaBar = _manaBar;
    }


    private void Update ()
    {
        if (Time.timeScale == 0) return;

        if (isDead) return;

        if (!canMove) return;

        _time += Time.deltaTime;

        if (enemyAI != null) return; // AI Control

        GatherInput();
    }

    private void GatherInput ()
    {
        _frameInput = new FrameInput
        {
            JumpDown = inputManager.IsJumpPressed,
            JumpHeld = inputManager.IsJumpHeld,
            Move = new Vector2(inputManager.InputVelocity.x, inputManager.InputVelocity.y),
        };

        if (movementStates.SnapInput)
        {
            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < movementStates.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < movementStates.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
        }

        if (_frameInput.JumpDown)
        {
            JumpTriggered();
        }

    }

    public void JumpTriggered ()
    {
        _jumpToConsume = true;
        _timeJumpWasPressed = _time;
    }


    private void FixedUpdate ()
    {
        if (Time.timeScale == 0) return;

        CheckCollisions();
        HandleJump();

        HandleDirection();

        if (!OnRope())
        {
            HandleGravity();
            if (!enemyAI)
                ApplyMovement();
        }
    }

    private void LateUpdate ()
    {
        if (PlayerStatsManager.Instance != null)
            if (PlayerStatsManager.Instance.canvas.enabled == true)
            {
                if (inputManager.IsJumpPressed)
                {
                    PlayerStatsManager.Instance.VoteForRematch(playerConfig.playerIndex);
                }

                if (inputManager.IsSecondaryPressed)
                    PlayerStatsManager.Instance.LoadMainMenu();
            }
    }

    public void TakeDamage ( float damage, int otherPlayerIndex )
    {
        if (stats.Shield > 0 || stats.Health > 0)
        {
            Hit?.Invoke();
            if (inputManager != null)
                inputManager.StartVibration(0.5f, 250);

            playerAnimator.GetHitAnimation();

            // First, apply damage to the shield
            float damageToShield = Mathf.Min(damage, stats.Shield);
            stats.Shield -= damageToShield;

            if (shieldBar != null)
                shieldBar.AddValue(-damageToShield);

            // Calculate remaining damage after shield
            float remainingDamage = damage - damageToShield;

            // Apply remaining damage to health
            if (remainingDamage > 0)
            {
                stats.Health -= remainingDamage;
                hpBar.AddValue(-remainingDamage);
                playerRope?.DestroyCurrentRope();

                if (PlayerStatsManager.Instance != null)
                    PlayerStatsManager.Instance.AddDamageToPlayerState(remainingDamage, otherPlayerIndex);
            }

            if (stats.Health <= 0 && !isDead)
            {
                Die(otherPlayerIndex);
            }
        }
    }

    public void UpdateManaBar ( float _manaValue )
    {
        if (manaBar != null)
            manaBar.AddValue(_manaValue);
    }

    public void Die ( int killerIndex )
    {
        if (teamTag == null)
            teamTag = gameObject.tag;

        gameObject.tag = "Dodge";
        isDead = true;

        PlayerStatsManager.Instance?.allPlayerStats[playerConfig.playerIndex].RecordDeath();

        if (killerIndex >= 0)
            PlayerStatsManager.Instance?.allPlayerStats[killerIndex].RecordKill();


        playerRope?.DestroyCurrentRope();
        playerAnimator.DeathAnimation(true);
        canMove = false;

        _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;


        livesManager.LoseLife();
        lives--;

        if (lives > 0)
        {
            Respawn();
        }
        else
        {
            PlayerStatsManager.Instance.EndMatch();
        }
    }


    public void Respawn ()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine ()
    {
        yield return new WaitForSeconds(2f);
        // Start fade out coroutine instead of setting animation bool
        StartCoroutine(FadeOutSprite(bubble.GetComponent<SpriteRenderer>(), 6f));
        bubble.enabled = true;

        GameManager.Instance.playerSpawner.RespawnPlayer(playerConfig, this);
        respawnCountdownUI.StartTimer();
        yield return new WaitForSeconds(3f);


        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isDead = false;
        playerAnimator.DeathAnimation(false);

        stats.Health = stats.MaxHealth;
        hpBar.AddValue(stats.Health);
        manaBar.AddValue(stats.MaxMana);

        //minionSpawner.ConfigMinionSpawner();

        if (stats.spawnWithShield)
        {
            stats.Shield = stats.MaxShield;
            shieldBar.AddValue(stats.Shield);
        }

        canMove = true;
        // Wait for fade out to complete plus additional delay
        yield return new WaitForSeconds(3f); // 3 seconds for fade out and 0.5 second buffer

        bubble.enabled = false;
        gameObject.tag = teamTag;
    }

    private IEnumerator FadeOutSprite ( SpriteRenderer spriteRenderer, float duration )
    {
        float elapsedTime = 0;
        Color originalColor = spriteRenderer.color;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Ensure alpha is set to 0
    }

    public void Freeze ( float duration )
    {
        if (canMove)
        {
            StartCoroutine(FreezeCoroutine(duration));
        }
    }

    private IEnumerator FreezeCoroutine ( float duration )
    {
        iceCube.enabled = true;
        canMove = false;
        _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        yield return new WaitForSeconds(duration);

        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        iceCube.enabled = false;
        canMove = true;
    }

    #region Collisions

    private float _frameLeftGrounded = float.MinValue;
    public bool _grounded { get; private set; }

    private async void CheckCollisions ()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, movementStates.GrounderDistance, ~movementStates.PlayerLayer);

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            playerAnimator.FallAnimation(false);


            playerAnimator.JumpAnimation(false);
            await Task.Delay(3);
            _col.offset = _colOffsetDefault;

        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
            playerAnimator.FallAnimation(true);

        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }


    #endregion

    #region Jumping

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + movementStates.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + movementStates.CoyoteTime;

    private bool OnRope ()
    {
        if (playerRope != null)
            return playerRope.IsRopeConnected();
        else
            return false;
    }

    private void HandleJump ()
    {
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0)
        {
            _endedJumpEarly = true;
        }

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote || OnRope())
        {
            ExecuteJump();
        }

        _jumpToConsume = false;
    }

    private void ExecuteJump ()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = movementStates.JumpPower;

        if (OnRope())
            playerRope.DestroyCurrentRope();


        playerAnimator.JumpAnimation(true);
        Jumped?.Invoke();
    }



    #endregion

    #region Horizontal

    private void HandleDirection ()
    {
        if (_frameInput.Move.x == 0)
        {
            var deceleration = _grounded ? movementStates.GroundDeceleration : movementStates.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _currentSpeed, movementStates.Acceleration * Time.fixedDeltaTime);
        }
    }



    #endregion

    #region Gravity

    private void HandleGravity ()
    {
        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = movementStates.GroundingForce;
        }
        else
        {
            var inAirGravity = movementStates.FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= movementStates.JumpEndEarlyGravityModifier;
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -movementStates.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    void OnDrawGizmos ()
    {
        if (_col == null) return;

        Gizmos.color = Color.green;
        Vector2 castPosition = (Vector2)_col.bounds.center;
        Vector2 castSize = _col.size;
        Vector2 castDirection = Vector2.down;
        float castDistance = movementStates.GrounderDistance;

        Gizmos.DrawWireCube(castPosition + castDirection * castDistance, castSize);
    }


    public void ApplySpeedModifier ( float modifier, float newGravityScale, float duration )
    {
        // If there's an ongoing speed modifier, remove it first
        if (speedModifierCoroutine != null)
        {
            StopCoroutine(speedModifierCoroutine);
            _currentSpeed = movementStates.MaxSpeed; // Reset speed to base before applying new modifier
        }

        speedModifierCoroutine = StartCoroutine(SpeedModifierCoroutine(modifier, newGravityScale, duration));
    }

    private IEnumerator SpeedModifierCoroutine ( float speedModifier, float newGravityScale, float duration )
    {
        _currentSpeed *= speedModifier;
        _rb.gravityScale = newGravityScale;

        yield return new WaitForSeconds(duration);

        _currentSpeed = movementStates.MaxSpeed; // Reset to original speed after duration
        speedModifierCoroutine = null; // Clear the reference to the coroutine
    }


    public void RemoveSpeedModifier ()
    {
        if (speedModifierCoroutine != null)
        {
            StopCoroutine(speedModifierCoroutine); // Stop the coroutine
            speedModifierCoroutine = null;
        }

        _currentSpeed = movementStates.MaxSpeed;
        _rb.gravityScale = originalGravityScale;
    }

    #endregion

    private void ApplyMovement () => _rb.velocity = _frameVelocity;

    public void SetFrameVelocityX ( float velocityX )
    {
        Vector2 currentVelocity = _rb.velocity;
        currentVelocity.x = velocityX; // Only change the x component
        _frameVelocity = currentVelocity; // Apply the updated velocity
    }


#if UNITY_EDITOR
    private void OnValidate ()
    {
        if (movementStates == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif


    #region Power Ups

    public void IncreaseHealth ( int _hp )
    {
        stats.Health += _hp;
        stats.Health = Mathf.Clamp(stats.Health, 0, stats.MaxHealth);

        hpBar.AddValue(+_hp);
    }

    public void IncreaseSpeed ( float speedMultiplier, float duration )
    {
        StartCoroutine(IncreaseSpeedCoroutine(speedMultiplier, duration));
    }

    IEnumerator IncreaseSpeedCoroutine ( float fireDamageMultiplier, float duration )
    {
        _currentSpeed *= fireDamageMultiplier;

        yield return new WaitForSeconds(duration);

        _currentSpeed = movementStates.MaxSpeed;
    }

    #endregion
}

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 Move;
}

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;

    public event Action Jumped;
    public event Action Hit;
    public Vector2 FrameInput { get; }
}
