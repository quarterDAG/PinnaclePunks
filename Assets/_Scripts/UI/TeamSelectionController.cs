using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerConfigData;

public class TeamSelectionController : MonoBehaviour
{
    public int MaxPlayersPerTeam = 2;
    public List<PlayerConfig> playerConfigs;

    [SerializeField] private Collider2D TeamAreaA;
    [SerializeField] private Collider2D TeamAreaB;

    private void Awake ()
    {
        playerConfigs = new List<PlayerConfig>();
    }

    #region Getters

    public int GetUniquePlayerIndex ()
    {
        int newIndex = 0;

        // Loop through existing configs to find an unused index.
        while (playerConfigs.Any(p => p.playerIndex == newIndex))
        {
            newIndex++;
        }
        return newIndex;
    }

    private PlayerConfig GetPlayerConfig ( int playerIndex )
    {
        return playerConfigs.FirstOrDefault(pc => pc.playerIndex == playerIndex);
    }

    public bool CanJoinTeam ( PlayerConfigData.Team team )
    {
        Debug.Log("Team: " + team + CountTeamMembers(team));
        return CountTeamMembers(team) < MaxPlayersPerTeam;
    }

    private int CountTeamMembers ( PlayerConfigData.Team team )
    {
        return playerConfigs.FindAll(p => p.team == team).Count;
    }

    #endregion

    public void SetPlayerTeam ( int playerIndex, PlayerConfigData.Team team )
    {

        // Find the PlayerConfig and update the team.
        int configIndex = playerConfigs.FindIndex(p => p.playerIndex == playerIndex);
        //Debug.Log(configIndex);
        if (configIndex != -1)
        {
            // Extract the PlayerConfig, modify it, and then put it back.
            PlayerConfig playerConfig = playerConfigs[configIndex];
            playerConfig.team = team;
            playerConfigs[configIndex] = playerConfig;
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

        PlayerConfig config = GetPlayerConfig(playerIndex);

        config.playerState = PlayerState.Ready;
        UpdatePlayerConfig(playerIndex, config);
    }

    public void SetPlayerChoosingTeam ( int playerIndex )
    {
        Debug.Log($"Player {playerIndex} is choosing a team.");

        PlayerConfig config = GetPlayerConfig(playerIndex);

        config.playerState = PlayerState.ChoosingTeam;
        UpdatePlayerConfig(playerIndex, config);
    }

    private void UpdatePlayerConfig ( int playerIndex, PlayerConfig updatedConfig )
    {
        int configIndex = playerConfigs.FindIndex(pc => pc.playerIndex == playerIndex);
        if (configIndex != -1)
        {
            playerConfigs[configIndex] = updatedConfig;
        }
    }

}
