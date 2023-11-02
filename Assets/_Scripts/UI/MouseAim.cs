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


    private void Start ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update ()
    {
        HandleAim();
    }


    private void HandleAim ()
    {
        mousePosition = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)player.position).normalized;
        float actualDistance = Vector2.Distance(player.position, mousePosition);

        float aimDistance = Mathf.Min(actualDistance, maxAimDistance);
        aimPosition = (Vector2)player.position + direction * aimDistance;

        transform.position = aimPosition;

        // Change the color based on the result of the raycast
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
        if( collision.CompareTag("Shootable"))
        {
            isShootable = true;
        }
    }

    private void OnTriggerExit2D ( Collider2D collision )
    {
        if(collision.CompareTag("Shootable"))
        {
            isShootable = false;
        }
    }

    public Vector2 GetAimPosition()
    {
        return aimPosition;
    }

    public bool IsShootable()
    {
        return isShootable;
    }


}
