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
    [SerializeField] private PlayerConfig playerConfig;

    private Image frame;

    private float navigationCooldown = 0.2f;
    private float lastNavigationTime;


    private float selectionCooldown = 1.0f; // Cooldown time in seconds before allowing selection
    private float timeSinceInstantiation;

    public enum SelectorType
    {
        PlayerHeroSelector,
        BotHeroSelector,
        TeamSelector
    }

    public SelectorType selectorType;


    private void Awake ()
    {
        heroSelectController = FindAnyObjectByType<HeroSelectController>();
        frame = GetComponentInChildren<Image>();
    }

    void Start ()
    {
        if (selectorType == SelectorType.PlayerHeroSelector)
        {
            if (heroSelectController.isFreeForAllMode)
                FreeForAllSelector();
            GetHeroList();
        }

        if (selectorType == SelectorType.BotHeroSelector)
        {
            playerConfig = playerConfigData.AddPlayerConfig(null);
        }



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

        heroSelectController.RegisterSelector(this, playerConfig);

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
                heroSelectController?.UpdateReadyIcon(this, false);
                PlayerManager.Instance.SetPlayerState(playerConfig.playerIndex, PlayerState.SelectingHero);
            }
        }

        // Reset the flag if the secondary button is not being pressed
        if (!inputManager.IsSecondaryPressed)
        {
            secondaryButtonReleased = true;
        }

        if (inputManager.IsSpawnMinionPressed)
        {
            heroSelectController.ShowBotMenu(true);

            // Control the Bot Hero Selector
            heroSelectController.botHeroSelector.playerInput = playerInput;
            heroSelectController.botHeroSelector.inputManager = inputManager;


            if (selectorType == SelectorType.PlayerHeroSelector)
                this.enabled = false;

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

        if (selectorType == SelectorType.BotHeroSelector)
        {
            // Move to Bot Team Selector
            heroSelectController.botTeamSelector.gameObject.SetActive(true);
            heroSelectController.botTeamSelector.playerInput = playerInput;
            heroSelectController.botTeamSelector.inputManager = inputManager;
            heroSelectController.botTeamSelector.playerConfig = playerConfig;
            this.enabled = false;
        }
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
        if (optionList.Count == 0)
            GetHeroList();

        // Update the selected hero index first
        selectedOptionIndex = optionIndex;

        // Move the selector to the new hero avatar position
        transform.position = optionList[optionIndex].transform.position;

        // Notify the HeroSelectManager about the new selected index
        if (selectorType != SelectorType.TeamSelector)
            UpdateHeroSelectController();
    }

    private void UpdateHeroSelectController ()
    {
        if (heroSelectController != null)
            heroSelectController.UpdateSelectorAvatar(this, selectedOptionIndex, playerConfig);
    }

    public void GetHeroList ()
    {
        optionList = heroSelectController?.GetTeamAvatarList(playerConfig.team);
    }

    public void SetPlayerConfig ( PlayerConfig config )
    {
        playerConfig = config;
    }

    public void SetHeroSelectController ( HeroSelectController controller )
    {
        heroSelectController = controller;
    }

    public void ColorFrame ( PlayerConfig config )
    {
        frame = GetComponentInChildren<Image>();
        frame.color = config.playerColor;
    }

    public void UpdateFrameUI ( int playerIndex, int playersOnAvatar )
    {
        frame = GetComponentInChildren<Image>();

        // Set the fill method to Radial360
        frame.fillMethod = Image.FillMethod.Radial360;

        // Calculate the fill amount for each segment
        frame.fillAmount = playersOnAvatar > 1 ? 1.0f / playersOnAvatar : 1.0f;

        // Calculate and apply the rotation for each player's frame
        float rotationAngle = CalculateRotationAngle(playerIndex, playersOnAvatar);
        frame.rectTransform.localEulerAngles = new Vector3(0, 0, rotationAngle);
    }


    private float CalculateRotationAngle ( int playerIndex, int playersOnAvatar )
    {
        // Calculate the angle of rotation for each player
        // Use playersOnAvatar for the calculation as it represents the number of selectors on the avatar
        if (playersOnAvatar > 1)
        {
            return 360f / playersOnAvatar * playerIndex;
        }
        else
        {
            return 0f; // No rotation needed for a single player
        }
    }


    public PlayerConfig GetSelectorConfig ()
    {
        return playerConfig;
    }
}
