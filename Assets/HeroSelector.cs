using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class HeroSelector : MonoBehaviour
{
    [SerializeField] private InputManager inputManager; // The InputManager instance for this player
    public List<Transform> heroAvatars; // Array of hero avatars

    [SerializeField] private int selectedHeroIndex; // Index of the currently selected hero
    private bool isPlayerReady; // Flag to check if the player is ready

    private HeroSelectManager heroSelectManager;

    private PlayerConfig playerConfig;

    private Image frame;

    void Start ()
    {
        selectedHeroIndex = 0; // Default to the first hero
        isPlayerReady = false;
    }


    void Update ()
    {
        if (!isPlayerReady)
        {
            // Navigate through heroes
            HandleHeroNavigation();

            // Select hero and set to ready
            if (inputManager.IsJumpPressed)
            {
                SetHeroToStage(selectedHeroIndex);
                isPlayerReady = true;
                inputManager.ResetJump(false, false); // Reset jump to avoid repeated selection
            }
        }
        else
        {
            // Cancel ready state
            if (inputManager.IsSecondaryPressed)
            {
                isPlayerReady = false;
                // Additional logic for deselection can be added here
            }
        }
    }

    private void HandleHeroNavigation ()
    {
        int oldIndex = selectedHeroIndex;
        Vector2 inputVelocity = inputManager.InputVelocity;
        if (inputVelocity.x > 0)
        {
            selectedHeroIndex++;
        }
        else if (inputVelocity.x < 0)
        {
            selectedHeroIndex--;
        }
        selectedHeroIndex = Mathf.Clamp(selectedHeroIndex, 0, heroAvatars.Count - 1);

        // Update selector position if index changed
        if (oldIndex != selectedHeroIndex)
        {
            MoveSelectorToHero(selectedHeroIndex);
        }
    }

    public void MoveSelectorToHero ( int heroIndex )
    {
        // Move the selector to the new hero avatar position
        transform.position = heroAvatars[heroIndex].transform.position;
    }
    public void GetHeroAvatarList ()
    {
        heroAvatars = heroSelectManager.GetTeamAvatarList(playerConfig.team);
    }

    private void SetHeroToStage ( int heroIndex )
    {
        // Logic to display the selected hero on the Hero Stage
        // This could involve animating the hero into the stage, displaying a "Ready" indicator, etc.
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
}
