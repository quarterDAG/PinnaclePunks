using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public List<Transform> teamASpawnPoints;
    public List<Transform> teamBSpawnPoints;
    private int teamASpawnIndex = 0;
    private int teamBSpawnIndex = 0;


    [Header("Prefab & Parent Settings")]
    public List<GameObject> playerPrefabs;
    public GameObject playersParent;

    [Header("Player Status")]
    public GameObject playerStatusPrefab;
    public Transform teamAStatusParent;
    public Transform teamBStatusParent;
    [SerializeField] private List<Vector2> teamAStatusPositions;
    [SerializeField] private List<Vector2> teamBStatusPositions;

    private int teamAPlayerCount = 0;
    private int teamBPlayerCount = 0;


    private void Start ()
    {
        PlayerManager.Instance.SetPlayerSpawner(this);
        PlayerManager.Instance.InitializePlayers();
    }

    public PlayerInput InstantiatePlayer ( PlayerConfig config, int playerCount )
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
            playerPrefabs[config.selectedPlayer],
            controlScheme: config.controlScheme.ToString(),
            pairWithDevice: config.inputDevice,
            playerIndex: playerCount
        );

        // Set the instantiated player's position and rotation
        instantiatedPlayer.transform.position = spawnPoint.position;
        instantiatedPlayer.transform.SetParent(playersParent.transform, false);

        // Assign player's respawn point
        instantiatedPlayer.GetComponent<PlayerController>().AssignRespawn(spawnPoint);

        return instantiatedPlayer;
    }

    private Transform GetSpawnPoint ( PlayerConfig config )
    {
        Transform spawnPoint = null;

        // Determine the spawn point based on the team
        if (config.team == PlayerConfigData.Team.TeamA)
        {
            if (teamASpawnIndex < teamASpawnPoints.Count)
            {
                spawnPoint = teamASpawnPoints[teamASpawnIndex];
                //teamASpawnIndex++;
            }
        }
        else if (config.team == PlayerConfigData.Team.TeamB)
        {
            if (teamBSpawnIndex < teamBSpawnPoints.Count)
            {
                spawnPoint = teamBSpawnPoints[teamBSpawnIndex];
                //teamBSpawnIndex++;
            }
        }

        return spawnPoint;
    }

    public void InstantiatePlayerStatusComponent ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        Vector2 statusPosition = GetPlayerStatusPosition(config);

        Transform parentGO = GetParentGO(config);

        // Instantiate player status and parent it to the position transform
        GameObject playerStatusGO = Instantiate(playerStatusPrefab, parentGO, false);
        Bar hpBar = playerStatusGO.GetComponentInChildren<Bar>();

        // Ensure the GameObject is active before trying to modify RectTransform properties
        playerStatusGO.SetActive(true);

        // Set the first child's anchoredPosition to be zero relative to its parent if it exists
        if (playerStatusGO.transform.childCount > 0)
        {
            RectTransform HPAvatar = playerStatusGO.transform.GetChild(0).GetComponent<RectTransform>();
            HPAvatar.anchoredPosition = statusPosition; // Reset the anchored position to (0,0)
        }

        // Link player status to the player
        if (hpBar != null)
        {
            instantiatedPlayer.GetComponent<PlayerController>().AssignHPBar(hpBar);
        }

        Transform playerColorTransform = playerStatusGO.transform.Find("HP & AVATAR/PlayerColor");
        Image playerColorImage = playerColorTransform.GetComponent<Image>();
        playerColorImage.color = config.playerColor; // Set the color to the one specified in config

        instantiatedPlayer.GetComponentInChildren<MouseAim>().GetComponent<SpriteRenderer>().color = config.playerColor;

        instantiatedPlayer.transform.Find("Indicator").GetComponent<SpriteRenderer>().color = config.playerColor;
    }

    private Vector2 GetPlayerStatusPosition ( PlayerConfig config )
    {
        int index;
        List<Vector2> statusPositions;

        if (config.team == PlayerConfigData.Team.TeamA)
        {
            statusPositions = teamAStatusPositions;
            index = teamAPlayerCount % statusPositions.Count;
            teamAPlayerCount++;
        }
        else // Team B
        {
            statusPositions = teamBStatusPositions;
            index = teamBPlayerCount % statusPositions.Count;
            teamBPlayerCount++;
        }

        if (statusPositions.Count == 0)
        {
            Debug.LogError("No status positions defined for team: " + config.team);
            return Vector2.zero;
        }

        return statusPositions[index];
    }

    private Transform GetParentGO ( PlayerConfig config )
    {
        return config.team == PlayerConfigData.Team.TeamA ? teamAStatusParent : teamBStatusParent;
    }

    #region Reset 
    public void ResetPlayerSpawner ()
    {
        // Reset spawn indices
        teamASpawnIndex = 0;
        teamBSpawnIndex = 0;

        // Reset player counts
        teamAPlayerCount = 0;
        teamBPlayerCount = 0;

        // Optional: Handle existing players and their status components
        // This might involve deactivating or destroying them
        ResetPlayersAndStatuses();
    }

    private void ResetPlayersAndStatuses ()
    {
        // Assuming all players are child objects of 'playersParent'
        foreach (Transform child in playersParent.transform)
        {
            // Here you can choose to deactivate or destroy the players
            // Deactivate: child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // Reset player status components for both teams
        ResetStatusComponents(teamAStatusParent);
        ResetStatusComponents(teamBStatusParent);
    }

    private void ResetStatusComponents ( Transform statusParent )
    {
        foreach (Transform child in statusParent)
        {
            // Similar to players, deactivate or destroy status components
            // Deactivate: child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    #endregion
}
