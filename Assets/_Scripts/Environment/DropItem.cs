using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

public class DropItem : MonoBehaviour
{
    public enum DropType { HP, Mana, Count } // Add other drop types as needed
    public DropType dropType;
    public int healAmount = 100;
    public float manaAmount = 100f;

    private int originalHealAmount;
    private float originalSmAmount;


    public float selfDestructTime = 5f; // Time in seconds after which the item self-destructs

    [SerializeField] private SpriteRenderer itemSpriteRenderer;
    private Animator animator;
    private ParticleSystem _ps;

    [SerializeField] private List<Sprite> itemSpriteList = new List<Sprite>();

    private AudioSource audioSource;
    [SerializeField] AudioClip bottleOpen;

    private void Start ()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        _ps = GetComponentInChildren<ParticleSystem>();

        ChooseRandomDropType();

        originalHealAmount = healAmount;
        originalSmAmount = manaAmount;

        StartCoroutine(FadeOutEffect());
    }

    private IEnumerator FadeOutEffect ()
    {
        float elapsedTime = 0;

        while (elapsedTime < selfDestructTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / selfDestructTime);
            itemSpriteRenderer.color = new Color(itemSpriteRenderer.color.r, itemSpriteRenderer.color.g, itemSpriteRenderer.color.b, alpha);

            // Interpolating the value of the drop item
            if (dropType == DropType.HP)
            {
                healAmount = (int)Mathf.Lerp(originalHealAmount, 0, elapsedTime / selfDestructTime);
            }
            else if (dropType == DropType.Mana)
            {
                manaAmount = Mathf.Lerp(originalSmAmount, 0, elapsedTime / selfDestructTime);
            }

            yield return null;
        }

        animator.SetBool("Pop", true);
        yield return new WaitForSeconds(0.35f);

        Destroy(gameObject);
    }



    private async void OnTriggerEnter2D ( Collider2D other )
    {
        if (other.CompareTag("TeamA") || other.CompareTag("TeamB"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                audioSource.PlayOneShot(bottleOpen);
                animator.SetBool("Pop", true);
                _ps.Play();
                ApplyEffect(player);
                await Task.Delay(350);
                Destroy(gameObject);

            }
        }
    }

    private void ChooseRandomDropType ()
    {
        dropType = (DropType)Random.Range(0, (int)DropType.Count);
        if (dropType == DropType.HP)
        {
            itemSpriteRenderer.sprite = itemSpriteList[0];
            SetParticlesColor(Color.red);
        }

        if (dropType == DropType.Mana)
        {
            itemSpriteRenderer.sprite = itemSpriteList[1];
            SetParticlesColor(Color.blue);
        }
    }

    private void SetParticlesColor ( Color color )
    {
        var main = _ps.main;
        main.startColor = color;
    }

    private void ApplyEffect ( PlayerController player )
    {
        // Implement the effect based on the type of drop
        switch (dropType)
        {
            case DropType.HP:
                // Increase player's health
                player.IncreaseHealth(healAmount);
                break;
            case DropType.Mana:
                // Increase player's stamina
                player.UpdateManaBar(manaAmount);
                break;

            default:
                break;
        }
    }
}
