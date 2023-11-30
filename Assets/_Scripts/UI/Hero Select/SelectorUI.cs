using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static PlayerConfigData;


public class SelectorUI : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    public List<Transform> optionList;

    [SerializeField] private int selectedOptionIndex;

    [SerializeField] private PlayerConfigData playerConfigData;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInput playerInput;


    public bool flipTeamB;
    private bool isPlayerSelected;
    private bool secondaryButtonReleased = true;

    private HeroSelectController heroSelectController;
    private MapSelectController mapSelectController;

    [SerializeField] private PlayerConfig playerConfig;

    private Image frame;

    private float navigationCooldown = 0.2f;
    private float lastNavigationTime;


    private float selectionCooldown = 1.0f; // Cooldown time in seconds before allowing selection
    private float timeSinceInstantiation;

    private void Awake ()
    {
        heroSelectController = FindAnyObjectByType<HeroSelectController>();
        frame = GetComponentInChildren<Image>();
        GetOptionList();

    }

    void Start ()
    {

        if (heroSelectController != null)
            if (heroSelectController.isFreeForAllMode)
                FreeForAllSelector();

        selectedOptionIndex = 0;
        lastNavigationTime = -navigationCooldown;
        timeSinceInstantiation = 0;
    }

    private void FreeForAllSelector ()
    {
        transform.SetParent(heroSelectController.parentFFA, false);
        transform.SetSiblingIndex(0);
        transform.localScale = new Vector3(4, 4, 4);

        MoveSelectorToOption(0);

        playerConfig = playerConfigData.AddPlayerConfig(playerInput);
        ColorFrame(playerConfig);

        playerConfig.team = Team.FreeForAll;
        PlayerManager.Instance.SetTeam(playerConfig.playerIndex, Team.FreeForAll);
        PlayerManager.Instance.SetPlayerState(playerConfig.playerIndex, PlayerState.SelectingHero);

        heroSelectController.RegisterSelector(this);

    }

    void Update ()
    {
        timeSinceInstantiation += Time.deltaTime;

        if (!isPlayerSelected)
        {
            if (Time.time - lastNavigationTime > navigationCooldown)
            {
                HandleOptionsNavigation();
            }

            if (timeSinceInstantiation > selectionCooldown)
            {
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
    private int GetNavigationDirection ( float input, Team team )
    {
        bool isInputPositive = input > 0;
        bool shouldFlip = (team == Team.TeamB && flipTeamB);

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
        GetOptionList();

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

    /*    public void UpdateVisual ( int count, bool isClockwise )
        {
            frame = GetComponentInChildren<Image>();

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
        }*/

    public void UpdateVisual ( int count, bool isClockwise )
    {
        frame = GetComponentInChildren<Image>();

        // Set the fill method to Radial360
        frame.fillMethod = Image.FillMethod.Radial360;
        //frame.fillClockwise = isClockwise;

        // Calculate the fill amount for each segment
        frame.fillAmount = count > 1 ? 1.0f / count : 1.0f;

        // Rotate the image based on the player's index
        if (count > 1)
        {
            // Assuming playerIndex is a 0-based index
            int playerIndex = playerConfig.playerIndex;
            float rotationAngle = (360f / count) * playerIndex;

            // Apply the rotation
            frame.transform.rotation = Quaternion.Euler(0, 0, -rotationAngle);
        }
        else
        {
            // Reset rotation for a single player
            frame.transform.rotation = Quaternion.identity;
        }
    }






    public PlayerConfig GetSelectorConfig ()
    {
        return playerConfig;
    }
}
