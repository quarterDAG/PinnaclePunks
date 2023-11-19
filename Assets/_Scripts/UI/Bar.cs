using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    [SerializeField] private Image fillImage; // Reference to the fill image component
    [SerializeField] private TextMeshProUGUI percentageText;

    [SerializeField] private float maxValue = 100f; // Assuming max health is 100
    [SerializeField] private float currentValue = 100f;

    public enum BarType
    {
        HP,
        Slowmotion,
        Shield
    }

    public BarType barType;

    private void Start ()
    {
        if (barType == BarType.Slowmotion) { AddBarToGameManager(); }
        UpdateValue(currentValue);
    }

    public void SetValue ( float value )
    {
        currentValue = value;
    }

    public void UpdateValue ( float amount )
    {
        currentValue += amount;
        currentValue = Mathf.Clamp(currentValue, 0, maxValue);

        // Normalize the current value to be between 0 and 1.
        float normalizedValue = currentValue / maxValue;

        fillImage.fillAmount = normalizedValue;
    }

    public void AddBarToGameManager ()
    {
        GameManager.Instance.AddSMBar(this);
    }

    public void ResetBar ()
    {
        UpdateValue(maxValue);
    }

    public bool IsEmpty ()
    {
        return currentValue <= 0;
    }
}
