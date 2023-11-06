using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeamSelectionController : MonoBehaviour
{
    public Vector3[] playerIconPositions; // Ensure this is a 3D vector to match Transform positions

    public float teamAPositionX;
    public float teamBPositionX;
    public float spectatorPositionX = 0f;

    private const int MaxPlayersPerTeam = 2;
    private int teamACount = 0;
    private int teamBCount = 0;

    private PlayerInputManager playerInputManager;

    public enum PlayerState
    {
        TeamA,
        TeamB,
        Spectator
    }

    private Dictionary<int, PlayerState> playerTeamAssignments; // true for team A, false for team B


    private void Awake ()
    {
        playerTeamAssignments = new Dictionary<int, PlayerState>();
        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable ()
    {
        playerInputManager.onPlayerJoined += HandlePlayerJoined;
        playerInputManager.onPlayerLeft += HandlePlayerLeft;
    }

    private void OnDisable ()
    {
        playerInputManager.onPlayerJoined -= HandlePlayerJoined;
        playerInputManager.onPlayerLeft -= HandlePlayerLeft;
    }



    private async void HandlePlayerJoined ( PlayerInput playerInput )
    {
        await Task.Delay(500);
        // Make sure the playerInput is not null and has an InputIcon component.
        if (playerInput != null && playerInput.GetComponent<InputIcon>() != null)
        {
            InputIcon inputIcon = playerInput.GetComponent<InputIcon>();
            inputIcon.Initialize(this, playerInput.playerIndex);

            // Other initialization code...
        }
        else
        {
            Debug.LogError("PlayerInput or InputIcon component is missing on the player GameObject.");
        }
    }


    public bool TryMovePlayerToTeam ( int playerIndex, bool moveToTeamA )
    {
        // First, check if the playerIndex exists in the dictionary.
        // If it does not, add it with a default state (e.g., Spectator).
        if (!playerTeamAssignments.ContainsKey(playerIndex))
        {
            playerTeamAssignments[playerIndex] = PlayerState.Spectator;
        }

        // Check if the player is in the state we want to move them to.
        // If they are, return false since no move is needed.
        PlayerState currentPlayerState = playerTeamAssignments[playerIndex];
        if ((moveToTeamA && currentPlayerState == PlayerState.TeamA) ||
            (!moveToTeamA && currentPlayerState == PlayerState.TeamB))
        {
            return false;
        }

        // Move the player to Team A if there's space
        if (moveToTeamA && teamACount < MaxPlayersPerTeam)
        {
            if (currentPlayerState == PlayerState.TeamB)
            {
                teamBCount--;
            }

            playerTeamAssignments[playerIndex] = PlayerState.TeamA;
            teamACount++;
            return true;
        }
        // Move the player to Team B if there's space
        else if (!moveToTeamA && teamBCount < MaxPlayersPerTeam)
        {
            if (currentPlayerState == PlayerState.TeamA)
            {
                teamACount--;
            }

            playerTeamAssignments[playerIndex] = PlayerState.TeamB;
            teamBCount++;
            return true;
        }

        // If there's no space on the desired team, or the player is a spectator, don't move them
        return false;
    }



    public bool TryMovePlayerToSpectator ( int playerIndex )
    {
        if (!playerTeamAssignments.ContainsKey(playerIndex))
            playerTeamAssignments[playerIndex] = PlayerState.Spectator;

        if (playerTeamAssignments[playerIndex] == PlayerState.Spectator)
            return false; // Player is already a spectator, so do nothing.

        if (playerTeamAssignments[playerIndex] == PlayerState.TeamA)
            teamACount--;

        else if (playerTeamAssignments[playerIndex] == PlayerState.TeamB)
            teamBCount--;

        playerTeamAssignments[playerIndex] = PlayerState.Spectator;

        return true;
    }


    private void HandlePlayerLeft ( PlayerInput playerInput )
    {
        Debug.Log("Player Left: " +  playerInput);
        if (playerInput != null)
        {
            int playerIndex = playerInput.playerIndex;

            if (playerTeamAssignments.TryGetValue(playerIndex, out PlayerState state))
            {
                if (state == PlayerState.TeamA)
                    teamACount = Mathf.Max(0, teamACount - 1);
                else if (state == PlayerState.TeamB)
                    teamBCount = Mathf.Max(0, teamBCount - 1);

                playerTeamAssignments.Remove(playerIndex);
            }

            InputIcon inputIcon = playerInput.GetComponent<InputIcon>();
            if (inputIcon != null)
                Destroy(inputIcon.gameObject);

        }
    }




}
