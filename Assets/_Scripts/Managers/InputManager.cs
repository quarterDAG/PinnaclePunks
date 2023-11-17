using System.Collections;
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
    public bool IsSpawnMonsterPressed { get; private set; }



    private void LateUpdate ()
    {
        InventoryInput = Vector2.zero;
    }


    public void OnMovementChanged ( InputAction.CallbackContext context )
    {
        if (context.control.device is Gamepad gamepad)
        {
            currentGamepad = gamepad;
        }

        if (!context.performed)
        {
            return;
        }
        InputVelocity = context.ReadValue<Vector2>();
    }

    public void OnAimChanged ( InputAction.CallbackContext context )
    {
        if (!context.performed)
        {
            return;
        }
        InputAim = context.ReadValue<Vector2>();
    }


    public void OnJump ( InputAction.CallbackContext context )
    {
        float value = context.ReadValue<float>();
        float releaseThreshold = 0.1f; // Set a threshold for detecting release

        if (value > releaseThreshold)
        {
            if (!IsJumpHeld)
            {
                // The button is pressed, set IsJumpPressed to true
                IsJumpPressed = true;
                IsJumpHeld = true;
            }
        }
        else
        {
            // The button is released (or nearly released), set IsJumpPressed to false
            IsJumpPressed = false;
            IsJumpHeld = false;
        }
    }


    public void OnAttack ( InputAction.CallbackContext context )
    {
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

    public void OnSpawnMonster ( InputAction.CallbackContext context )
    {
        if (!context.performed)
        {
            IsSpawnMonsterPressed = false;
            return;
        }

        IsSpawnMonsterPressed = true;
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


    public void StartVibration ( float intensity, float duration )
    {
        currentGamepad.SetMotorSpeeds(intensity, intensity); // Set both motors to the same speed

        StartCoroutine(StopVibration(duration));
    }

    IEnumerator StopVibration ( float duration )
    {
        yield return new WaitForSeconds(duration);

        currentGamepad.ResetHaptics();
    }

}
