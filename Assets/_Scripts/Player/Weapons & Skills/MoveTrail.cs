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

    private int shotOwnerIndex;


    void Update ()
    {
        transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        Destroy(gameObject, 1 / Time.timeScale);
    }

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        // Check for damage tag and if the collision has an ICharacter component

        if (collision.CompareTag(damageTag) && collision.GetComponent<ICharacter>() != null)
        {
            collision.GetComponent<ICharacter>().TakeDamage(bulletDamage, shotOwnerIndex);

           
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

    public void SetPlayerOwnerIndex(int _playerIndex)
    {
        shotOwnerIndex = _playerIndex;
    }

    public void SetBulletGradient ( Gradient newGradient )
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.colorGradient = newGradient;
    }
}
