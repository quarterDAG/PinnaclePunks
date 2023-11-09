using System;
using System.Threading.Tasks;
using UnityEngine;
using static Bar;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour, IPlayerController, ICharacter
{
    [SerializeField] private ScriptableStats _stats;

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;

    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    [System.Serializable]
    public class PlayerStates
    {
        public int Health = 100;
        public int MaxHealth = 100;
    }

    public PlayerStates stats = new PlayerStates();

    #endregion

    private float _time;

    private PlayerRope playerRope;
    [SerializeField] private Bar hpBar;

    [SerializeField] private bool canMove = true;

    private PlayerAnimator playerAnimator;
    public bool isDead { get; private set; }
    [SerializeField] private int lives = 3; // Each player starts with 3 lives
    private Transform respawnPoint; // This will be the player's base or respawn point
    private CountdownUI respawnCountdownUI;
    private string teamTag;

    private InputManager inputManager;
    private LivesManager livesManager;

    [SerializeField] private PlayerStats playerStats;

    private void Awake ()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        playerRope = GetComponent<PlayerRope>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        respawnCountdownUI = GetComponentInChildren<CountdownUI>();
        inputManager = GetComponent<InputManager>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Start ()
    {
        teamTag = gameObject.tag;

    }


    public void AssignHPBar ( Bar _hpBar )
    {
        hpBar = _hpBar;
        livesManager = _hpBar.GetComponentInChildren<LivesManager>();
    }

    public void AssignRespawn ( Transform spawnPoint )
    {
        respawnPoint = spawnPoint;
    }

    private void Update ()
    {
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

        if (_stats.SnapInput)
        {
            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
        }

        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

    }

    private void FixedUpdate ()
    {
        CheckCollisions();

        HandleJump();
        HandleDirection();

        if (!playerRope.IsRopeConnected())
        {
            HandleGravity();
            ApplyMovement();
        }
    }




    public async void TakeDamage ( int damage )
    {

        if (stats.Health > 0)
        {
            playerAnimator.GetHitAnimation();
            stats.Health -= damage;
            hpBar.UpdateValue(-damage);
            playerRope.DestroyCurrentRope();

            if (TimeManager.Instance.isSlowMotionActive)
            {
                await Task.Delay(1000);
                TimeManager.Instance.CancelSlowMotionRequest();
            }
        }

        else
        {
            if (!isDead)
                Die();
        }

    }

    public void Die ()
    {
        isDead = true;
        playerStats.RecordDeath();

        playerRope.DestroyCurrentRope();
        playerAnimator.DeathAnimation(true);
        canMove = false;

        _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;


        livesManager.LoseLife();
        lives--; // Reduce life by 1
        if (lives > 0)
        {
            Respawn();
        }
        else
        {
            //this.gameObject.SetActive(false);
        }
    }

    async void Respawn ()
    {
        gameObject.tag = "Dodge";
        await Task.Delay(2000);

        this.transform.position = respawnPoint.position;
        respawnCountdownUI.StartTimer();
        await Task.Delay(3000);

        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isDead = false;
        playerAnimator.DeathAnimation(false);

        stats.Health = stats.MaxHealth;
        hpBar.UpdateValue(stats.Health);

        canMove = true;

        await Task.Delay(2000);

        gameObject.tag = teamTag;
    }



    #region Collisions

    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

    private void CheckCollisions ()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

        // Hit a Ceiling
        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));

            playerAnimator.JumpAnimation(false);
        }
        // Left the Ground
        else if (_grounded && !groundHit /*&& !playerRope.IsRopeConnected()*/)
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

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private bool OnRope => playerRope.IsRopeConnected();


    private void HandleJump ()
    {
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0)
        {
            _endedJumpEarly = true;
            inputManager.ResetJump(false, true);
        }

        if (_endedJumpEarly || _grounded && _frameInput.JumpHeld)
        {
            inputManager.ResetJump(false, false);
        }

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote || OnRope)
        {
            ExecuteJump();
            inputManager.ResetJump(false, true);
        }

        _jumpToConsume = false;
    }



    private void ExecuteJump ()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = _stats.JumpPower;

        if (playerRope.IsRopeConnected())
        {
            playerRope.DestroyCurrentRope();
        }

        playerAnimator.JumpAnimation(true);
        Jumped?.Invoke();
    }

    #endregion

    #region Horizontal

    private void HandleDirection ()
    {
        if (_frameInput.Move.x == 0)
        {
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Gravity

    private void HandleGravity ()
    {
        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = _stats.GroundingForce;
        }
        else
        {
            var inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion

    private void ApplyMovement () => _rb.velocity = _frameVelocity;

#if UNITY_EDITOR
    private void OnValidate ()
    {
        if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif
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
    public Vector2 FrameInput { get; }
}
