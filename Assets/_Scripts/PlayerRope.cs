using UnityEngine;

public class PlayerRope : MonoBehaviour
{
    public float distanceFromChainEnd = 1.5f;
    [SerializeField] private HingeJoint2D joint;

    public void ConnectRopeEnd(Rigidbody2D endRB)
    {
        joint.enabled = true;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = endRB;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = new Vector2(0f, -distanceFromChainEnd);
    }

}
