using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Bar : MonoBehaviour
{
    [SerializeField] private Image fillImage; // Reference to the fill image component

    [SerializeField] private float maxValue = 100f; // Assuming max health is 100
    [SerializeField] private float currentValue = 100f;

    [SerializeField] private float manaRefillRate = 5f; // Mana refill rate per second
    private bool isRefillingMana = false;


    public enum BarType
    {
        HP,
        Mana,
        Shield
    }

    public BarType barType;

    private void Start ()
    {
        AddValue(0); // Initialize the bar value

        if (barType == BarType.Mana)
        {
            AddBarToGameManager();
        }

    }

    private void StartManaRefill ()
    {
        if (!isRefillingMana)
        {
            isRefillingMana = true;
            InvokeRepeating(nameof(RefillMana), 0.5f, 0.5f); // Refill 2 points per second
        }
    }

    private void RefillMana ()
    {
        if (currentValue < maxValue)
        {
            AddValue(1); // Increment by 1 point each half second
        }
        else
        {
            isRefillingMana = false;
            CancelInvoke(nameof(RefillMana)); // Stop refilling if max value is reached
        }
    }

    public void SetValue(float amount)
    {
        currentValue = amount;
    }

    public void AddValue ( float amount )
    {
        currentValue = Mathf.Clamp(currentValue + amount, 0, maxValue);
        UpdateUI();

        if (barType == BarType.Mana && currentValue < maxValue)
        {
            StartManaRefill();
        }
    }

    private void UpdateUI ()
    {
        if (fillImage != null)
            fillImage.fillAmount = currentValue / maxValue;
    }

    public void AddBarToGameManager ()
    {
        GameManager.Instance.AddSMBar(this);
    }

    public void ResetBar ()
    {
        AddValue(maxValue);
    }

    public bool IsEmpty ()
    {
        return currentValue <= 0;
    }
}
