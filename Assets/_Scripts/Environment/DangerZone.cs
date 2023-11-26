using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DangerZone : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private bool reverseDamageLogic;
    [SerializeField] private bool isShrinking;
    [SerializeField] private int damagePerSecond = 5;

    private HashSet<PlayerController> playersInDamageZone = new HashSet<PlayerController>();

    [Header("Shrinking Settings")]
    [SerializeField] private float shrinkDuration = 90f; // Duration over which the object will shrink
    [SerializeField] private float endScale = 10f; // The scale you want to end at, 0 for completely disappearing

    [SerializeField] private float bounceStrength = 1f;
    private Transform spriteTransform;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Coroutine shrinkCoroutine;

    void Start ()
    {
        GameManager.Instance.SetSafeZone(this);
        spriteTransform = GetComponent<Transform>();
        originalPosition = spriteTransform.position;
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


    private float GetCurrentSafeZoneRadius ()
    {
        // Calculate the current radius based on the local scale
        // Assuming the safe zone is a circle and scales uniformly
        return spriteTransform.localScale.x * 100;
    }


    private void OnTriggerEnter2D ( Collider2D other )
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            UpdatePlayerDamageStatus(player, inside: true);
        }
    }

    private void OnTriggerExit2D ( Collider2D other )
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            UpdatePlayerDamageStatus(player, inside: false);
        }
    }

    private async void OnCollisionEnter2D ( Collision2D collision )
    {
        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            UpdatePlayerDamageStatus(player, inside: true);
        }

        var drop = collision.gameObject.GetComponent<DropItem>();
        if (drop != null)
        {
            await drop.Pop(null);
        }

    }


    private void OnCollisionExit2D ( Collision2D collision )
    {
        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            UpdatePlayerDamageStatus(player, inside: false);
        }
    }


    private void UpdatePlayerDamageStatus ( PlayerController player, bool inside )
    {
        if ((reverseDamageLogic && inside) || (!reverseDamageLogic && !inside))
        {
            playersInDamageZone.Add(player);
        }
        else
        {
            playersInDamageZone.Remove(player);
        }
    }

    private void ApplyDamageToPlayersInDamageZone ()
    {
        foreach (var player in playersInDamageZone)
        {
            player.TakeDamage(damagePerSecond, -1);
        }
    }

    public void StopApplyingDamage ()
    {
        CancelInvoke(nameof(ApplyDamageToPlayersInDamageZone));
    }


    public void ResetSafeZone ()
    {
        if (shrinkCoroutine != null)
        {
            StopCoroutine(shrinkCoroutine);
        }

        spriteTransform.localScale = originalScale;
        spriteTransform.position = originalPosition;
        playersInDamageZone.Clear();

        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            bool isInside = IsInsideSafeZone(player.transform.position);
            UpdatePlayerDamageStatus(player, isInside);
        }

        if (isShrinking)
            shrinkCoroutine = StartCoroutine(Shrink());

        InvokeRepeating(nameof(ApplyDamageToPlayersInDamageZone), 1f, 1f);
    }
}
