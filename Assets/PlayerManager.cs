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
    }

    public enum ControlScheme
    {
        Keyboard,
        Gamepad
    }

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public List<PlayerConfig> playerConfigs;

    private int gamepadIndex = 0;


    void Start ()
    {
        InitializePlayers();
    }

    private void InitializePlayers ()
    {
        var gamepads = Gamepad.all;
        int gamepadIndex = 0; // Index to keep track of assigned gamepads

        foreach (var config in playerConfigs)
        {
            PlayerInput instantiatedPlayer = null;

            if (config.controlScheme == ControlScheme.Keyboard)
            {
                // Instantiate player with keyboard and mouse.
                instantiatedPlayer = PlayerInput.Instantiate(
                    playerPrefab,
                    controlScheme: "Keyboard", // This name should match the one in the Input Actions Asset
                    pairWithDevices: new InputDevice[] { Keyboard.current, Mouse.current }
                );
            }
            else if (config.controlScheme == ControlScheme.Gamepad && gamepadIndex < gamepads.Count)
            {
                // Instantiate player with the current gamepad.
                instantiatedPlayer = PlayerInput.Instantiate(
                    playerPrefab,
                    controlScheme: "Gamepad", // This name should match the one in the Input Actions Asset
                    pairWithDevice: gamepads[gamepadIndex]
                );

                gamepadIndex++; // Increment the index for the next gamepad.
            }

            // After instantiation, switch the control scheme if necessary.
            if (instantiatedPlayer != null)
            {
                string controlSchemeName = config.controlScheme.ToString();
                InputDevice[] devices = config.controlScheme == ControlScheme.Keyboard ?
                    new InputDevice[] { Keyboard.current, Mouse.current } :
                    new InputDevice[] { gamepads[gamepadIndex - 1] };

                instantiatedPlayer.SwitchCurrentControlScheme(
                    controlSchemeName,
                    devices
                );
            }
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






