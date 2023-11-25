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
    public GameObject playerAvatarPrefab;
    public Transform teamAStatusParent;
    public Transform teamBStatusParent;
    [SerializeField] private List<Vector2> teamAStatusPositions;
    [SerializeField] private List<Vector2> teamBStatusPositions;

    private int teamAPlayerCount = 0;
    private int teamBPlayerCount = 0;

    public InfinitePlatformGenerator platformGenerator;
    private Dictionary<int, GameObject> playerRespawnMarkers = new Dictionary<int, GameObject>();




    private void Start ()
    {
        PlayerManager.Instance.SetPlayerSpawner(this);
        GameManager.Instance.AddPlayerSpawner(this);
        PlayerManager.Instance.InitializeHeroes();
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
            playerPrefabs[config.selectedHero],
            controlScheme: config.controlScheme.ToString(),
            pairWithDevice: config.inputDevice,
            playerIndex: playerCount
        );

        instantiatedPlayer.GetComponent<InputManager>().SetCurrentInputDevice(config.inputDevice);

        // Set the instantiated player's position and rotation
        instantiatedPlayer.transform.position = spawnPoint.position;
        instantiatedPlayer.transform.SetParent(playersParent.transform, false);

        if (platformGenerator != null)
        {
            GameObject respawnMarker = new GameObject("RespawnMarker_" + playerCount);
            respawnMarker.transform.position = platformGenerator.GetLastPlatformPosition();
            respawnMarker.transform.position += new Vector3(0, 1.0f, 0); // Adjust the Y position
            playerRespawnMarkers[playerCount] = respawnMarker;
        }

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

    public void InstantiateHeroAvatarComponent ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        Vector2 statusPosition = GetPlayerStatusPosition(config);

        Transform parentGO = GetParentGO(config);

        // Instantiate player status and parent it to the position transform

        GameObject playerStatusGO = Instantiate(playerAvatarPrefab, parentGO, false);
        Bar hpBar = null;
        Bar shieldBar = null;
        Bar manaBar = null;

        List<Bar> barList = new List<Bar>();
        barList.AddRange(playerStatusGO.GetComponentsInChildren<Bar>());

        foreach (Bar bar in barList)
        {
            switch (bar.barType)
            {
                case Bar.BarType.HP:
                    hpBar = bar;
                    break;
                case Bar.BarType.Shield:
                    shieldBar = bar;
                    break;
                case Bar.BarType.Mana:
                    manaBar = bar;
                    break;
            }
        }

        HeroAvatarImageController heroAvatarImageController = playerStatusGO.GetComponentInChildren<HeroAvatarImageController>();
        heroAvatarImageController.UpdateHeroImageAvatar(config);

        // Ensure the GameObject is active before trying to modify RectTransform properties
        playerStatusGO.SetActive(true);

        // Set the first child's anchoredPosition to be zero relative to its parent if it exists
        if (playerStatusGO.transform.childCount > 0)
        {
            RectTransform HPAvatar = playerStatusGO.transform.GetChild(0).GetComponent<RectTransform>();
            HPAvatar.anchoredPosition = statusPosition;

            if (config.team == PlayerConfigData.Team.TeamB)
                HPAvatar.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }



        PlayerController playerController = instantiatedPlayer.GetComponent<PlayerController>();

        if (hpBar != null)
            playerController.AssignHPBar(hpBar);
        else
            Debug.Log("No HP bar found!");

        if (shieldBar != null)
            playerController.AssignShieldBar(shieldBar);
        else
            Debug.Log("No shield bar found!");

        if (manaBar != null)
            playerController.AssignManaBar(manaBar);
        else
            Debug.Log("No mana bar found!");


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

    public void UpdateAllPlayerRespawnPoints ()
    {
        Vector3 lastPlatformPosition = platformGenerator.GetLastPlatformPosition();
        lastPlatformPosition.y += 1.0f; // Adjust the Y position

        foreach (var kvp in playerRespawnMarkers)
        {
            kvp.Value.transform.position = lastPlatformPosition;
        }
    }

    public void RespawnPlayer ( PlayerConfig config, PlayerController playerController )
    {
        if (platformGenerator != null)
        {
            // Find the player and respawn marker
            if (playerRespawnMarkers.TryGetValue(config.playerIndex, out GameObject respawnMarker))
            {
                // Find the player with this playerCount
                foreach (Transform child in playersParent.transform)
                {
                    PlayerInput playerInput = child.GetComponent<PlayerInput>();
                    if (playerInput != null && playerInput.playerIndex == config.playerIndex)
                    {
                        child.position = respawnMarker.transform.position; // Respawn the player
                        break;
                    }
                }
            }
        }
        else
        {
            if (config.team == PlayerConfigData.Team.TeamA)
            {
                playerController.transform.position = teamASpawnPoints[config.playerIndex].position;
            }
            else
            {
                playerController.transform.position = teamBSpawnPoints[config.playerIndex].position;
            }
        }
    }

    private void OnDestroy ()
    {
        // Cleanup respawn markers
        foreach (var marker in playerRespawnMarkers.Values)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
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
