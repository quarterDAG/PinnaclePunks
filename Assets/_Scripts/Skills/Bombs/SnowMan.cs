using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowMan : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float effectDuration = 5f;


    void Start ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(FadeOutEffect());

    }

    private IEnumerator FadeOutEffect ()
    {
        float elapsedTime = 0;
        Color startColor = spriteRenderer.color; // If using a SpriteRenderer

        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / effectDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

}
