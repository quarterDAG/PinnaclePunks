using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

public class DropItem : MonoBehaviour
{
    public enum DropType { HP, SM, Count } // Add other drop types as needed
    public DropType dropType;
    public int healAmount = 30;
    public float smAmount = 40f;
    public float selfDestructTime = 5f; // Time in seconds after which the item self-destructs

    [SerializeField] private SpriteRenderer itemSpriteRenderer;
    private Animator animator;
    private ParticleSystem _ps;

    [SerializeField] private List<Sprite> itemSpriteList = new List<Sprite>();

    private void Start ()
    {
        animator = GetComponent<Animator>();
        _ps = GetComponentInChildren<ParticleSystem>();

        ChooseRandomDropType();

        StartCoroutine(FadeOutEffect());


        //Destroy(gameObject, selfDestructTime); // Schedule the destruction

    }

    private IEnumerator FadeOutEffect ()
    {
        float elapsedTime = 0;
        Color startColor = itemSpriteRenderer.color; // If using a SpriteRenderer
        // Color startColor = renderer.material.color; // If using a MeshRenderer

        while (elapsedTime < selfDestructTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / selfDestructTime);
            itemSpriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
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

        if (dropType == DropType.SM)
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
            case DropType.SM:
                // Increase player's stamina
                player.GetComponent<SlowmotionController>().UpdateSMBar(smAmount);
                break;

            default:
                break;
        }
    }
}
