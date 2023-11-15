using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HeroSelectManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public List<Transform> avatarsTeamA;
    public List<Transform> avatarsTeamB;
    private int teamASpawnIndex = 0;
    private int teamBSpawnIndex = 0;

    public Transform teamAParent;
    public Transform teamBParent;


    [Header("Prefab & Parent Settings")]
    public GameObject selectorPrefab;


    private void Start ()
    {
        PlayerManager.Instance.SetHeroSelectManager(this);
        PlayerManager.Instance.InitializeSelectors();
    }

    public PlayerInput InstantiateSelector ( PlayerConfig config, int playerCount )
    {
        Transform spawnPoint = GetSpawnPoint(config);

        if (config.team == PlayerConfigData.Team.TeamA)
        {
            teamASpawnIndex++;
        }
        else if (config.team == PlayerConfigData.Team.TeamB)
        {
            teamBSpawnIndex++;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("No available spawn points for team: " + config.team);
            return null; // Return null if no spawn point is available
        }


        // Instantiate player at the spawn point
        PlayerInput instantiatedPlayer = PlayerInput.Instantiate(
            selectorPrefab,
            controlScheme: config.controlScheme.ToString(),
            pairWithDevice: config.inputDevice,
            playerIndex: playerCount
        );

        // Set the instantiated player's position and rotation
        instantiatedPlayer.transform.position = spawnPoint.position;
        instantiatedPlayer.transform.SetParent(GetParentGO(config), false);
        instantiatedPlayer.transform.localScale = new Vector3(4, 4, 4);

        SetupHeroSelector(config, instantiatedPlayer);

        return instantiatedPlayer;
    }

    private void SetupHeroSelector ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        HeroSelector heroSelector = instantiatedPlayer.GetComponent<HeroSelector>();
        heroSelector.SetHeroSelectManager(this);
        heroSelector.SetPlayerConfig(config);
        heroSelector.GetHeroAvatarList();
        heroSelector.ColorFrame(config);
        heroSelector.MoveSelectorToHero(0);
    }

    private Transform GetSpawnPoint ( PlayerConfig config )
    {
        Transform spawnPoint = null;

        // Determine the spawn point based on the team
        if (config.team == PlayerConfigData.Team.TeamA)
        {
            if (teamASpawnIndex < avatarsTeamA.Count)
            {
                spawnPoint = avatarsTeamA[teamASpawnIndex];
                //teamASpawnIndex++;
            }
        }
        else if (config.team == PlayerConfigData.Team.TeamB)
        {
            if (teamBSpawnIndex < avatarsTeamB.Count)
            {
                spawnPoint = avatarsTeamB[teamBSpawnIndex];
                //teamBSpawnIndex++;
            }
        }

        return spawnPoint;
    }


    private Transform GetParentGO ( PlayerConfig config )
    {
        return config.team == PlayerConfigData.Team.TeamA ? teamAParent : teamBParent;
    }

    public List<Transform> GetTeamAvatarList ( PlayerConfigData.Team team )
    {
        if (team == PlayerConfigData.Team.TeamA) { return avatarsTeamA; }
        else { return avatarsTeamB; }
    }


}
