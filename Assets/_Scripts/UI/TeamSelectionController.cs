using UnityEngine;
using UnityEngine.SceneManagement;
using static PlayerConfigData;

public class TeamSelectionController : MonoBehaviour
{
    public int MaxPlayersPerTeam = 2;

    [SerializeField] private Collider2D TeamAreaA;
    [SerializeField] private Collider2D TeamAreaB;


    public bool CanJoinTeam ( Team team )
    {
        return CountTeamMembers(team) < MaxPlayersPerTeam;
    }

    private int CountTeamMembers ( Team team )
    {
        return PlayerManager.Instance.GetPlayerConfigList().FindAll(p => p.team == team).Count;
    }



    public void SetPlayerTeam ( int playerIndex, Team team )
    {
        if (PlayerManager.Instance.GetPlayerConfig(playerIndex).playerState == PlayerState.Ready) return;

        // Find the PlayerConfig and update the team.
        int configIndex = PlayerManager.Instance.GetPlayerConfigList().FindIndex(p => p.playerIndex == playerIndex);
        //Debug.Log(configIndex);
        if (configIndex != -1)
        {
          PlayerManager.Instance.SetTeam(playerIndex, team);
        }

        HandleTeamAreasColliders();
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
        Debug.Log($"Player {playerIndex} is ready!");

        PlayerManager.Instance.SetPlayerState(playerIndex, PlayerState.Ready);
    }

    public void SetPlayerChoosingTeam ( int playerIndex )
    {
        Debug.Log($"Player {playerIndex} is choosing a team.");

        PlayerManager.Instance.SetPlayerState(playerIndex, PlayerState.ChoosingTeam);
    }

    public void StartGame ()
    {

        //Start Button
        SceneManager.LoadScene("GameScene");
    }



}
