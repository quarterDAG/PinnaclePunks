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
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    private Vector2 _colOffsetDefault;
    private Vector2 _targetColOffset;
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
        public int Health = 99;
        public int MaxHealth = 99;
        public int Shield = 0;
        public int MaxShield = 99;
        public bool spawnWithShield;
    }

    public PlayerStates stats = new PlayerStates();


    private float _time;

    private PlayerRope playerRope;
    private Bar hpBar;
    private Bar shieldBar;

    private bool canMove = true;
    private float originalGravityScale;

    private PlayerAnimator playerAnimator;
    public bool isDead { get; private set; }
    [SerializeField] private int lives = 3; // Each player starts with 3 lives
    private Transform respawnPoint; // This will be the player's base or respawn point
    private CountdownUI respawnCountdownUI;
    private string teamTag;

    private InputManager inputManager;
    private LivesManager livesManager;



    public PlayerConfig playerConfig { get; private set; }

    private void Awake ()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        playerRope = GetComponentInChildren<PlayerRope>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        respawnCountdownUI = GetComponentInChildren<CountdownUI>();
        inputManager = GetComponent<InputManager>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        _colOffsetDefault = _col.offset;
        _targetColOffset = new Vector2(_colOffsetDefault.x, _colOffsetDefault.y + 1.2f);

        _currentSpeed = movementStates.MaxSpeed;
        originalGravityScale = _rb.gravityScale;

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

    public void AssignRespawn ( Transform spawnPoint )
    {
        respawnPoint = spawnPoint;
    }

    private void Update ()
    {
        if (Time.timeScale == 0) return;

        if (isDead) return;

        if (!canMove) return;

        _time += Time.deltaTime;

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
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

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
            ApplyMovement();
        }

    }

    private void LateUpdate ()
    {
        if (inputManager.IsJumpPressed)
        {
            PlayerStatsManager.Instance.VoteForRematch(playerConfig.playerIndex);
        }
    }


    public async void TakeDamage ( int damage, int otherPlayerIndex )
    {
        if (stats.Shield > 0 || stats.Health > 0)
        {
            Hit?.Invoke();
            inputManager.StartVibration(0.5f, 250);
            playerAnimator.GetHitAnimation();

            // First, apply damage to the shield
            int damageToShield = Mathf.Min(damage, stats.Shield);
            stats.Shield -= damageToShield;
            shieldBar.UpdateValue(-damageToShield);

            // Calculate remaining damage after shield
            int remainingDamage = damage - damageToShield;

            // Apply remaining damage to health
            if (remainingDamage > 0)
            {
                stats.Health -= remainingDamage;
                hpBar.UpdateValue(-remainingDamage);
                playerRope?.DestroyCurrentRope();

                if (TimeManager.Instance.isSlowMotionActive)
                {
                    await Task.Delay(1000);
                    TimeManager.Instance.CancelSlowMotionRequest();
                }

                PlayerStatsManager.Instance.AddDamageToPlayerState(remainingDamage, otherPlayerIndex);
            }

            if (stats.Health <= 0 && !isDead)
            {
                Die(otherPlayerIndex);
            }
        }
    }


    public void Die ( int killerIndex )
    {
        gameObject.tag = "Dodge";
        isDead = true;
        PlayerStatsManager.Instance.allPlayerStats[playerConfig.playerIndex].RecordDeath();
        if (killerIndex >= 0)
            PlayerStatsManager.Instance.allPlayerStats[killerIndex].RecordKill();

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

    async void Respawn ()
    {
        await Task.Delay(2000);

        this.transform.position = respawnPoint.position;
        respawnCountdownUI.StartTimer();
        await Task.Delay(3000);

        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isDead = false;
        playerAnimator.DeathAnimation(false);

        stats.Health = stats.MaxHealth;
        hpBar.UpdateValue(stats.Health);

        if (stats.spawnWithShield)
        {
            stats.Shield = stats.MaxShield;
            shieldBar.SetValue(stats.Shield);
        }

        canMove = true;

        await Task.Delay(2000);

        gameObject.tag = teamTag;
    }



    #region Collisions

    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

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


            playerAnimator.JumpAnimation(false);
            await Task.Delay(3);
            _col.offset = _colOffsetDefault;
            //StartCoroutine(MoveColliderCoroutine(_colOffsetDefault, 0.005f)); // 10 milliseconds = 0.01 seconds

        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
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

        StartCoroutine(MoveColliderCoroutine(_targetColOffset, 0.5f));

        if (OnRope())
            playerRope.DestroyCurrentRope();


        playerAnimator.JumpAnimation(true);
        Jumped?.Invoke();
    }

    private IEnumerator MoveColliderCoroutine ( Vector2 targetOffset, float duration )
    {
        float halfDuration = duration / 2f;
        float time = 0;
        Vector2 initialOffset = _col.offset;

        // First, move to the target offset
        while (time < halfDuration)
        {
            _col.offset = Vector2.Lerp(initialOffset, targetOffset, time / halfDuration);
            time += Time.deltaTime;
            yield return null;
        }

        // Ensure the collider reaches the target exactly
        _col.offset = targetOffset;

        // Reset time and move back to the original offset
        time = 0;
        while (time < halfDuration)
        {
            _col.offset = Vector2.Lerp(targetOffset, initialOffset, time / halfDuration);
            time += Time.deltaTime;
            yield return null;
        }

        // Ensure the collider returns to its original position exactly
        _col.offset = initialOffset;
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

        hpBar.UpdateValue(+_hp);
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
