using UnityEngine;
using System.Collections;


public class Rope : MonoBehaviour
{

    public Rigidbody2D hook;
    public GameObject linkPrefab;
    public PlayerRope playerRope;
    public int links = 15;
    public float ropeGenerateDelay = 0.05f; // Time in seconds between instantiating links
    public float shootSpeed = 10f; // Multiplier to speed up rope generation when shooting

    //[SerializeField] private float swingForce = 100f;

    private GameObject lastRopeLink;

    private bool inAir = true;
    private float timeInAir = 0f;


    private void Update ()
    {
        if (inAir)
        {
            transform.Translate(Vector3.right * Time.deltaTime * shootSpeed);

            // Increment the time in air
            timeInAir += Time.deltaTime;

            // Check if the rope has been in the air for more than 1 second
            if (timeInAir > 1)
            {
                Destroy(gameObject);
            }

        }
        else
            timeInAir = 0f;
    }



    private void OnTriggerEnter2D ( Collider2D collision )
    {
        if (collision.CompareTag("Shootable"))
        {
            inAir = false;
            GenerateRope();
        }
    }


    void GenerateRope ()
    {
        Rigidbody2D previousRG = hook;

        for (int i = 0; i < links; i++)
        {
            GameObject link = Instantiate(linkPrefab, transform);
            HingeJoint2D joint = link.GetComponent<HingeJoint2D>();
            joint.connectedBody = previousRG;

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

    public bool IsLastLinkConnected()
    {
        return lastRopeLink != null;
    }


}
