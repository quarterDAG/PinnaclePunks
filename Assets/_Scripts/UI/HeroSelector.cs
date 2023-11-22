using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class HeroSelector : MonoBehaviour
{
    [SerializeField] private InputManager inputManager; // The InputManager instance for this player
    public List<Transform> heroAvatars; // Array of hero avatars

    [SerializeField] private int selectedHeroIndex; // Index of the currently selected hero
    private bool isPlayerReady;
    private bool secondaryButtonReleased = true;

    private HeroSelectManager heroSelectManager;

    private PlayerConfig playerConfig;

    private Image frame;

    private float navigationCooldown = 0.2f; // Time in seconds between avatar changes
    private float lastNavigationTime;

    void Start ()
    {
        selectedHeroIndex = 0;

        lastNavigationTime = -navigationCooldown;
    }

    void Update ()
    {
        if (!isPlayerReady)
        {
            if (Time.time - lastNavigationTime > navigationCooldown)
            {
                HandleHeroNavigation();
            }

            // Select hero and set to ready
            if (inputManager.IsJumpPressed)
            {
                JumpButtonPressed();
            }

            if (inputManager.IsSecondaryPressed && secondaryButtonReleased)
            {
                secondaryButtonReleased = false;
                if (heroSelectManager.AreAllPlayersSelecting())
                    heroSelectManager.PreviousScene();
            }
        }
        else
        {
            if (inputManager.IsSecondaryPressed && secondaryButtonReleased)
            {
                secondaryButtonReleased = false;
                isPlayerReady = false;
                heroSelectManager.UpdateReadyIcon(this, false);
                PlayerManager.Instance.SetPlayerState(playerConfig.playerIndex, PlayerConfigData.PlayerState.SelectingHero);

                // Additional logic for deselection can be added here
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
        isPlayerReady = true;
        inputManager.ResetJump(false, false); // Reset jump to avoid repeated selection

        heroSelectManager.UpdateReadyIcon(this, true);

        PlayerManager.Instance.SetPlayerSelectedHero(selectedHeroIndex, playerConfig);
    }

    private void HandleHeroNavigation ()
    {
        int oldIndex = selectedHeroIndex;
        Vector2 inputVelocity = inputManager.InputVelocity;
        bool hasNavigated = false;

        if (playerConfig.team == PlayerConfigData.Team.TeamA)
        {
            if (inputVelocity.x > 0)
            {
                selectedHeroIndex++;
                hasNavigated = true;
            }
            else if (inputVelocity.x < 0)
            {
                selectedHeroIndex--;
                hasNavigated = true;
            }

        }
        else
        {
            if (inputVelocity.x < 0)
            {
                selectedHeroIndex++;
                hasNavigated = true;
            }
            else if (inputVelocity.x > 0)
            {
                selectedHeroIndex--;
                hasNavigated = true;
            }
        }


        selectedHeroIndex = Mathf.Clamp(selectedHeroIndex, 0, heroAvatars.Count - 1);

        // Update selector position if index changed
        if (oldIndex != selectedHeroIndex)
        {
            MoveSelectorToHero(selectedHeroIndex);
        }

        // Reset navigation timer
        if (hasNavigated)
        {
            lastNavigationTime = Time.time;
        }
    }

    public void MoveSelectorToHero ( int heroIndex )
    {
        // Update the selected hero index first
        selectedHeroIndex = heroIndex;

        // Move the selector to the new hero avatar position
        transform.position = heroAvatars[heroIndex].transform.position;

        // Notify the HeroSelectManager about the new selected index
        heroSelectManager.UpdateSelectorAvatar(this, selectedHeroIndex, playerConfig.team);
    }

    public void GetHeroAvatarList ()
    {
        heroAvatars = heroSelectManager.GetTeamAvatarList(playerConfig.team);
    }



    public void SetPlayerConfig ( PlayerConfig config )
    {
        playerConfig = config;
    }

    public void SetHeroSelectManager ( HeroSelectManager manager )
    {
        heroSelectManager = manager;
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


    public PlayerConfig GetHeroSelectorConfig ()
    {
        return playerConfig;
    }
}
