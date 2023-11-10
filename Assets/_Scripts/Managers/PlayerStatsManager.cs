using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }


    public GameObject playerStatsPrefab; // Your prefab
    public Transform statsPanel; // Parent panel in the UI

    // A list of PlayerStats ScriptableObjects
    public List<PlayerStats> allPlayerStats = new List<PlayerStats>();

    private Canvas canvas;

    private void Awake ()
    {
        Singleton();
        canvas = GetComponent<Canvas>();
        Hide();
    }

    private void OnDestroy ()
    {
        foreach (PlayerStats playerStats in allPlayerStats)
        {
            playerStats.ResetStats();
        }
    }

    private void Singleton ()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void UpdatePlayerStats ( PlayerStats playerStats, int playerIndex )
    {
        allPlayerStats.Insert(playerIndex, playerStats);
    }

    public void Hide ()
    {
        canvas.enabled = false;
    }

    public void Show ()
    {
        canvas.enabled = true;
    }

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

            displayItem.Setup(stats, GetStatColor(stats));
        }

        Show();
    }

    private Color GetStatColor ( PlayerStats playerStats )
    {
        switch (playerStats.playerName)
        {
            case "Red":
                return Color.red;
            case "Blue":
                return Color.blue;
            case "Green":
                return Color.green;
            case "Yellow":
                return Color.yellow;
            default:
                return Color.white;

        }
    }


    public void AddDamageToPlayerState ( int _damage, int _playerIndex )
    {
        if (_playerIndex >= 0) // (-1 = Shot by a monster)
            allPlayerStats[_playerIndex].damage += _damage;
    }
}
