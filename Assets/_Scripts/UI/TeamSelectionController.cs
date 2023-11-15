using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PlayerConfigData;

public class TeamSelectionController : MonoBehaviour
{
    public int MaxPlayersPerTeam = 2;

    [SerializeField] private Collider2D TeamAreaA;
    [SerializeField] private Collider2D TeamAreaB;

    [SerializeField] private List<Transform> iconPositionsA;
    [SerializeField] private List<Transform> iconPositionsB;

    private int teamPositionIndexA;
    private int teamPositionIndexB;

    [SerializeField] private CountdownUI countdownUI;

    [SerializeField] private string nextScene = "HeroSelect";

    private void Start ()
    {
        countdownUI.OnCountdownFinished += StartGame;
    }

    private void OnDestroy ()
    {
        countdownUI.OnCountdownFinished -= StartGame;
    }


    public bool CanJoinTeam ( Team team )
    {
        return CountTeamMembers(team) < MaxPlayersPerTeam;
    }

    private int CountTeamMembers ( Team team )
    {
        return PlayerManager.Instance.GetPlayerConfigList().FindAll(p => p.team == team).Count;
    }


    public void SetPlayerTeam ( int playerIndex, Team team, Transform icon )
    {
        var playerConfig = PlayerManager.Instance.GetPlayerConfig(playerIndex);
        if (playerConfig.playerState == PlayerState.Ready) return;

        Team currentTeam = playerConfig.team;

        // Set the team of the player in the PlayerManager
        PlayerManager.Instance.SetTeam(playerIndex, team);

        // Update the collider handling logic
        HandleTeamAreasColliders();

        // If the player is switching teams, handle the icon's position and layer
        if (currentTeam != team)
        {
            if (currentTeam == Team.TeamB && teamPositionIndexA > 0)
                teamPositionIndexA--;
            else if (currentTeam == Team.TeamA && teamPositionIndexB > 0)
                teamPositionIndexB--;

            // Set the new position of the icon based on the team
            if (team == Team.TeamA)
            {
                icon.position = iconPositionsA[teamPositionIndexA++].position;
                icon.gameObject.layer = LayerMask.NameToLayer("Default");
            }
            else if (team == Team.TeamB)
            {
                icon.position = iconPositionsB[teamPositionIndexB++].position;
                icon.gameObject.layer = LayerMask.NameToLayer("Default");
            }
            // Handle the case when a player is set to Spectator
            else if (team == Team.Spectator)
            {
                icon.gameObject.layer = LayerMask.NameToLayer("Spectators");
            }
        }
    }




    public void HandleTeamAreasColliders ()
    {
        if (!CanJoinTeam(Team.TeamA))
            TeamAreaA.forceSendLayers = 1 << 10;
        else
            TeamAreaA.forceSendLayers = 0;
        if (!CanJoinTeam(Team.TeamB))
            TeamAreaB.forceSendLayers = 1 << 10;
        else
            TeamAreaB.forceSendLayers = 0;
    }


    public void SetPlayerReady ( int playerIndex )
    {
        //Debug.Log($"Player {playerIndex} is ready!");

        PlayerManager.Instance.SetPlayerState(playerIndex, PlayerState.Ready);

        if(TwoPlayersAreReady())
        {
            StartGameCountdown();
        }
    }

    public void SetPlayerChoosingTeam ( int playerIndex )
    {
        //Debug.Log($"Player {playerIndex} is choosing a team.");

        PlayerManager.Instance.SetPlayerState(playerIndex, PlayerState.ChoosingTeam);

        if (!TwoPlayersAreReady())
        {
            StopGameCoundown();
        }
    }

    private bool TwoPlayersAreReady ()
    {
        List<PlayerConfig> playerConfigs = PlayerManager.Instance.GetPlayerConfigList();

        // Use LINQ to count how many players are ready
        int readyPlayersCount = playerConfigs.Count(p => p.playerState == PlayerState.Ready);

        return readyPlayersCount >= 2;
    }

    private void StartGameCountdown()
    {
        countdownUI.StartTimer();
    }

    private void StopGameCoundown()
    {
        countdownUI.StopTimer();
    }

    public void StartGame ()
    {

        //Start Button
        SceneManager.LoadScene(nextScene);
    }



}
