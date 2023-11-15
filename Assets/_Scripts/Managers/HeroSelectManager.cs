using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HeroSelectManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public List<Transform> avatarsTeamA;
    public List<Transform> avatarsTeamB;
    private int teamASpawnIndex = 0;
    private int teamBSpawnIndex = 0;

    public Transform teamAParent;
    public Transform teamBParent;


    [Header("Prefab & Parent Settings")]
    public GameObject selectorPrefab;

    private Dictionary<HeroSelector, int> selectorAvatarMapTeamA = new Dictionary<HeroSelector, int>();
    private Dictionary<HeroSelector, int> selectorAvatarMapTeamB = new Dictionary<HeroSelector, int>();

    [Header("Hero Avatars")]
    [SerializeField] private List<Image> heroImagesA;
    [SerializeField] private List<Image> heroImagesB;

    [Header("Hero Sprites")]
    [SerializeField] private List<Sprite> avatarSprites;


    private void Start ()
    {
        PlayerManager.Instance.SetHeroSelectManager(this);
        PlayerManager.Instance.InitializeSelectors();
    }



    public PlayerInput InstantiateSelector ( PlayerConfig config, int playerCount )
    {
        Transform spawnPoint = GetSpawnPoint(config);

        if (config.team == PlayerConfigData.Team.TeamA)
        {
            teamASpawnIndex++;
        }
        else if (config.team == PlayerConfigData.Team.TeamB)
        {
            teamBSpawnIndex++;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("No available spawn points for team: " + config.team);
            return null; // Return null if no spawn point is available
        }


        // Instantiate player at the spawn point
        PlayerInput instantiatedPlayer = PlayerInput.Instantiate(
            selectorPrefab,
            controlScheme: config.controlScheme.ToString(),
            pairWithDevice: config.inputDevice,
            playerIndex: playerCount
        );

        //PlayerManager.Instance.SetPlayerState(config.playerIndex, PlayerConfigData.PlayerState.SelectingHero);

        // Set the instantiated player's position and rotation
        instantiatedPlayer.transform.position = spawnPoint.position;
        instantiatedPlayer.transform.SetParent(GetParentGO(config), false);
        instantiatedPlayer.transform.localScale = new Vector3(4, 4, 4);

        SetupHeroSelector(config, instantiatedPlayer);

        return instantiatedPlayer;
    }

    private void SetupHeroSelector ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        HeroSelector heroSelector = instantiatedPlayer.GetComponent<HeroSelector>();
        heroSelector.SetHeroSelectManager(this);
        heroSelector.SetPlayerConfig(config);
        heroSelector.GetHeroAvatarList();
        heroSelector.ColorFrame(config);

        RegisterSelector(heroSelector);
    }

    public void RegisterSelector ( HeroSelector selector )
    {
        int nextAvailableIndex = FindNextAvailableAvatarIndex(selector.GetHeroSelectorConfig().team);
        GetSelectorMap(selector.GetHeroSelectorConfig().team)[selector] = nextAvailableIndex;
        selector.MoveSelectorToHero(nextAvailableIndex);
        UpdateHeroVisuals();
    }

    public void UpdateSelectorAvatar ( HeroSelector selector, int avatarIndex, PlayerConfigData.Team team )
    {
        if (team == PlayerConfigData.Team.TeamA)
            selectorAvatarMapTeamA[selector] = avatarIndex;
        else if (team == PlayerConfigData.Team.TeamB)
            selectorAvatarMapTeamB[selector] = avatarIndex;

        UpdateHeroVisuals();
        UpdateHeroImages(team);
    }

    private void UpdateHeroVisuals ()
    {
        // Reset visual for all selectors in both teams
        ResetVisuals(selectorAvatarMapTeamA);
        ResetVisuals(selectorAvatarMapTeamB);

        // Update visuals for each team
        UpdateTeamVisuals(selectorAvatarMapTeamA);
        UpdateTeamVisuals(selectorAvatarMapTeamB);
    }

    private void UpdateHeroImages ( PlayerConfigData.Team team )
    {
        var selectorMap = GetSelectorMap(team);
        List<Image> heroImages = team == PlayerConfigData.Team.TeamA ? heroImagesA : heroImagesB;

        // Assume all images to be reset to default or disabled
        foreach (var image in heroImages)
        {
            image.enabled = false; // Hide the image if no avatar is selected
        }

        // Update images based on the current selection
        foreach (var pair in selectorMap)
        {
            HeroSelector selector = pair.Key;
            int avatarIndex = pair.Value;

            if (avatarIndex >= 0 && avatarIndex < avatarSprites.Count)
            {
                Sprite selectedSprite = avatarSprites[avatarIndex];
                Image playerImage = heroImages[selector.GetHeroSelectorConfig().playerIndex]; // Assuming each player has a fixed image slot

                playerImage.enabled = true;
                playerImage.sprite = selectedSprite;
            }
        }
    }


    private void ResetVisuals ( Dictionary<HeroSelector, int> selectorMap )
    {
        foreach (var selector in selectorMap.Keys)
        {
            selector.UpdateVisual(1, true); // Default to a full frame
        }
    }

    private void UpdateTeamVisuals ( Dictionary<HeroSelector, int> selectorMap )
    {
        var avatarSelectorsMap = new Dictionary<int, List<HeroSelector>>();

        // Populate the map
        foreach (var pair in selectorMap)
        {
            int avatarIndex = pair.Value;
            if (!avatarSelectorsMap.ContainsKey(avatarIndex))
            {
                avatarSelectorsMap[avatarIndex] = new List<HeroSelector>();
            }
            avatarSelectorsMap[avatarIndex].Add(pair.Key);
        }

        // Update each selector's visual
        foreach (var avatarPair in avatarSelectorsMap)
        {
            var selectorsOnAvatar = avatarPair.Value;
            if (selectorsOnAvatar.Count > 1)
            {
                for (int i = 0; i < selectorsOnAvatar.Count; i++)
                {
                    selectorsOnAvatar[i].UpdateVisual(selectorsOnAvatar.Count, i == 0);
                }
            }
        }
    }

    private int FindNextAvailableAvatarIndex ( PlayerConfigData.Team team )
    {
        List<Transform> avatars = GetTeamAvatarList(team);
        var avatarSelectorCount = new Dictionary<int, int>();

        // Initialize counts for each avatar
        for (int i = 0; i < avatars.Count; i++)
        {
            avatarSelectorCount[i] = 0;
        }

        // Determine the correct selector map based on the team
        var selectorMap = team == PlayerConfigData.Team.TeamA ? selectorAvatarMapTeamA : selectorAvatarMapTeamB;

        // Count the number of selectors per avatar
        foreach (var pair in selectorMap)
        {
            int avatarIndex = pair.Value;
            if (avatarIndex >= 0 && avatarIndex < avatars.Count)
            {
                avatarSelectorCount[avatarIndex]++;
            }
        }

        // Find the next available avatar index
        for (int i = 0; i < avatars.Count; i++)
        {
            if (avatarSelectorCount[i] < 1)
            {
                // Threshold for availability
                return i;
            }
        }

        return -1; // Return -1 if no avatars are available
    }

    public bool AreAllPlayersReady ()
    {
        List<PlayerConfig> playerConfigList = PlayerManager.Instance.GetPlayerConfigList();

        foreach (var playerConfig in playerConfigList)
        {
            if (playerConfig.playerState != PlayerConfigData.PlayerState.Ready)
            {
                return false; // If any player is not ready
            }
        }
        return true; // All players are ready
    }

    public void StartGame ()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }


    #region Getters

    private Dictionary<HeroSelector, int> GetSelectorMap ( PlayerConfigData.Team team )
    {
        return team == PlayerConfigData.Team.TeamA ? selectorAvatarMapTeamA : selectorAvatarMapTeamB;
    }

    private Transform GetSpawnPoint ( PlayerConfig config )
    {
        Transform spawnPoint = null;

        // Determine the spawn point based on the team
        if (config.team == PlayerConfigData.Team.TeamA)
        {
            if (teamASpawnIndex < avatarsTeamA.Count)
            {
                spawnPoint = avatarsTeamA[teamASpawnIndex];
            }
        }
        else if (config.team == PlayerConfigData.Team.TeamB)
        {
            if (teamBSpawnIndex < avatarsTeamB.Count)
            {
                spawnPoint = avatarsTeamB[teamBSpawnIndex];
            }
        }

        return spawnPoint;
    }

    private Transform GetParentGO ( PlayerConfig config )
    {
        return config.team == PlayerConfigData.Team.TeamA ? teamAParent : teamBParent;
    }

    public List<Transform> GetTeamAvatarList ( PlayerConfigData.Team team )
    {
        return team == PlayerConfigData.Team.TeamA ? avatarsTeamA : avatarsTeamB;
    }

    #endregion
}
