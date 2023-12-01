using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType
    {
        Arrow,
        IceBolt
    }

    [SerializeField] private ProjectileType projectileType;

    [SerializeField] private int moveSpeed = 230;

    [SerializeField] private LayerMask hitTheseLayers;
    [SerializeField] private string damageTag;

    [SerializeField] private float bulletDamage = 10;
    [SerializeField] private float pushbackForce = 50f;
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    private Collider2D projectileCollider;

    private int shotOwnerIndex;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip arrowHit;

    private PlayerController playerController;

    private void Awake ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponentInChildren<LineRenderer>();
        projectileCollider = GetComponent<Collider2D>();
    }

    void Update ()
    {
        transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        Destroy(gameObject, 1 / Time.timeScale);
    }


    private void OnTriggerEnter2D ( Collider2D collision )
    {
        if (collision.CompareTag(damageTag) || collision.CompareTag("DropBat"))
        {
            var character = collision.GetComponent<ICharacter>();
            var rb = collision.GetComponent<Rigidbody2D>();
            var playerController = collision.GetComponent<PlayerController>();

            if (character != null)
            {
                if (playerController == null || playerController.playerConfig.playerIndex != shotOwnerIndex)
                {
                    if (collision.GetComponent<Minion>() == null)
                        HandleDamage(character, rb, collision);

                    if (projectileType == ProjectileType.IceBolt)
                        character.Freeze(2f);
                }
            }
        }
        else if (IsInLayerMask(collision.gameObject.layer, hitTheseLayers))
        {
            Destroy(gameObject);
        }

        DropItem drop = collision.gameObject.GetComponent<DropItem>();
        if (drop != null)
        {
            drop.Pop(playerController);
        }
    }

    private void HandleDamage ( ICharacter character, Rigidbody2D rb, Collider2D collision )
    {
        projectileCollider.enabled = false;
        PlaySoundEffect(arrowHit);
        character.TakeDamage(bulletDamage, shotOwnerIndex);

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (rb != null && collision.CompareTag(damageTag))
        {
            ApplyPushback(rb, collision.transform.position);
        }

        StartCoroutine(DestroyAfterDelay(1f)); // Delay in seconds
    }

    private void ApplyPushback ( Rigidbody2D rb, Vector3 position )
    {
        Vector2 pushbackDirection = (position - transform.position).normalized;
        rb.AddForce(pushbackDirection * pushbackForce);
    }

    private void PlaySoundEffect ( AudioClip clip )
    {
        audioSource.PlayOneShot(clip);
    }

    private IEnumerator DestroyAfterDelay ( float delay )
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private bool IsInLayerMask ( int layer, LayerMask layerMask )
    {
        return layerMask == (layerMask | (1 << layer));
    }


    public void SetDamage ( float damage )
    {
        bulletDamage = damage;
    }

    public void SetTagToDamage ( string tag )
    {
        damageTag = tag;
    }

    public void SetPlayerOwnerIndex ( int _playerIndex )
    {
        shotOwnerIndex = _playerIndex;
    }

    public void SetBulletGradient ( Gradient newGradient )
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.colorGradient = newGradient;
    }

    public void SetPlayerController ( PlayerController _playerController )
    {
        playerController = _playerController;
    }
}
