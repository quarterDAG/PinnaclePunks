using UnityEngine;

public class SlowBomb : MonoBehaviour
{
    [SerializeField] private float slowDownFactor = 0.5f;
    [SerializeField] private float gravityFactor = 10f;
    [SerializeField] private float effectDuration = 5f;
    [SerializeField] private float effectRadius = 3f;

    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private LayerMask playerLayer;

    [SerializeReference] private GameObject slowEffectPrefab;


    void Update ()
    {
        transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        Destroy(gameObject, 1 / Time.timeScale);
    }

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            CreateSlowEffectArea(collision);
            Destroy(gameObject); // Destroy the bomb after it hits the platform
        }

    }


    private void CreateSlowEffectArea ( Collider2D platformCollider )
    {
        // Calculate the position on top of the platform
        Vector3 effectPosition = new Vector3(transform.position.x, platformCollider.bounds.max.y - 0.3f, transform.position.z);

        // Instantiate the slow effect prefab at the calculated position
        GameObject slowArea = Instantiate(slowEffectPrefab, effectPosition, Quaternion.identity);

        CircleCollider2D areaCollider = slowArea.GetComponent<CircleCollider2D>();
        areaCollider.radius = effectRadius;

        SlowEffectArea effectArea = slowArea.GetComponent<SlowEffectArea>();
        effectArea.Setup(slowDownFactor, gravityFactor, effectDuration, playerLayer);

        // Destroy the area of effect object after the effect duration
        //Destroy(slowArea, effectDuration);
    }
}