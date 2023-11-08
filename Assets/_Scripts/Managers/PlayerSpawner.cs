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
    public GameObject playerPrefab;
    public GameObject playersParent;

    [Header("Player Status")]
    public GameObject playerStatusPrefab;
    public Transform teamAStatusParent;
    public Transform teamBStatusParent;
    [SerializeField] private List<Vector2> teamAStatusPositions;
    [SerializeField] private List<Vector2> teamBStatusPositions;



    private void Start ()
    {
        PlayerManager.Instance.SetPlayerSpawner(this);
        PlayerManager.Instance.InitializePlayers();

    }

    public PlayerInput InstantiatePlayer ( PlayerConfig config, int playerCount )
    {
     
        Transform spawnPoint = GetSpawnPoint(config);

        if (spawnPoint == null)
        {
            Debug.LogError("No available spawn points for team: " + config.team);
            return null; // Return null if no spawn point is available
        }


        // Instantiate player at the spawn point
        PlayerInput instantiatedPlayer = PlayerInput.Instantiate(
            playerPrefab,
            controlScheme: config.controlScheme.ToString(),
            pairWithDevice: config.inputDevice,
            playerIndex: playerCount
        );

        // Set the instantiated player's position and rotation
        instantiatedPlayer.transform.position = spawnPoint.position;
        instantiatedPlayer.transform.SetParent(playersParent.transform, false);

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
                spawnPoint = teamASpawnPoints[teamASpawnIndex++];
            }
        }
        else if (config.team == PlayerConfigData.Team.TeamB)
        {
            if (teamBSpawnIndex < teamBSpawnPoints.Count)
            {
                spawnPoint = teamBSpawnPoints[teamBSpawnIndex++];
            }
        }

        return spawnPoint;
    }

    public void InstantiatePlayerStatusComponent ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        Vector2 statusPosition = GetPlayerStatusPosition(config);
        Transform parentGO = GetParentGO(config);

        Debug.Log($"Instantiating status for player {config.playerIndex} on {config.team}");


        // Instantiate player status and parent it to the position transform
        GameObject playerStatusGO = Instantiate(playerStatusPrefab, parentGO.position, Quaternion.identity);
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
        List<Vector2> statusPositions = config.team == PlayerConfigData.Team.TeamA ? teamAStatusPositions : teamBStatusPositions;
        if (config.playerIndex >= 0 && config.playerIndex < statusPositions.Count)
        {
            return statusPositions[config.playerIndex];
        }
        else
        {
            Debug.LogError("Player index out of range for status positions.");
            return statusPositions[config.playerIndex];
        }
    }

    private Transform GetParentGO ( PlayerConfig config )
    {
        return config.team == PlayerConfigData.Team.TeamA ? teamAStatusParent : teamBStatusParent;
    }
}
