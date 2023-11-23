using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SafeZone : MonoBehaviour
{
    public int damagePerSecond = 5;

    private HashSet<PlayerController> playersOutsideSafeZone = new HashSet<PlayerController>();

    public float shrinkDuration = 90f; // Duration over which the object will shrink
    public float endScale = 10f; // The scale you want to end at, 0 for completely disappearing

    private Transform spriteTransform;

    private Vector3 originalScale;
    private Coroutine shrinkCoroutine;

    void Start ()
    {
        GameManager.Instance.SetSafeZone(this);
        spriteTransform = GetComponent<Transform>();

        originalScale = spriteTransform.localScale; // Store the original scale

        ResetSafeZone();
    }

    private IEnumerator Shrink ()
    {
        Vector3 startScale = spriteTransform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / shrinkDuration;

            spriteTransform.localScale = Vector3.Lerp(startScale, new Vector3(endScale, endScale, endScale), progress);

            yield return null;
        }
    }

    public bool IsInsideSafeZone ( Vector3 playerPosition )
    {
        return Vector3.Distance(playerPosition, transform.position) <= GetCurrentSafeZoneRadius();
    }

    public void RegisterInsideZonePlayer ( PlayerController player )
    {
        playersOutsideSafeZone.Remove(player);
    }

    public void RegisterOutsideZonePlayer ( PlayerController player )
    {
        playersOutsideSafeZone.Add(player);
    }

    private float GetCurrentSafeZoneRadius ()
    {
        // Calculate the current radius based on the local scale
        // Assuming the safe zone is a circle and scales uniformly
        return spriteTransform.localScale.x * 100;
    }


    private void OnTriggerEnter2D ( Collider2D other )
    {
        if (!other.CompareTag("Dodge"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                playersOutsideSafeZone.Remove(player);
            }

        }
    }

    private void OnTriggerExit2D ( Collider2D other )
    {
        if (!other.CompareTag("Dodge"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                playersOutsideSafeZone.Add(player);
            }

        }
    }

    private void ApplyDamageToPlayersOutsideSafeZone ()
    {
        foreach (var player in playersOutsideSafeZone)
        {
            player.TakeDamage(damagePerSecond, -1);
        }
    }

    public void StopApplyingDamage ()
    {
        CancelInvoke(nameof(ApplyDamageToPlayersOutsideSafeZone));
    }


    public void ResetSafeZone ()
    {
        if (shrinkCoroutine != null)
        {
            StopCoroutine(shrinkCoroutine);
        }


        spriteTransform.localScale = originalScale; // Reset the scale
        playersOutsideSafeZone.Clear(); // Clear the list of players outside the safe zone

        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            if (IsInsideSafeZone(player.transform.position))
            {
                playersOutsideSafeZone.Remove(player);
            }
            else
            {
                playersOutsideSafeZone.Add(player);
            }
        }


        shrinkCoroutine = StartCoroutine(Shrink());
        InvokeRepeating(nameof(ApplyDamageToPlayersOutsideSafeZone), 1f, 1f); // Start after 1 second and repeat every 1 second
    }
}
