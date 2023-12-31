using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnCollisionEnter2D ( Collision2D collision )
    {
        if (collision.gameObject.CompareTag("Dodge")) return;


        ICharacter character = collision.gameObject.GetComponent<ICharacter>();
        if (character != null)
        {
            character.Die(-1);
        }

    }
}
