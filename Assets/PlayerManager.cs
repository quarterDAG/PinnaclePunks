using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

public class PlayerManager : MonoBehaviour
{

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

    void Start ()
    {
        InitializePlayers();
    }

    private void InitializePlayers ()
    {
        var gamepads = Gamepad.all;
        int gamepadIndex = 0;

        foreach (var config in playerConfigs)
        {
            PlayerInput instantiatedPlayer = null;

            if (config.controlScheme == ControlScheme.Keyboard)
            {
                instantiatedPlayer = PlayerInput.Instantiate(
                    playerPrefab,
                    controlScheme: "Keyboard",
                    pairWithDevices: new InputDevice[] { Keyboard.current, Mouse.current }
                );
            }
            else if (config.controlScheme == ControlScheme.Gamepad && gamepadIndex < gamepads.Count)
            {
                instantiatedPlayer = PlayerInput.Instantiate(
                    playerPrefab,
                    controlScheme: "Gamepad",
                    pairWithDevice: gamepads[gamepadIndex]
                );

                gamepadIndex++;
            }

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






