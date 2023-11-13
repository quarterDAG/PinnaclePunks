using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Windows;
using System.Threading.Tasks;
using static PlayerConfigData;

public class InputManager : MonoBehaviour
{
    public string currentControlScheme { get; private set; } = "Gamepad"; //Default
    public Vector2 InputVelocity { get; private set; }
    public Vector2 InputAim { get; private set; }

    #region Jump
    public bool IsJumpPressed { get; private set; }
    public bool IsJumpHeld { get; private set; }
    #endregion

    public bool IsAttackPressed { get; private set; }
    public bool IsRopeShootPressed { get; private set; }
    public bool IsDashPressed { get; private set; }
    public bool IsSlowmotionPressed { get; private set; }
    public Vector2 InventoryInput { get; private set; }
    public bool IsSpawnMonsterPressed { get; private set; }


    private void Start ()
    {
        InputSystem.settings.maxEventBytesPerUpdate = 0;

  
    }


    private void LateUpdate ()
    {
        InventoryInput = Vector2.zero;
    }


    public void UpdateCurrentControlScheme ( string controlScheme )
    {
        currentControlScheme = controlScheme;
    }

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

    public void OnRopeShoot ( InputAction.CallbackContext context )
    {
        if (context.ReadValue<float>() != 0)
        {
            IsRopeShootPressed = true;
            IsJumpPressed = false;
        }
        else
        {
            IsRopeShootPressed = false;
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

    public ControlScheme GetCurrentControlScheme ( InputDevice lastDevice )
    {

        if (lastDevice is Gamepad)
        {
            return ControlScheme.Gamepad;
        }
        else if (lastDevice is Mouse)
        {
            return ControlScheme.Keyboard;
        }
        else if (lastDevice is Keyboard)
        {
            return ControlScheme.Keyboard;
        }
        else
            return ControlScheme.Gamepad;
        // Add more checks for other device types if needed.

    }
}
