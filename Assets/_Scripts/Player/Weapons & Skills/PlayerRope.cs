using UnityEngine;

public class PlayerRope : MonoBehaviour
{
    [SerializeField] private Transform player;
    private PlayerController playerController;

    [SerializeField] private MouseAim mouseAim;

    [SerializeField] private GameObject ropePrefab;
    private GameObject currentRope;
    [SerializeField] private float ropeDistance = 20f;
    [SerializeField] private float ropeSpawnRate = 10;
    [SerializeField] private Transform firePoint;

    private float timeToSpawnRope = 0;

    [SerializeField] private float distanceFromChainEnd = 1.5f;
    [SerializeField] private HingeJoint2D joint;

    [SerializeField] private float swingForce = 100f;

    private InputManager inputManager;

    private void Awake ()
    {
        playerController = GetComponent<PlayerController>();
        inputManager = GetComponent<InputManager>();
    }

    private void Update ()
    {
        if (playerController.isDead) return;

        if (inputManager.IsRopeShootPressed)
        {
            ShootRope();
        }

        if (IsRopeConnected())
            HandleSwing();
    }
/*
    private void HandleRope ()
    {
    
        //inputManager.ResetRope();
    }*/


    private void ShootRope ()
    {
        Vector2 aimPosition = mouseAim.GetAimPosition();
        Vector2 firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);
        RaycastHit2D hit = Physics2D.Raycast(firePointPosition, aimPosition - firePointPosition, ropeDistance);

        if (Time.time >= timeToSpawnRope)
        {
            DestroyCurrentRope();

            currentRope = Instantiate(ropePrefab, firePoint.position, firePoint.rotation);

            PlayerRope playerRope = player.GetComponent<PlayerRope>();
            currentRope.GetComponent<Rope>().SetPlayerRope(playerRope);

            timeToSpawnRope = Time.time + 1 / ropeSpawnRate;
        }

    }


    public void DestroyCurrentRope ()
    {
        if (currentRope)
        {
            Destroy(currentRope);
            currentRope = null;
        }
    }

    private void HandleSwing ()
    {
        float vertical = inputManager.InputVelocity.y;
        float horizontal = inputManager.InputVelocity.x;
        transform.GetComponent<Rigidbody2D>().AddForce(transform.right * horizontal * swingForce, ForceMode2D.Force);
        transform.GetComponent<Rigidbody2D>().AddForce(transform.up * vertical * swingForce, ForceMode2D.Force);
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
