using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }

    public GameObject playerStatsPrefab;
    public Transform statsPanel;
    public TMPro.TextMeshProUGUI winningTeamAnnouncementText;

    public List<PlayerStats> allPlayerStats = new List<PlayerStats>();
    public List<PlayerStatsDisplayItem> playerStatsDisplayItems = new List<PlayerStatsDisplayItem>();

    private Canvas canvas;

    private HashSet<int> playersVotedForRematch = new HashSet<int>();


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


    private int CalculatePlayerScore ( PlayerStats stats )
    {
        // Check for zero deaths to avoid division by zero
        if (stats.deaths == 0)
        {
            return stats.kills; // If no deaths, return kills as score
        }
        else
        {
            // Calculate K/D ratio and multiply by a factor, e.g., 100, for better readability
            return (int)((float)stats.kills / stats.deaths * 100);
        }
    }


    public PlayerStats DetermineWinner ()
    {
        PlayerStats winner = null;
        int highestScore = int.MinValue;

        foreach (PlayerStats playerStats in allPlayerStats)
        {
            int score = CalculatePlayerScore(playerStats);
            if (score > highestScore)
            {
                highestScore = score;
                winner = playerStats;
            }
        }

        return winner;
    }

    public void DisplayStats ()
    {
        SortPlayersByScore(); // Sort the players by score before displaying

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
            playerStatsDisplayItems.Add(displayItem);

            displayItem.Setup(stats, GetStatColor(stats));
        }

        Show();

    }


    private void SortPlayersByScore ()
    {
        allPlayerStats.Sort(( player1, player2 ) =>
            CalculatePlayerScore(player2).CompareTo(CalculatePlayerScore(player1)));
    }

    public PlayerConfigData.Team DetermineWinningTeam ()
    {
        int teamAScore = 0;
        int teamBScore = 0;

        foreach (PlayerStats playerStats in allPlayerStats)
        {
            int playerScore = CalculatePlayerScore(playerStats);

            if (playerStats.playerConfig.team == PlayerConfigData.Team.TeamA)
            {
                teamAScore += playerScore;
            }
            else if (playerStats.playerConfig.team == PlayerConfigData.Team.TeamB)
            {
                teamBScore += playerScore;
            }
        }

        // Determine the winning team based on the total scores
        if (teamAScore > teamBScore)
        {
            return PlayerConfigData.Team.TeamA;
        }
        else if (teamBScore > teamAScore)
        {
            return PlayerConfigData.Team.TeamB;
        }
        else
        {
            return PlayerConfigData.Team.Spectator; // Indicate a tie or no winner
        }
    }


    public void EndMatch ()
    {
        DisplayStats();
        PlayerConfigData.Team winningTeam = DetermineWinningTeam();

        string teamName = FormatTeamName(winningTeam);
        string announcementMessage = teamName == null ? "The match is a tie!" : teamName + " Wins!";

        if (winningTeamAnnouncementText != null)
        {
            winningTeamAnnouncementText.text = announcementMessage;
        }
        else
        {
            Debug.LogError("WinningTeamAnnouncementText is not set in the inspector");
        }
    }

    private string FormatTeamName ( PlayerConfigData.Team team )
    {
        switch (team)
        {
            case PlayerConfigData.Team.TeamA:
                return "Team A";
            case PlayerConfigData.Team.TeamB:
                return "Team B";
            default:
                return null; // For tie or Spectator
        }
    }


    public void VoteForRematch ( int playerIndex )
    {
        if(canvas.enabled == false) return;

        if (!playersVotedForRematch.Contains(playerIndex))
        {
            playersVotedForRematch.Add(playerIndex);
            UpdateReadyIcon(playerIndex, true);

            if (playersVotedForRematch.Count == allPlayerStats.Count)
            {
                // All players voted for a rematch
                StartCoroutine(RestartMatch());
            }
        }
    }

    private void UpdateReadyIcon ( int playerIndex, bool isReady )
    {
        // Assume you have a way to get the corresponding PlayerStatsDisplayItem for each player index
        PlayerStatsDisplayItem displayItem = playerStatsDisplayItems[playerIndex];
        if (displayItem != null)
        {
            displayItem.SetReady(isReady);
        }
    }

    private IEnumerator RestartMatch ()
    {
        // Add logic to restart the match
        yield return new WaitForSeconds(1); // Waiting time before restarting
        Debug.Log("REMATCH!!!!!!!!!!");
        // Reset game state and start a new match
    }



}
