using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    [SerializeField] private int moveSpeed = 230;

    [SerializeField] private LayerMask hitTheseLayers;
    [SerializeField] private string damageTag;

    [SerializeField] private int bulletDamage = 10;
    [SerializeField] private float pushbackForce = 50f;
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;

    private int shotOwnerIndex;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip arrowHit;

    private void Awake ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponentInChildren<LineRenderer>();
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

            if (character != null)
            {
                HandleDamage(character, rb, collision);
            }
        }
        else if (IsInLayerMask(collision.gameObject.layer, hitTheseLayers))
        {
            Destroy(gameObject);
        }
    }

    private void HandleDamage ( ICharacter character, Rigidbody2D rb, Collider2D collision )
    {
        PlaySoundEffect(arrowHit);
        character.TakeDamage(bulletDamage, shotOwnerIndex);

        spriteRenderer.enabled = false;
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


    public void SetDamage ( int damage )
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
}
