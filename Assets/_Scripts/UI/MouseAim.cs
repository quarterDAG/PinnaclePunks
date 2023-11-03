using UnityEngine;

public class MouseAim : MonoBehaviour
{

    public Camera sceneCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float maxAimDistance = 5f;

    private Vector2 mousePosition;
    private Vector2 aimPosition;
    private bool isShootable;


    [SerializeField] private Color shootableColor = Color.green; // Color when aim is on a shootable object
    [SerializeField] private Color nonShootableColor = Color.red; // Color when aim is not on a shootable object

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
        if (inputManager.IsUsingGamepad)
        {
            UpdateAimWithGamepad();
        }
        else
        {
            UpdateAimWithMouse();
        }

        // Apply the last known position to the transform and update the color
        transform.position = aimPosition;
        UpdateAimColor();
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

    private void UpdateAimColor ()
    {
        if (isShootable)
        {
            spriteRenderer.color = shootableColor;
        }
        else
        {
            spriteRenderer.color = nonShootableColor;
        }
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
