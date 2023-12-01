using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{

    public Gamepad currentGamepad;

    public Vector2 InputVelocity { get; private set; }
    public Vector2 InputAim { get; private set; }

    #region Jump
    public bool IsJumpPressed { get; private set; }
    public bool IsJumpHeld { get; private set; }
    #endregion

    public bool IsAttackPressed { get; private set; }
    public bool IsSecondaryPressed { get; private set; }
    public bool IsDashPressed { get; private set; }
    public bool IsSlowmotionPressed { get; private set; }
    public Vector2 InventoryInput { get; private set; }
    public bool IsSpawnMinionPressed { get; private set; }

    public bool IsStartPressed { get; private set; }

    private PlayerController playerController;

    private void Awake ()
    {
        playerController = GetComponent<PlayerController>();
    }

  /*  private void LateUpdate ()
    {
        InventoryInput = Vector2.zero;
    }
*/

    public void OnMovementChanged ( InputAction.CallbackContext context )
    {
        
        if (!context.performed)
        {
            return;
        }
        InputVelocity = context.ReadValue<Vector2>();
    }

    public void OnAimChanged ( InputAction.CallbackContext context )
    {
        if (playerController.isDead) return;


        if (!context.performed)
        {
            return;
        }
        InputAim = context.ReadValue<Vector2>();
    }


    public void OnJump ( InputAction.CallbackContext context )
    {
        float value = context.ReadValue<float>();
        float releaseThreshold = 0.1f; 

        if (value > releaseThreshold)
        {
            if (!IsJumpHeld)
            {
                IsJumpPressed = true;
                IsJumpHeld = true;
            }
        }
        else
        {
            IsJumpPressed = false;
            IsJumpHeld = false;
        }
    }


    public void OnAttack ( InputAction.CallbackContext context )
    {
        if (playerController.isDead) return;


        if (!context.performed)
        {
            IsAttackPressed = false;
            return;
        }

        if (context.performed)
            IsAttackPressed = true;

        if (context.canceled)
            IsAttackPressed = false;
    }

    public void OnSlowmotion ( InputAction.CallbackContext context )
    {
        if (playerController.isDead) return;


        if (!context.performed)
        {
            IsSlowmotionPressed = false;
            return;
        }

        IsSlowmotionPressed = true;
    }

    public void OnSecondary ( InputAction.CallbackContext context )
    {
        if (context.ReadValue<float>() != 0)
        {
            IsSecondaryPressed = true;
            IsJumpPressed = false;
        }
        else
        {
            IsSecondaryPressed = false;
        }

    }

    public void OnDash ( InputAction.CallbackContext context )
    {
        if (playerController.isDead) return;


        if (context.performed && !IsDashPressed)
        {
            IsDashPressed = true;
        }
    }

    public void OnSelectChanged ( InputAction.CallbackContext context )
    {

        if (!context.performed)
        {
            return;
        }
        InventoryInput = context.ReadValue<Vector2>();
    }

    public void OnSpawnMinion ( InputAction.CallbackContext context )
    {
        if (playerController.isDead) return;

        if (!context.performed)
        {
            IsSpawnMinionPressed = false;
            return;
        }

        IsSpawnMinionPressed = true;
    }

    public void OnStartPressed ( InputAction.CallbackContext context )
    {
        GameManager.Instance.PauseMenu(this);
    }

    public void ResetDash ()
    {
        IsDashPressed = false;
    }

    public void ResetJump ( bool isJumpPressed, bool isJumpHeld )
    {
        IsJumpPressed = isJumpPressed;
        IsJumpHeld = isJumpHeld;
    }


    public void StartVibration ( float intensity, int duration )
    {
        if (currentGamepad != null)
        {
            currentGamepad.SetMotorSpeeds(intensity, intensity); // Set both motors to the same speed

            StopVibration(duration);
        }
    }

    public async void StopVibration ( int duration )
    {
        await Task.Delay(duration);

        if (currentGamepad != null)
        {
            currentGamepad.ResetHaptics();
        }
    }

    private void OnDestroy ()
    {
        StopAllCoroutines();
    }

    public void SetCurrentInputDevice(InputDevice _inputDevice)
    {
        if (_inputDevice is Gamepad)
        {
            currentGamepad = _inputDevice as Gamepad;
        }
    }


}
