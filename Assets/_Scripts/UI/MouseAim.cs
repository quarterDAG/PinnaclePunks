using UnityEngine;
using static PlayerManager;

public class MouseAim : MonoBehaviour
{

    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float maxAimDistance = 5f;

    private Vector2 mousePosition;
    private Vector2 aimPosition;
    private bool isShootable;

    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer of the aim

    private InputManager inputManager;

    [SerializeField] private float gamepadAimSensitivity = 0.05f;

    private void Awake ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        transform.position = aimPosition;
    }

    private void UpdateAimWithGamepad ()
    {
        Vector2 gamepadInput = inputManager.InputAim;
        Vector2 aimInput = gamepadInput * gamepadAimSensitivity;
        aimPosition = (Vector2)player.position + aimInput * maxAimDistance;

        // If the input is in the deadzone, do not update lastAimPosition, keeping the aim in its last position
    }

    private void UpdateAimWithMouse ()
    {
        mousePosition = sceneCamera.ScreenToWorldPoint(inputManager.InputAim);
        Vector2 direction = (mousePosition - (Vector2)player.position).normalized;
        float actualDistance = Vector2.Distance(player.position, mousePosition);
        float aimDistance = Mathf.Min(actualDistance, maxAimDistance);
        aimPosition = (Vector2)player.position + direction * aimDistance;
    }




    private void OnTriggerEnter2D ( Collider2D collision )
    {
        if (collision.CompareTag("Shootable"))
        {
            isShootable = true;
        }
    }

    private void OnTriggerExit2D ( Collider2D collision )
    {
        if (collision.CompareTag("Shootable"))
        {
            isShootable = false;
        }
    }

    public Vector2 GetAimPosition ()
    {
        return aimPosition;
    }

    public bool IsShootable ()
    {
        return isShootable;
    }


}
