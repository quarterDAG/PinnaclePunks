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

    public bool IsShootPressed { get; private set; }
    public bool IsRopeShootPressed { get; private set; }
    public bool IsDashPressed { get; private set; }
    public bool IsSlowmotionPressed { get; private set; }
    public Vector2 InventoryInput { get; private set; }
    public bool IsSpawnMonsterPressed { get; private set; }


    private void Update ()
    {
        //DisableDuplicateGamepads();
    }

    private void LateUpdate ()
    {
        InventoryInput = Vector2.zero;
        IsRopeShootPressed = false;

    }

    private void DisableDuplicateGamepads ()
    {
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad is UnityEngine.InputSystem.DualShock.DualShockGamepad) // Check if it's a DualShock gamepad.
            {
              
                InputSystem.DisableDevice(gamepad);

               /* foreach (var item in Gamepad.all)
                {
                    // Check if another gamepad is detected at almost the same time.
                    if ((item is UnityEngine.InputSystem.XInput.XInputController) && (Math.Abs(item.lastUpdateTime - gamepad.lastUpdateTime) < 0.1))
                    {
                        //Debug.Log($"DualShock gamepad detected and a copy of XInput was active at almost the same time. Disabling XInput device. `{gamepad}`; `{item}`");
                    }
                }*/
            }
        }
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
        if (context.started)
        {
            IsJumpPressed = true;
        }
        if (context.performed)
        {
            IsJumpHeld = true;
        }
        else if (context.canceled)
        {
            IsJumpPressed = false;
            IsJumpHeld = false;
        }
    }

    public void OnShoot ( InputAction.CallbackContext context )
    {
        if (!context.performed)
        {
            IsShootPressed = false;
            return;
        }

        IsShootPressed = true;
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
        if (context.performed && !IsRopeShootPressed)
        {
            IsRopeShootPressed = true;
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
