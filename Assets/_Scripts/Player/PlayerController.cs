using System;
using System.Threading.Tasks;
using UnityEngine;

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
    }

    public PlayerStates stats = new PlayerStates();

    #endregion

    private float _time;

    [SerializeField] private TimeManager timeManager;
    [SerializeField] private Weapon weapon;
    private PlayerRope playerRope;
    [SerializeField] HPBar hpBar;

    private PlayerAnimator playerAnimator;

    private void Awake ()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        playerRope = GetComponent<PlayerRope>();
        playerAnimator = GetComponent<PlayerAnimator>();    

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update ()
    {
        _time += Time.deltaTime;
        GatherInput();
        HandleSlowMotion();
    }

    private void GatherInput ()
    {
        _frameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
            JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
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

    private void HandleSlowMotion ()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            timeManager.DoSlowMotion();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            timeManager.StopSlowMotion();
        }
    }



    public async void TakeDamage ( int damage )
    {
        playerAnimator.GetHitAnimation();
        stats.Health -= damage;

        hpBar.UpdateHPFillUI(stats.Health);
        playerRope.DestroyCurrentRope();

        if (timeManager.isSlowMotionActive)
        {
            await Task.Delay(1000);
            timeManager.StopSlowMotion();
        }


        if (stats.Health <= 0)
        {
            //GameMaster.KillPlayer(this);
        }

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
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote || OnRope) ExecuteJump();

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
