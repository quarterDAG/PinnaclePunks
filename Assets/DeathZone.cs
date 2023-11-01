using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        if (collision != null)
        {
            ICharacter character = collision.GetComponent<ICharacter>();
            if (character != null)
            {
                character.Die();
            }
        }
    }


}
