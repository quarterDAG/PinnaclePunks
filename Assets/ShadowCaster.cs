using UnityEngine;

public class ShadowCaster : MonoBehaviour
{
    public Transform shadowTransform;
    public SpriteRenderer shadowSpriteRenderer;
    public float maxHeight = 10.0f;
    public LayerMask groundLayer;

    [SerializeField] private Collider2D playerCollider;
    private Color currentColor = Color.black; // Default color

    void Update ()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, maxHeight, groundLayer);

        if (hit.collider != null && hit.collider != playerCollider)
        {
            Vector3 shadowLocalPosition = transform.InverseTransformPoint(hit.point);
            shadowTransform.localPosition = new Vector3(shadowLocalPosition.x, shadowLocalPosition.y, shadowTransform.localPosition.z);

            float distance = hit.distance;
            float alpha = 1 - (distance / maxHeight);
            shadowSpriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Clamp(alpha, 0, 1));
        }
        else
        {
            shadowSpriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0);
        }
    }

    public void SetColor ( Color color )
    {
        currentColor = color;
        shadowSpriteRenderer.color = color; // This sets the initial color, including alpha
    }
}
