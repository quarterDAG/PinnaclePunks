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



    private void Awake ()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This will ensure the instance is not destroyed between scene changes.
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // If another instance exists, destroy this one.
        }
    }

    void Start ()
    {
        InitializePlayers();
    }
    private void InitializePlayers ()
    {

        var gamepads = Gamepad.all;

        foreach (var gamepad in gamepads)
        {
            Debug.Log("Gamepad: " + gamepad.name);
        }


        int gamepadIndex = 0;
        int teamASpawnIndex = 0; // Index to track Team A spawns
        int teamBSpawnIndex = 0; // Index to track Team B spawns

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
            instantiatedPlayer.GetComponent<PlayerController>().AssignRespawn(spawnPoint);

            playerCount++;

            // Set up other player components
            instantiatedPlayer.GetComponent<InputManager>().UpdateCurrentControlScheme(config.controlScheme.ToString());
            SetupPlayerTag(config, instantiatedPlayer);
            instantiatedPlayer.GetComponentInChildren<PlayerMonsterSpawner>().ConfigMonsterSpawner();
        }
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






