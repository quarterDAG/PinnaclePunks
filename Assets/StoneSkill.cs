using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSkill : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();
    [SerializeField] private int shooterIndex = -1;
    [SerializeField] private string damageThisTag;


    void OnTriggerEnter2D ( Collider2D other )
    {
        if (!hitEnemies.Contains(other) && (other.CompareTag(damageThisTag) || other.CompareTag("DropBat")))
        {
            if (damage > 0 && shooterIndex >= 0)
            {
                ICharacter character = other.GetComponent<ICharacter>();
                if (character != null)
                {
                    character.TakeDamage(damage, shooterIndex);
                    hitEnemies.Add(other); // Ensure each enemy is only hit once
                }
            }
        }
    }

    void OnEnable ()
    {
        hitEnemies.Clear(); // Clear the set when the stone is enabled (in case of object pooling)
    }

    public void SetShooterIndex ( int _shooterIndex )
    { shooterIndex = _shooterIndex; }

    public void SetDamage ( int _damage )
    { damage = _damage; }

    public void SetTagToDamage ( string _tag )
    { damageThisTag = _tag; }
}
