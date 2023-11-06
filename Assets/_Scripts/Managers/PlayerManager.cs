using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;
using System.Drawing;
using static PlayerManager;

public class PlayerManager : MonoBehaviour
{

    public static PlayerManager Instance { get; private set; }

    [Serializable]
    public struct PlayerConfig
    {
        public int playerIndex;
        public ControlScheme controlScheme;
        public InputDevice device;
        public Team team;
        public PlayerColor playerColor;
    }

    public enum ControlScheme
    {
        Keyboard,
        Gamepad
    }

    public enum Team
    {
        TeamA,
        TeamB
    }

    public enum PlayerColor
    {
        Blue,
        Red,
        Green,
        Yellow
    }

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public List<PlayerConfig> playerConfigs;

    [Header("Player Status")]
    public GameObject playerStatusPrefab;
    public Transform teamAStatusParent;
    public Transform teamBStatusParent;
    [SerializeField] private List<Vector2> teamAStatusPositions;
    [SerializeField] private List<Vector2> teamBStatusPositions;

    public int playerCount { get; private set; }

    [Header("Spawn Points")]
    public List<Transform> teamASpawnPoints;
    public List<Transform> teamBSpawnPoints;

    [Header("Hierarchy Settings")]
    public GameObject playersParent;

    [Header("Camera Settings")]
    public List<Camera> playerCameras; // Assign individual player cameras here
    [SerializeField] private Cinemachine.CinemachineTargetGroup cinemachineTargetGroup;
    [SerializeField] private float proximityThreshold = 50f; // Distance at which cameras switch
    [SerializeField] private Canvas dividerCanvas;



    private void Awake ()
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

    void Start ()
    {
        InitializePlayers();
    }

    private void Update ()
    {
        CheckPlayersProximity();
    }

    private void CheckPlayersProximity ()
    {
        bool allPlayersClose = true;

        for (int i = 0; i < playerConfigs.Count; i++)
        {
            for (int j = i + 1; j < playerConfigs.Count; j++)
            {
                float distance = Vector3.Distance(playerCameras[i].transform.parent.position, playerCameras[j].transform.parent.position);
                if (distance > proximityThreshold)
                {
                    allPlayersClose = false;
                    break;
                }
            }
            if (!allPlayersClose) break;
        }

        // Enable/disable cameras based on players' proximity
        foreach (var cam in playerCameras)
        {
            cam.enabled = !allPlayersClose;
        }
        dividerCanvas.enabled = !allPlayersClose;
    }


    private void InitializePlayers ()
    {
        var gamepads = Gamepad.all;
        int gamepadIndex = 0;

        int teamASpawnIndex = 0;
        int teamBSpawnIndex = 0;

        foreach (var config in playerConfigs)
        {
            Transform spawnPoint = null;

            // Determine the spawn point based on the team
            if (config.team == Team.TeamA && teamASpawnIndex < teamASpawnPoints.Count)
            {
                spawnPoint = teamASpawnPoints[teamASpawnIndex++];
            }
            else if (config.team == Team.TeamB && teamBSpawnIndex < teamBSpawnPoints.Count)
            {
                spawnPoint = teamBSpawnPoints[teamBSpawnIndex++];
            }

            if (spawnPoint == null)
            {
                Debug.LogError("No available spawn points for team: " + config.team);
                continue; // Skip this iteration if no spawn point is available
            }

            // Instantiate player at the spawn point
            PlayerInput instantiatedPlayer = PlayerInput.Instantiate(
                playerPrefab,
                controlScheme: config.controlScheme.ToString(),
                pairWithDevice: config.controlScheme == ControlScheme.Gamepad ? gamepads[gamepadIndex++] : null,
                playerIndex: playerCount
            );

            // Set the instantiated player's position and rotation
            instantiatedPlayer.transform.position = spawnPoint.position;
            instantiatedPlayer.transform.SetParent(playersParent.transform, false);
            instantiatedPlayer.GetComponent<PlayerController>().AssignRespawn(spawnPoint);

            playerCount++;

            // Set up other player components
            AddPlayerToCinemachineTargetGroup(instantiatedPlayer.transform);
            instantiatedPlayer.GetComponent<InputManager>().UpdateCurrentControlScheme(config.controlScheme.ToString());
            SetupPlayerTag(config, instantiatedPlayer);
            instantiatedPlayer.GetComponentInChildren<PlayerMonsterSpawner>().ConfigMonsterSpawner();

            AddPlayerCameraToPlayerCameras(instantiatedPlayer);

            // Instantiate player status component
            InstantiatePlayerStatusComponent(config, instantiatedPlayer);
        }
    }
    private void InstantiatePlayerStatusComponent ( PlayerConfig config, PlayerInput instantiatedPlayer )
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


        // Find the PlayerColor GameObject and change its color
        UnityEngine.Color unityColor = GetColorFromEnum(config.playerColor);

        Transform playerColorTransform = playerStatusGO.transform.Find("HP & AVATAR/PlayerColor");
        Image playerColorImage = playerColorTransform.GetComponent<Image>();
        playerColorImage.color = unityColor; // Set the color to the one specified in config

        instantiatedPlayer.GetComponentInChildren<MouseAim>().GetComponent<SpriteRenderer>().color = unityColor;

        instantiatedPlayer.transform.Find("Indicator").GetComponent<SpriteRenderer>().color = unityColor;



    }

    public static UnityEngine.Color GetColorFromEnum ( PlayerColor playerColor )
    {
        switch (playerColor)
        {
            case PlayerColor.Blue:
                return UnityEngine.Color.blue;
            case PlayerColor.Red:
                return UnityEngine.Color.red;
            case PlayerColor.Green:
                return UnityEngine.Color.green;
            case PlayerColor.Yellow:
                return UnityEngine.Color.yellow;
            default:
                return UnityEngine.Color.white; // Default color if none is matched
        }
    }

    private Vector2 GetPlayerStatusPosition ( PlayerConfig config )
    {
        List<Vector2> statusPositions = config.team == Team.TeamA ? teamAStatusPositions : teamBStatusPositions;
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
        return config.team == Team.TeamA ? teamAStatusParent : teamBStatusParent;
    }

    private void AddPlayerCameraToPlayerCameras ( PlayerInput instantiatedPlayer )
    {
        Camera playerCamera = instantiatedPlayer.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            playerCameras.Add(playerCamera); // Add the camera to the list
        }
        else
        {
            Debug.LogWarning("No camera found in the player's children. Make sure the player prefab has a camera.");
        }
    }

    private void AddPlayerToCinemachineTargetGroup ( Transform playerTransform )
    {
        var target = new Cinemachine.CinemachineTargetGroup.Target
        {
            target = playerTransform,
            weight = 1f,
            radius = 5f
        };

        cinemachineTargetGroup.AddMember(playerTransform, target.weight, target.radius);
    }

    private void SetupPlayerTag ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        string tag = config.team.ToString();
        instantiatedPlayer.gameObject.tag = tag;
        SetTagRecursively(instantiatedPlayer.gameObject.transform, tag);
    }

    void SetTagRecursively ( Transform parent, string tag )
    {
        parent.gameObject.tag = tag;
        foreach (Transform child in parent)
        {
            SetTagRecursively(child, tag); // Recursive call for all children
        }
    }

}



#if UNITY_EDITOR

[CustomEditor(typeof(PlayerManager))]
public class PlayerManagerEditor : Editor
{
    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI();

        PlayerManager setupScript = (PlayerManager)target;

    }
}
#endif






