using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using System.Linq;

public class PlayerManager : MonoBehaviour
{

    public static PlayerManager Instance { get; private set; }

    [Serializable]
    public struct PlayerConfig
    {
        public ControlScheme controlScheme;
        public InputDevice device;
        public Team team;
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

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public List<PlayerConfig> playerConfigs;

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

       /* foreach (var gamepad in gamepads)
        {
            Debug.Log("Gamepad: " + gamepad.name);
        }*/

        int gamepadIndex = 0;
        int teamASpawnIndex = 0; 
        int teamBSpawnIndex = 0; 

        foreach (var config in playerConfigs)
        {
            PlayerInput instantiatedPlayer = null;
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
            instantiatedPlayer = PlayerInput.Instantiate(
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
        }
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






