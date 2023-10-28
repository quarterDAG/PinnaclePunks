using UnityEngine;

public class PlayerRope : MonoBehaviour
{
    public float distanceFromChainEnd = 1.5f;
    [SerializeField] private HingeJoint2D joint;

    [SerializeField] private float swingForce = 100f;


    private void Update ()
    {
        if(IsRopeConnected())
            HandleSwing();
    }

    private void HandleSwing ()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        transform.GetComponent<Rigidbody2D>().AddForce(transform.right * horizontal * swingForce, ForceMode2D.Force);
        transform.GetComponent<Rigidbody2D>().AddForce(transform.up * vertical * swingForce, ForceMode2D.Force);
    }

    public void ConnectRopeEnd(Rigidbody2D endRB)
    {
        joint.enabled = true;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = endRB;
        joint.connectedAnchor = new Vector2(0f, -distanceFromChainEnd);
    }

    public bool IsRopeConnected()
    {
        return joint.connectedBody != null;
    }

}
