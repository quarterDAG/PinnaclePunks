using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public PlayerController[] players; // Array to hold all player references

    // This method can be called when a player dies
    public void PlayerDied ( PlayerController player )
    {
        player.Die();
        // You can also add additional game-wide logic here if needed
    }

}
