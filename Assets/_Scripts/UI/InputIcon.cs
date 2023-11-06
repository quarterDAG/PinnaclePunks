using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputIcon : MonoBehaviour
{
    private TeamSelectionController teamSelectionController;
    private int playerIndex;
    private RectTransform rect;

    public enum PlayerState
    {
        TeamA,
        TeamB,
        Spectator
    }

    [SerializeField] private PlayerState playerState = PlayerState.Spectator;

    private InputManager inputManager;

    private void Awake ()
    {
        inputManager = GetComponent<InputManager>();
        rect = GetComponent<RectTransform>();
    }

    private void Start ()
    {
        teamSelectionController = GetComponentInParent<TeamSelectionController>();
    }

    public void Initialize ( TeamSelectionController controller, int index )
    {
        rect.SetParent(controller.transform, false);
        rect.localPosition = controller.playerIconPositions[index];
        teamSelectionController = controller;
        playerIndex = index;
    }



    void Update ()
    {
        HandleMovement();
    }

    private void HandleMovement ()
    {

        if (inputManager.InventoryInput.x == 0)
            return;

        Debug.Log(inputManager.InventoryInput);

        if (inputManager.InventoryInput.x > 0)
        {
            switch (playerState)
            {
                case PlayerState.TeamA:
                    if (teamSelectionController.TryMovePlayerToSpectator(playerIndex))
                    {
                        MoveToPosition(teamSelectionController.spectatorPositionX);
                        playerState = PlayerState.Spectator;
                    }
                    break;
                case PlayerState.Spectator:
                    if (teamSelectionController.TryMovePlayerToTeam(playerIndex, false))
                    {
                        MoveToPosition(teamSelectionController.teamBPositionX);
                        playerState = PlayerState.TeamB;
                    }
                    break;
            }
        }

        else if (inputManager.InventoryInput.x < 0)
        {
            switch (playerState)
            {
                case PlayerState.Spectator:
                    if (teamSelectionController.TryMovePlayerToTeam(playerIndex, true))
                    {
                        MoveToPosition(teamSelectionController.teamAPositionX);
                        playerState = PlayerState.TeamA;
                    }
                    break;
                case PlayerState.TeamB:
                    if (teamSelectionController.TryMovePlayerToSpectator(playerIndex))
                    {
                        MoveToPosition(teamSelectionController.spectatorPositionX);
                        playerState = PlayerState.Spectator;
                    }
                    break;
            }
        }
    }


    private void MoveToPosition ( float newPositionX )
    {
        Vector2 newAnchoredPosition = rect.anchoredPosition;
        newAnchoredPosition.x = newPositionX;
        rect.anchoredPosition = newAnchoredPosition;
    }

}

