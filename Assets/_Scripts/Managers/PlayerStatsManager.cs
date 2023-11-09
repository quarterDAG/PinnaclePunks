using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public GameObject playerStatsPrefab; // Your prefab
    public Transform statsPanel; // Parent panel in the UI

    // A list of PlayerStats ScriptableObjects
    public List<PlayerStats> allPlayerStats;

    // Call this at the end of the match
    public void DisplayStats ()
    {
        // Clear previous stats display
        foreach (Transform child in statsPanel)
        {
            Destroy(child.gameObject);
        }

        // Create a new stats display for each player
        foreach (PlayerStats stats in allPlayerStats)
        {
            GameObject statsItem = Instantiate(playerStatsPrefab, statsPanel);
            PlayerStatsDisplayItem displayItem = statsItem.GetComponent<PlayerStatsDisplayItem>();
            displayItem.Setup(stats);
        }
    }
}
