using System.Collections.Generic;
using UnityEngine;
using static PlayerConfigData;
using static PlayerManager;




[System.Serializable]
public struct PlayerConfig
{
    public int playerIndex;
    public Color playerColor;
    public Team team;
    public ControlScheme controlScheme;
}

[CreateAssetMenu(fileName = "PlayerConfigData", menuName = "Game/Player Config Data", order = 1)]

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
        TeamA,
        TeamB
    }

    public List<Color> playerColors = new List<Color>();


    // You can add methods to manage player configs if necessary
    public void AddPlayerConfig ( PlayerConfig config )
    {
        playerConfigs.Add(config);
    }

    public void RemovePlayerConfig ( PlayerConfig config )
    {
        playerConfigs.Remove(config);
    }

}