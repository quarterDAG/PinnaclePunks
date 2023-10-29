using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Image hpFillImage; // Reference to the fill image component
    private float maxHealth = 100f; // Assuming max health is 100

    public void UpdateHPFillUI ( float currentHealth )
    {
        // Calculate fill amount based on current health
        float fillAmount = currentHealth / maxHealth;

        // Update the fill image's fillAmount property
        hpFillImage.fillAmount = fillAmount;
    }
}
