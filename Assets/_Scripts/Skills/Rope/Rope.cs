using UnityEngine;
using System.Collections;


public class Rope : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Rigidbody2D hook;
    public GameObject linkPrefab;
    public PlayerRope playerRope;
    public int links = 15;
    public float ropeGenerateDelay = 0.05f; // Time in seconds between instantiating links
    public float shootSpeed = 10f; // Multiplier to speed up rope generation when shooting

    [SerializeField] private float destroyAfterSeconds = 0.8f;
    private GameObject lastRopeLink;

    private bool inAir = true;
    private float timeInAir = 0f;

    private AudioSource audioSource;
    [SerializeField] private AudioClip grapplingHook;


    private void Awake ()
    {
        audioSource = GetComponent<AudioSource>();
        lineRenderer = GetComponent<LineRenderer>();
    }
    private void Update ()
    {
        DestroyRopeAfterSecondInAir();
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer ()
    {
        if (lastRopeLink != null)
        {
            lineRenderer.positionCount = links + 1;  // Ensure correct position count

            lineRenderer.SetPosition(0, hook.transform.position);

            for (int i = 0; i < links; i++)
            {
                GameObject link = transform.GetChild(i).gameObject;
                lineRenderer.SetPosition(i + 1, link.transform.position);
            }
        }

    }


    private void DestroyRopeAfterSecondInAir ()
    {
        if (inAir)
        {
            transform.Translate(Vector3.right * Time.deltaTime * shootSpeed);

            // Increment the time in air
            timeInAir += Time.deltaTime;

            // Check if the rope has been in the air for more than 1 second
            if (timeInAir > destroyAfterSeconds)
            {
                Destroy(gameObject);
            }

        }
        else
            timeInAir = 0f;
    }

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        if (collision.CompareTag("Platform"))
        {
            audioSource.PlayOneShot(grapplingHook);
            inAir = false;
            GenerateRope();
        }
    }

    void GenerateRope ()
    {
        Rigidbody2D previousRG = hook;

        // Get the LineRenderer component once
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        // Set the number of positions for the LineRenderer based on the number of links
        lineRenderer.positionCount = links + 1; // +1 to include the hook's position

        // Set the first position of the LineRenderer to the hook's position
        lineRenderer.SetPosition(0, hook.transform.position);

        for (int i = 0; i < links; i++)
        {
            GameObject link = Instantiate(linkPrefab, transform);
            HingeJoint2D joint = link.GetComponent<HingeJoint2D>();
            joint.connectedBody = previousRG;

            // Set the LineRenderer's position for this link
            lineRenderer.SetPosition(i + 1, link.transform.position);

            if (i < links - 1)
            {
                previousRG = link.GetComponent<Rigidbody2D>();
            }
            else
            {
                playerRope.ConnectRopeEnd(link.GetComponent<Rigidbody2D>());
                lastRopeLink = link;
            }
        }
    }



    public void SetPlayerRope ( PlayerRope _playerRope )
    {
        this.playerRope = _playerRope;
    }

    public bool IsLastLinkConnected ()
    {
        return lastRopeLink != null;
    }


}
