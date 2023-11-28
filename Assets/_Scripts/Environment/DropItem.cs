using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

public class DropItem : MonoBehaviour
{

    [Header("General Settings")]
    [SerializeField] private DropType dropType;
    [SerializeField]
    private enum DropType
    {
        HP,
        Mana,
        IncreaseFireRate,
        IncreaseFireDamage,
        IncreaseSpeed,
        Count
    }
    [SerializeField] private float selfDestructTime = 5f;
    [SerializeField] private SpriteRenderer bubbleSpriteRenderer;
    [SerializeField] private SpriteRenderer itemSpriteRenderer;

    [Header("Power Ups")]
    [SerializeField] private float powerupDuration = 7f;
    [SerializeField] private float fireRateMultiplier = 1.5f;
    [SerializeField] private float fireDamageMultiplier = 1.5f;
    [SerializeField] private float speedMultiplier = 1.5f;


    private int healAmount = 100;
    private float manaAmount = 100f;

    private int originalHealAmount;
    private float originalSmAmount;

    private ParticleSystem _ps;

    [SerializeField] private List<Sprite> itemSpriteList = new List<Sprite>();

    private AudioSource audioSource;
    [SerializeField] AudioClip bottleOpen;

    private bool isTaken;

    private void Start ()
    {
        bubbleSpriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        _ps = GetComponentInChildren<ParticleSystem>();

        SetParticlesColor(Color.green);
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

        //animator.SetBool("Pop", true);
        yield return new WaitForSeconds(0.35f);

        Destroy(gameObject);
    }



    private void OnTriggerEnter2D ( Collider2D other )
    {

        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            Pop(player);

        }

    }


    public async void Pop ( PlayerController player )
    {
        if (isTaken) return;

        isTaken = true;
        audioSource.PlayOneShot(bottleOpen);
        _ps.Play();

        itemSpriteRenderer.enabled = false;
        bubbleSpriteRenderer.enabled = false;

        if (player != null)
            ApplyEffect(player);

        await Task.Delay(350);
        Destroy(gameObject);
    }

    private void ChooseRandomDropType ()
    {
        dropType = (DropType)Random.Range(0, (int)DropType.Count);

        switch (dropType)
        {
            case DropType.HP:
                itemSpriteRenderer.sprite = itemSpriteList[0];
                SetParticlesColor(Color.red);
                break;

            case DropType.Mana:
                itemSpriteRenderer.sprite = itemSpriteList[1];
                SetParticlesColor(Color.blue);
                break;

            case DropType.IncreaseFireRate:
                itemSpriteRenderer.sprite = itemSpriteList[2]; // Ensure this is the correct index
                break;

            case DropType.IncreaseFireDamage:
                itemSpriteRenderer.sprite = itemSpriteList[3];
                break;

            case DropType.IncreaseSpeed:
                itemSpriteRenderer.sprite = itemSpriteList[4];
                break;

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

            case DropType.IncreaseFireRate:
                player.powerUpImage.ActivatePowerUp(0, powerupDuration);
                player.GetComponentInChildren<IWeapon>().IncreaseFireRate(fireRateMultiplier, powerupDuration);
                break;

            case DropType.IncreaseFireDamage:
                player.powerUpImage.ActivatePowerUp(1, powerupDuration);
                player.GetComponentInChildren<IWeapon>().IncreaseFireDamage(fireDamageMultiplier, powerupDuration);
                break;

            case DropType.IncreaseSpeed:
                player.powerUpImage.ActivatePowerUp(2, powerupDuration);
                player.IncreaseSpeed(speedMultiplier, powerupDuration);
                break;

            default:
                break;
        }
    }
}
