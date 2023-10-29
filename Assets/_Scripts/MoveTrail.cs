using System.Collections;
using System.Collections.Generic;
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

        if (hitTheseLayers == (hitTheseLayers | (1 << collision.gameObject.layer)))
        {
           Destroy(gameObject);
        }

        if(collision.CompareTag(damageTag))
        {
            collision.GetComponent<ICharacter>().TakeDamage(bulletDamage);
            Destroy(gameObject);
        }
    }

    public void SetTagToDamage ( string tag )
    {
        damageTag = tag;
    }
}
