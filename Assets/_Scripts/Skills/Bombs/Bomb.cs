using UnityEngine;

public class Bomb : MonoBehaviour
{
    public enum BombType
    {
        SlowBomb,
        SnowBomb
    }

    [Header("Bomb Settings")]
    [SerializeField] private BombType bombType = BombType.SlowBomb;
    [SerializeField] private float prefabAdjustmentY = -0.3f;

    [Header("Slow Bomb Settings")]
    [SerializeField] private float slowDownFactor = 0.5f;
    [SerializeField] private float gravityFactor = 10f;
    [SerializeField] private float effectDuration = 5f;
    [SerializeField] private float effectRadius = 3f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Prefabs")]
    [SerializeReference] private GameObject slowEffectPrefab;
    [SerializeReference] private GameObject snowmanPrefab;

    void Update ()
    {
        transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        Destroy(gameObject, 1 / Time.timeScale);
    }

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            switch (bombType)
            {
                case BombType.SlowBomb:
                    CreateSlowEffectArea(collision);
                    break;
                case BombType.SnowBomb:
                    CreateSnowman(collision);
                    break;
            }
            Destroy(gameObject); // Destroy the bomb after it hits the platform
        }
    }

    private void CreateSnowman ( Collider2D platformCollider )
    {
        // Calculate the position on top of the platform
        Vector3 snowmanPosition = new Vector3(transform.position.x, platformCollider.bounds.max.y + prefabAdjustmentY, transform.position.z);

        // Instantiate the snowman prefab at the calculated position
        Instantiate(snowmanPrefab, snowmanPosition, Quaternion.identity);
    }



    private void CreateSlowEffectArea ( Collider2D platformCollider )
    {
        // Calculate the position on top of the platform
        Vector3 effectPosition = new Vector3(transform.position.x, platformCollider.bounds.max.y + prefabAdjustmentY, transform.position.z);

        // Instantiate the slow effect prefab at the calculated position
        GameObject slowArea = Instantiate(slowEffectPrefab, effectPosition, Quaternion.identity);

        CircleCollider2D areaCollider = slowArea.GetComponent<CircleCollider2D>();
        areaCollider.radius = effectRadius;

        SlowEffectArea effectArea = slowArea.GetComponent<SlowEffectArea>();
        effectArea.Setup(slowDownFactor, gravityFactor, effectDuration, playerLayer);
    }
}