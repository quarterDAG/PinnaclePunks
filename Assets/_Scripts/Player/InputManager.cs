using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputDevice lastUsedDevice;
    private float lastDeviceUseTime;
    private const float deviceSwitchCooldown = 1.0f; // Time in seconds to switch back to mouse

    public bool IsUsingGamepad;
 /*   {
        get
        {
            return lastUsedDevice is Gamepad;
        }
    }*/


    public Vector2 InputVelocity { get; private set; }
    public Vector2 InputAim { get; private set; }


    public bool IsJumpPressed { get; private set; }
    public bool IsJumpHeld { get; private set; }
    public bool IsShootPressed { get; private set; }
    public bool IsRopeShootPressed { get; private set; }
    public bool IsDashPressed { get; private set; }
    public bool IsSlowmotionPressed { get; private set; }
    public Vector2 InventoryInput { get; private set; }

    public bool IsSpawnMonsterPressed { get; private set; }




/*    private void Update ()
    {
        UpdateLastUsedDevice();
    }

    private void UpdateLastUsedDevice ()
    {
        // Check if any gamepad button is pressed
        if (Gamepad.current != null && Gamepad.current.allControls.Any(c => c.IsPressed()))
        {
            lastUsedDevice = Gamepad.current;
            lastDeviceUseTime = Time.time;
        }
        // Check if there's significant mouse movement or mouse button is pressed
        else if (Mouse.current != null && (Mouse.current.delta.ReadValue() != Vector2.zero || Mouse.current.leftButton.isPressed))
        {
            lastUsedDevice = Mouse.current;
            lastDeviceUseTime = Time.time;
        }
        // Check if any keyboard key is pressed
        else if (Keyboard.current != null && Keyboard.current.allKeys.Any(k => k.isPressed))
        {
            lastUsedDevice = Keyboard.current;
            lastDeviceUseTime = Time.time;
        }
        else if (Time.time - lastDeviceUseTime > deviceSwitchCooldown)
        {
            // If no input has been detected for the cooldown period, default back to mouse
            lastUsedDevice = Mouse.current;
        }
    }*/


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
        // When the jump button is pressed down
        if (context.started)
        {
            IsJumpPressed = true;
            IsJumpHeld = true; // You start holding the button as soon as it's pressed
        }
        // When the jump button is held down
        else if (context.performed)
        {
            IsJumpHeld = true;
        }
        // When the jump button is released
        else if (context.canceled)
        {
            IsJumpPressed = false;
            IsJumpHeld = false;
        }
    }

    public void ResetJump () => InputVelocity = new Vector2(InputVelocity.x, 0);


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

    public void ResetRope ()
    {
        IsRopeShootPressed = false;
    }

}
