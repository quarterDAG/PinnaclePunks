using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTrail : MonoBehaviour
{

    [SerializeField] private int moveSpeed = 230;

    [SerializeField] private LayerMask destroyOnTheseLayers;

    [SerializeField] private int bulletDamage = 10;


    void Update ()
    {
        transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        Destroy(gameObject, 1 / Time.timeScale);
    }

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        Debug.Log("Bullet collided with: " + collision.gameObject.name);

        if (destroyOnTheseLayers == (destroyOnTheseLayers | (1 << collision.gameObject.layer)))
        {
           Destroy(gameObject);
        }

        if(collision.CompareTag("Enemy"))
        {
            collision.GetComponent<Enemy>().DamageEnemy(bulletDamage);
        }
    }
}
