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

    private Vector2 targetPosition;

    /*void Start ()
    {
        GenerateRope();
    }*/

    public void SetTarget ( Vector2 target )
    {
        targetPosition = target;

        // Calculate the distance from the player to the target
        float distanceToTarget = Vector2.Distance(hook.position, targetPosition);

        // Calculate the number of links required based on the distance
        //int requiredLinks = Mathf.CeilToInt(distanceToTarget / linkLength);

        // Generate the rope using the required number of links
        GenerateRope();
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
            }

        }
    }



    public void SetPlayerRope ( PlayerRope _playerRope )
    {
        this.playerRope = _playerRope;
    }


}
