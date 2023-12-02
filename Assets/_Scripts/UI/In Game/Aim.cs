using UnityEngine;

public class Aim : MonoBehaviour
{

    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Transform weapon;
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
        if (inputManager != null)
        {
            Vector2 gamepadInput = inputManager.InputAim * gamepadAimSensitivity;
            UpdateAimPostion(gamepadInput);
        }
    }


    public void UpdateAimPostion ( Vector2 _direction )
    {
        if (_direction.magnitude > deadzone)
        {
            // Normalize the input to ensure the direction vector always has a length of 1
            aimDirection = _direction.normalized;
        }

        // Calculate the new position by extending the normalized direction vector 
        // by maxAimDistance from the player's position
        Vector2 newPosition = (Vector2)weapon.position + aimDirection * maxAimDistance;

        // Update the transform position and aimPosition
        transform.position = newPosition;
        aimPosition = newPosition;
    }




    public Vector2 GetAimPosition ()
    {
        return aimPosition;
    }

}
