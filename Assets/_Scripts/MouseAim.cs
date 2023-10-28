using UnityEngine;

public class MouseAim : MonoBehaviour
{

    public Camera sceneCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float maxAimDistance = 5f;

    //[SerializeField] private GameObject ropePrefab; 

    private Vector2 mousePosition;
    private Vector2 aimPosition;
    private bool isShootable;

    //private GameObject currentRope;

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
        //HandleRope();
    }

   /*

    private void HandleRope ()
    {
        // Check for left mouse button click
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isShootable)
            {
                DestroyCurrentRope();

                // Instantiate a new Rope prefab at the aim position
                currentRope = Instantiate(ropePrefab, aimPosition, Quaternion.identity);

                PlayerRope playerRope = player.GetComponent<PlayerRope>();
                currentRope.GetComponent<Rope>().SetPlayerRope(playerRope);

                Rope ropeScript = currentRope.GetComponent<Rope>();
                ropeScript.SetTarget(aimPosition);
            }
        }
        else if (Input.GetMouseButtonDown(1) && currentRope)
        {
            DestroyCurrentRope();
        }
    }*/

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
/*

    public void DestroyCurrentRope ()
    {
        if (currentRope)
        {

            Destroy(currentRope);
            currentRope = null;
        }
    }*/
/*
    public bool HasRope ()
    {
        return currentRope != null;
    }*/

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


}
