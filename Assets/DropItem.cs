using UnityEngine;

public class DropItem : MonoBehaviour
{
    public enum DropType { HP, SM, Other, Count } // Add other drop types as needed
    public DropType dropType;
    public int effectAmount; // The amount of effect (like HP to add, SM to add, etc.)
    public float selfDestructTime = 5f; // Time in seconds after which the item self-destructs

    private SpriteRenderer spriteRenderer;

    private void Start ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        ChooseRandomDropType();
        Destroy(gameObject, selfDestructTime); // Schedule the destruction

    }
    private void OnTriggerEnter2D ( Collider2D other )
    {
        // Assuming the player has a tag "Player"
        if (other.CompareTag("TeamA") || other.CompareTag("TeamB"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                ApplyEffect(player);
                Destroy(gameObject); // Destroy the drop item after applying its effect

            }
        }
    }

    private void ChooseRandomDropType ()
    {
        dropType = (DropType)Random.Range(0, (int)DropType.Count);
        if(dropType == DropType.HP)
        {
            spriteRenderer.color = Color.blue;
        }
    }

    private void ApplyEffect ( PlayerController player )
    {
        // Implement the effect based on the type of drop
        switch (dropType)
        {
            case DropType.HP:
                // Increase player's health
                player.GetComponent<PlayerController>().IncreaseHealth(effectAmount);
                break;
            case DropType.SM:
                // Increase player's stamina
                //player.GetComponent<PlayerController>().IncreaseStamina(effectAmount);
                break;
            case DropType.Other:
                // Handle other types of drops
                break;
            default:
                break;
        }
    }
}
