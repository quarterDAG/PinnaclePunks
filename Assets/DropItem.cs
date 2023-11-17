using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DropItem : MonoBehaviour
{
    public enum DropType { HP, SM, Count } // Add other drop types as needed
    public DropType dropType;
    public int effectAmount; // The amount of effect (like HP to add, SM to add, etc.)
    public float selfDestructTime = 5f; // Time in seconds after which the item self-destructs

    [SerializeField] private SpriteRenderer itemSpriteRenderer;
    private Animator animator;
    private ParticleSystem ps;

    [SerializeField] private List<Sprite> itemSpriteList = new List<Sprite>();

    private void Start ()
    {
        animator = GetComponent<Animator>();
        ps = GetComponentInChildren<ParticleSystem>();

        ChooseRandomDropType();
        Destroy(gameObject, selfDestructTime); // Schedule the destruction

    }
    private async void OnTriggerEnter2D ( Collider2D other )
    {
        // Assuming the player has a tag "Player"
        if (other.CompareTag("TeamA") || other.CompareTag("TeamB"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                animator.SetBool("Pop", true);
                ps.Play();
                ApplyEffect(player);
                await Task.Delay(500);
                Destroy(gameObject); // Destroy the drop item after applying its effect

            }
        }
    }

    private void ChooseRandomDropType ()
    {
        dropType = (DropType)Random.Range(0, (int)DropType.Count);
        if (dropType == DropType.HP)
        {
            itemSpriteRenderer.sprite = itemSpriteList[0];
        }

        if (dropType == DropType.SM)
        {
            itemSpriteRenderer.sprite = itemSpriteList[1];
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

            default:
                break;
        }
    }
}
