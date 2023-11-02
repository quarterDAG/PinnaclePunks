using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    public bool IsJumpingPressed
    {
        get;
        private set;
    }
    public Vector2 InputVelocity
    {
        get;
        private set;
    }
    
    public void OnMovementChanged(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }
        float horizontalInput = context.ReadValue<float>();
        InputVelocity = new Vector2(horizontalInput, 0);
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        // was the jump button released?
        if (!context.performed)
        {
            IsJumpingPressed = false;
            return;
        }

        InputVelocity += new Vector2(0, 1);
        IsJumpingPressed = true;
    }

    public void ResetJump() => InputVelocity = new Vector2(InputVelocity.x, 0);
}
