using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }

    public GameObject playerStatsPrefab;
    public Transform statsPanel;
    public TMPro.TextMeshProUGUI winningTeamAnnouncementText;

    public List<PlayerStats> allPlayerStats = new List<PlayerStats>();
    public List<PlayerStatsDisplayItem> playerStatsDisplayItems = new List<PlayerStatsDisplayItem>();

    public Canvas canvas {  get; private set; }

    private HashSet<int> playersVotedForRematch = new HashSet<int>();
    [SerializeField] private CountdownUI countdownUI;

    private void Awake ()
    {
        Singleton();
        canvas = GetComponent<Canvas>();
        Hide();
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

    private void OnDestroy ()
    {
        ResetAllStats();
    }

    private void ResetAllStats ()
    {
        foreach (PlayerStats playerStats in allPlayerStats)
        {
            playerStats.ResetStats();
        }
    }

    public void ClearAllStatsList()
    { allPlayerStats.Clear(); }

    public void UpdatePlayerStats ( PlayerStats playerStats, int playerIndex )
    {
        allPlayerStats.Insert(playerIndex, playerStats);
    }

    public void Hide ()
    {
        canvas.enabled = false;
        winningTeamAnnouncementText.enabled = false;
    }

    public void Show ()
    {
        canvas.enabled = true;
        winningTeamAnnouncementText.enabled = true;
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

    public void AddDamageToPlayerState ( float _damage, int _playerIndex )
    {
        if (_playerIndex >= 0) // (-1 = Shot by a monster)
            allPlayerStats[_playerIndex].damage += _damage;
    }

    private int CalculatePlayerScore ( PlayerStats stats )
    {
        if (stats.kills > 0)
        {
            // If there are kills, calculate K/D ratio and multiply by a factor for readability
            return stats.deaths == 0 ? stats.kills * 100 : (int)((float)stats.kills / stats.deaths * 100);
        }
        else
        {
            // If there are no kills, use the inverse of deaths as the score
            // Fewer deaths result in a higher score
            // Adding 1 to avoid division by zero and to ensure a player with 0 deaths gets the highest score
            return 1000 / (stats.deaths + 1);
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
        GameManager.Instance.StopTime(true);

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
        if (canvas.enabled == false) return;

        if (!playersVotedForRematch.Contains(playerIndex))
        {
            playersVotedForRematch.Add(playerIndex);
            UpdateReadyIcon(playerIndex, true);

            if (playersVotedForRematch.Count == allPlayerStats.Count)
            {
                // All players voted for a rematch
                RestartMatch();
            }
        }
    }

    private void UpdateReadyIcon ( int playerIndex, bool isReady )
    {
        PlayerStatsDisplayItem displayItem = playerStatsDisplayItems[playerIndex];
        if (displayItem != null)
        {
            displayItem.SetReady(isReady);
        }
    }

    #region Rematch & Reset
    private async void RestartMatch ()
    {
        winningTeamAnnouncementText.enabled = false;
        countdownUI.SetCountdownTime(5);
        countdownUI.StartTimer();

        await Task.Delay(5000);

        Hide();       
        ResetAllStats();
        ResetVotes();

        GameManager.Instance.Rematch();

    }

    private void ResetVotes ()
    {
        playersVotedForRematch.Clear();
        playerStatsDisplayItems.Clear();

        // Reset the ready status on each player's stats display
        foreach (var displayItem in playerStatsDisplayItems)
        {
            if (displayItem != null)
            {
                displayItem.SetReady(false);
            }
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    #endregion

}
