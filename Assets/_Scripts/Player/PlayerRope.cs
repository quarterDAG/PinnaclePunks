using UnityEngine;

public class PlayerRope : MonoBehaviour
{
    [SerializeField] private Transform player;

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

    private void Update ()
    {
        HandleRope();


        if (IsRopeConnected())
            HandleSwing();
    }

    private void HandleRope ()
    {
        if (Input.GetButton("Fire2"))
        {
            if (mouseAim.IsShootable())
            {
                InstantiateRope();
            }
            else
                ShootRope();
        }
    }


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

    private void InstantiateRope ()
    {
        DestroyCurrentRope();

        Vector2 aimPosition = mouseAim.GetAimPosition();

        // Instantiate a new Rope prefab at the aim position
        currentRope = Instantiate(ropePrefab, aimPosition, Quaternion.identity);

        PlayerRope playerRope = player.GetComponent<PlayerRope>();
        currentRope.GetComponent<Rope>().SetPlayerRope(playerRope);

        Rope ropeScript = currentRope.GetComponent<Rope>();
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