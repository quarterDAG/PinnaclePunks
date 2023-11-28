using System.Collections.Generic;
using UnityEngine;
using static PlayerConfigData;
using UnityEngine.InputSystem;




[System.Serializable]
public struct PlayerConfig
{
    public int playerIndex;
    public string playerName;
    public Color playerColor;
    public Team team;
    public ControlScheme controlScheme;
    public PlayerState playerState;
    public InputDevice inputDevice;
    public int selectedHero;
}

[CreateAssetMenu(fileName = "PlayerConfigData", menuName = "Player/Player Config Data", order = 1)]

public class PlayerConfigData : ScriptableObject
{
    public List<PlayerConfig> playerConfigs = new List<PlayerConfig>(); // Now a list instead of an array

    public enum ControlScheme
    {
        Keyboard,
        Gamepad
    }

    public enum Team
    {
        FreeForAll,
        TeamA,
        TeamB,
        Spectator
    }

    public enum PlayerState
    {
        ChoosingTeam,
        SelectingHero,
        ChoosingMap,
        Ready,
        Playing
    }

    public List<Color> playerColors = new List<Color>();

/*
    // You can add methods to manage player configs if necessary
    public void AddPlayerConfig ( PlayerConfig config )
    {
        playerConfigs.Add(config);
    }*/

    public void RemovePlayerConfig ( PlayerConfig config )
    {
        playerConfigs.Remove(config);
    }

    // Call this method when a player joins the game.
    public PlayerConfig AddPlayerConfig (PlayerInput playerInput)
    {
        // Get the control scheme from the input manager, typically based on the last input received.
        int _playerIndex = PlayerManager.Instance.GetUniquePlayerIndex();

        Color _playerColor = GetUniquePlayerColor(_playerIndex);
        string _playerName = GetPlayerNameByColor(_playerIndex);


        // Create the new PlayerConfig with the unique color and control scheme.
        PlayerConfig newPlayerConfig = new PlayerConfig
        {
            playerIndex = _playerIndex,
            playerName = _playerName,
            playerColor = _playerColor,
            team = Team.Spectator,
            controlScheme = ControlScheme.Gamepad,
            inputDevice = playerInput.devices[0]
        };


        SetNameAndSendPlayerStats(_playerName, _playerIndex);

        // Add the new PlayerConfig to the playerConfigs list.
        PlayerManager.Instance.playerConfigs.Add(newPlayerConfig);

        return newPlayerConfig;

    }

    private void SetNameAndSendPlayerStats ( string _playerName, int _playerIndex )
    {
        PlayerStats _playerStats = CreateInstance<PlayerStats>();
        _playerStats.playerName = _playerName;
        PlayerStatsManager.Instance.UpdatePlayerStats(_playerStats, _playerIndex);
    }

    private Color GetUniquePlayerColor ( int playerIndex )
    {
        // Ensure that the player index is within the range of available colors.
        if (playerColors.Count > 0)
        {
            // Use modulo to loop back to the start of the color list if there are more players than colors.
            return playerColors[playerIndex % playerColors.Count];
        }
        else
            return Color.white;
    }

    private string GetPlayerNameByColor ( int playerIndex )
    {
        switch (playerIndex)
        {
            case 0:
                return "Red";
            case 1:
                return "Blue";
            case 2:
                return "Green";
            case 3:
                return "Yellow";
            default:
                return "Unknown";

        }
    }

}
