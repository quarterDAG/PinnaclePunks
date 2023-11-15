using UnityEngine;
using System.Collections;

public class SlowEffectArea : MonoBehaviour
{
    [SerializeField] private float slowDownFactor;
    [SerializeField] private float effectDuration;
    [SerializeField] private float slowGravityScale;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private LayerMask playerLayer;


    public void Setup ( float factor, float slowFactor, float duration, LayerMask layer )
    {
        slowDownFactor = factor;
        slowGravityScale = slowFactor;
        effectDuration = duration;
        playerLayer = layer;

        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(FadeOutEffect());

    }

    private IEnumerator FadeOutEffect ()
    {
        float elapsedTime = 0;
        Color startColor = spriteRenderer.color; // If using a SpriteRenderer
        // Color startColor = renderer.material.color; // If using a MeshRenderer

        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / effectDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            // renderer.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha); // If using a MeshRenderer
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D ( Collider2D collider )
    {
        if (((1 << collider.gameObject.layer) & playerLayer) != 0)
        {
            PlayerController player = collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplySpeedModifier(slowDownFactor, slowGravityScale, effectDuration);
            }
        }
    }

    private void OnTriggerExit2D ( Collider2D collider )
    {
        if (((1 << collider.gameObject.layer) & playerLayer) != 0)
        {
            PlayerController player = collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.RemoveSpeedModifier();
            }
        }
    }
}
