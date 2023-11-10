using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsDisplayItem : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI deathsText;
    public TextMeshProUGUI damageText;

    public GameObject readyIcon;

    public void Setup ( PlayerStats stats, Color color )
    {
        playerNameText.text = stats.playerName;
        killsText.text = stats.kills.ToString();
        deathsText.text = stats.deaths.ToString();
        damageText.text = stats.damage.ToString();

        // Set the color of each text element
        playerNameText.color = color;
        killsText.color = color;
        deathsText.color = color;
        damageText.color = color;
    }

    public void SetReady ( bool isReady )
    {
        readyIcon.GetComponent<Image>().enabled = isReady;
    }

}
