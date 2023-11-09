using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsDisplayItem : MonoBehaviour
{
    public Text playerNameText;
    public Text killsText;
    public Text deathsText;
    public Text damageText;

    public void Setup ( PlayerStats stats )
    {
        playerNameText.text = stats.playerName;
        killsText.text = stats.kills.ToString();
        deathsText.text = stats.deaths.ToString();
        damageText.text = stats.damage.ToString();
    }
}
