using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MoveTrail : MonoBehaviour
{

    [SerializeField] private int moveSpeed = 230;

    [SerializeField] private LayerMask hitTheseLayers;
    [SerializeField] private string damageTag;

    [SerializeField] private int bulletDamage = 10;


    void Update ()
    {
        transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        Destroy(gameObject, 1 / Time.timeScale);
    }

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        Debug.Log("Bullet collided with: " + collision.gameObject.name);


        if (collision.CompareTag(damageTag))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 forceDirection = transform.right; // Assuming the bullet moves to the right
                float forceAmount = 100f; // Adjust this value as needed
                rb.AddForce(forceDirection * forceAmount, ForceMode2D.Impulse);
            }

            collision.GetComponent<ICharacter>().TakeDamage(bulletDamage);
            Destroy(gameObject);
        }


        if (hitTheseLayers == (hitTheseLayers | (1 << collision.gameObject.layer)))
        {
            Destroy(gameObject);
        }
    }

    public void SetDamage(int damage)
    {
        bulletDamage = damage;
    }

    public void SetTagToDamage ( string tag )
    {
        damageTag = tag;
    }

    public void SetBulletGradient ( Gradient newGradient )
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.colorGradient = newGradient;
    }
}
