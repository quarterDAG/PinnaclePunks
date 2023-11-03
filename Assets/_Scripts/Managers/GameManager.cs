// A manager script that would go on a game manager object
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    private List<Gamepad> assignedGamepads = new List<Gamepad>();

    private void Start ()
    {
        foreach (var gamepad in Gamepad.all)
        {
            if (!assignedGamepads.Contains(gamepad))
            {
                CreatePlayer(gamepad);
            }
        }
    }

    void CreatePlayer ( Gamepad gamepad )
    {
        GameObject playerObject = Instantiate(playerPrefab);
        PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();

        // This assigns the gamepad to the player
        playerInput.SwitchCurrentControlScheme(gamepad);

        assignedGamepads.Add(gamepad);
    }
}
