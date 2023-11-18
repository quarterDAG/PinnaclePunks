using Unity.VisualScripting;
using UnityEngine;

public class PlayerRope : MonoBehaviour
{
    private Transform player;
    private PlayerController playerController;

    [SerializeField] private MouseAim mouseAim;

    [SerializeField] private GameObject ropePrefab;
    private GameObject currentRope;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private float distanceFromChainEnd = 1.5f;
    [SerializeField] private HingeJoint2D joint;

    [SerializeField] private float swingForce = 100f;
    [SerializeField] private float ropeSMCost = 10f;

    private InputManager inputManager;
    private SlowmotionController slowmotionController;

    private void Awake ()
    {
        player = transform.parent;
        playerController = player.GetComponent<PlayerController>();
        inputManager = player.GetComponent<InputManager>();
        slowmotionController = player.GetComponent<SlowmotionController>();
    }

    private void Update ()
    {
        if (playerController.isDead) return;

        if (inputManager.IsSecondaryPressed)
        {
            if (currentRope == null)
            {
                ShootRope();
            }

            if (IsRopeConnected())
            {
                HandleSwing();
                lineRenderer.enabled = false;
            }
            else
            {
                UpdateLineRenderer();
            }

        }
        else
        {
            DestroyCurrentRope();
            lineRenderer.enabled = false;
        }
    }

    private void ShootRope ()
    {
        DestroyCurrentRope();

        if (slowmotionController.slowmotionBar.IsEmpty()) { return; }

        slowmotionController.UpdateSMBar(-ropeSMCost);
        currentRope = Instantiate(ropePrefab, firePoint.position, firePoint.rotation);
        currentRope.GetComponent<Rope>().SetPlayerRope(this);

        lineRenderer.enabled = true;
    }

    private void UpdateLineRenderer ()
    {
        if (currentRope != null)
        {
            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, currentRope.transform.position);
        }
    }


    public void DestroyCurrentRope ()
    {
        if (currentRope)
        {

            Destroy(currentRope);
            currentRope = null;
            lineRenderer.enabled = false;
        }
    }

    private void HandleSwing ()
    {

        if (currentRope.transform.position.y > transform.position.y)
        {
            float vertical = inputManager.InputVelocity.y;
            player.GetComponent<Rigidbody2D>().AddForce(transform.up * vertical * swingForce, ForceMode2D.Force);
        }

        float horizontal = inputManager.InputVelocity.x;
        player.GetComponent<Rigidbody2D>().AddForce(transform.right * horizontal * swingForce, ForceMode2D.Force);
    }

    public void ConnectRopeEnd ( Rigidbody2D endRB )
    {
        joint.enabled = true;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = endRB;
        joint.connectedAnchor = new Vector2(0f, -distanceFromChainEnd);
    }


    public bool IsRopeConnected ()
    {
        return joint.connectedBody != null;
    }

}
