using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class SelectorUI : MonoBehaviour
{
    [SerializeField] private InputManager inputManager; 
    public List<Transform> optionList; 

    [SerializeField] private int selectedOptionIndex;
    public bool flipTeamB;
    private bool isPlayerSelected;
    private bool secondaryButtonReleased = true;

    private HeroSelectController heroSelectController;
    private MapSelectController mapSelectController;

    private PlayerConfig playerConfig;

    private Image frame;

    private float navigationCooldown = 0.2f; 
    private float lastNavigationTime;

    void Start ()
    {
        selectedOptionIndex = 0;
        lastNavigationTime = -navigationCooldown;
    }

    void Update ()
    {
        if (!isPlayerSelected)
        {
            if (Time.time - lastNavigationTime > navigationCooldown)
            {
                HandleOptionsNavigation();
            }

            // Select hero and set to ready
            if (inputManager.IsJumpPressed)
            {
                JumpButtonPressed();
            }

            if (inputManager.IsSecondaryPressed && secondaryButtonReleased)
            {
                secondaryButtonReleased = false;
                if (heroSelectController.AreAllPlayersSelecting())
                    heroSelectController.PreviousScene();
            }
        }
        else
        {
            if (inputManager.IsSecondaryPressed && secondaryButtonReleased)
            {
                secondaryButtonReleased = false;
                isPlayerSelected = false;
                heroSelectController.UpdateReadyIcon(this, false);
                PlayerManager.Instance.SetPlayerState(playerConfig.playerIndex, PlayerConfigData.PlayerState.SelectingHero);
            }
        }

        // Reset the flag if the secondary button is not being pressed
        if (!inputManager.IsSecondaryPressed)
        {
            secondaryButtonReleased = true;
        }
    }

    private void JumpButtonPressed ()
    {
        isPlayerSelected = true;
        inputManager.ResetJump(false, false); // Reset jump to avoid repeated selection

        if (heroSelectController != null)
            SelectHero();
    }

    private void SelectHero ()
    {
        heroSelectController.UpdateReadyIcon(this, true);
        PlayerManager.Instance.SetPlayerSelectedHero(selectedOptionIndex, playerConfig);
    }

    private void HandleOptionsNavigation ()
    {
        int oldIndex = selectedOptionIndex;
        Vector2 inputVelocity = inputManager.InputVelocity;

        // Determine navigation direction based on input and team configuration
        int navigationDirection = GetNavigationDirection(inputVelocity.x, playerConfig.team);

        // Navigate if there is input
        if (navigationDirection != 0)
        {
            selectedOptionIndex += navigationDirection;
            selectedOptionIndex = Mathf.Clamp(selectedOptionIndex, 0, optionList.Count - 1);

            // Update selector position if index changed
            if (oldIndex != selectedOptionIndex)
            {
                MoveSelectorToOption(selectedOptionIndex);
                lastNavigationTime = Time.time;
            }
        }
    }

    // Helper method to determine navigation direction
    private int GetNavigationDirection ( float input, PlayerConfigData.Team team )
    {
        bool isInputPositive = input > 0;
        bool shouldFlip = (team == PlayerConfigData.Team.TeamB && flipTeamB);

        if (isInputPositive)
        {
            return shouldFlip ? -1 : 1;
        }
        else if (input < 0)
        {
            return shouldFlip ? 1 : -1;
        }

        return 0; // No input
    }

    public void MoveSelectorToOption ( int optionIndex )
    {
        // Update the selected hero index first
        selectedOptionIndex = optionIndex;

        // Move the selector to the new hero avatar position
        transform.position = optionList[optionIndex].transform.position;

        // Notify the HeroSelectManager about the new selected index
        UpdateController();
    }

    private void UpdateController ()
    {
        if (heroSelectController != null)
            heroSelectController.UpdateSelectorAvatar(this, selectedOptionIndex, playerConfig.team);
    }

    public void GetOptionList ()
    {
        if (heroSelectController != null)
            optionList = heroSelectController.GetTeamAvatarList(playerConfig.team);

        if (mapSelectController != null)
            optionList = mapSelectController.mapUIList;
    }

    public void SetPlayerConfig ( PlayerConfig config )
    {
        playerConfig = config;
    }

    public void SetHeroSelectController ( HeroSelectController controller )
    {
        heroSelectController = controller;
    }

    public void SetMapSelectController ( MapSelectController controller )
    {
        mapSelectController = controller;
    }

    public void ColorFrame ( PlayerConfig config )
    {
        frame = GetComponentInChildren<Image>();
        frame.color = config.playerColor;
    }

    public void UpdateVisual ( int count, bool isClockwise )
    {
        // Adjust the visual based on the count and the fill direction
        if (count >= 2)
        {
            frame.fillMethod = Image.FillMethod.Radial360;
            frame.fillAmount = 0.5f;
            frame.fillClockwise = isClockwise;
        }
        else
        {
            frame.fillAmount = 1.0f;
        }
    }

    public PlayerConfig GetSelectorConfig ()
    {
        return playerConfig;
    }
}
