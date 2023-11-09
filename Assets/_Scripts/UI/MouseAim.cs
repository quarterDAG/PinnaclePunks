using UnityEngine;
using static PlayerManager;

public class MouseAim : MonoBehaviour
{

    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float maxAimDistance = 5f;

    private Vector2 aimPosition;
    private Vector2 aimDirection = Vector2.left;


    private InputManager inputManager;

    [SerializeField] private float gamepadAimSensitivity = 0.05f;
    [SerializeField] private float deadzone = 0.1f;


    private void Awake ()
    {
        inputManager = GetComponentInParent<InputManager>();
    }

    // Update is called once per frame
    void Update ()
    {
        HandleAim();
    }

    private void HandleAim ()
    {
        switch (inputManager.currentControlScheme)
        {
            case "Gamepad":
                UpdateAimWithGamepad();
                break;
            case "Keyboard":
                UpdateAimWithMouse();
                break;
        }

        // Apply the last known position to the transform and update the color
        transform.position = (Vector2)player.position + aimDirection * maxAimDistance;
        aimPosition = transform.position;   
    }

    private void UpdateAimWithGamepad ()
    {
        Vector2 gamepadInput = inputManager.InputAim * gamepadAimSensitivity;
        if (gamepadInput.magnitude > deadzone)
        {
            // Normalize the input and use it to set the new aim direction
            aimDirection = gamepadInput;
        }
    }

    private void UpdateAimWithMouse ()
    {
        Vector2 mousePosition = sceneCamera.ScreenToWorldPoint(inputManager.InputAim);
        aimDirection = (mousePosition - (Vector2)player.position).normalized;
    }



    public Vector2 GetAimPosition ()
    {
        return aimPosition;
    }

}
