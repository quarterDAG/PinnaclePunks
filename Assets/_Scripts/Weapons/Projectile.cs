using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    [SerializeField] private int moveSpeed = 230;

    [SerializeField] private LayerMask hitTheseLayers;
    [SerializeField] private string damageTag;

    [SerializeField] private int bulletDamage = 10;
    [SerializeField] private float pushbackForce = 50f;


    private int shotOwnerIndex;


    void Update ()
    {
        transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        Destroy(gameObject, 1 / Time.timeScale);
    }

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        // Check for damage tag and if the collision has an ICharacter component

        if ((collision.CompareTag(damageTag) || collision.CompareTag("DropBat")) && collision.GetComponent<ICharacter>() != null)
        {
            collision.GetComponent<ICharacter>().TakeDamage(bulletDamage, shotOwnerIndex);

            Debug.Log(collision);

            // Pushback effect
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null && collision.CompareTag(damageTag))
            {
                Vector2 pushbackDirection = (collision.transform.position - transform.position).normalized;
                rb.AddForce(pushbackDirection * pushbackForce);
            }


            Destroy(gameObject);
        }
        // Check if the layer of the collided object is in the LayerMask
        else if (hitTheseLayers == (hitTheseLayers | (1 << collision.gameObject.layer)))
        {
            Destroy(gameObject);
        }
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
