using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static PlayerConfigData;

public class HeroSelectController : MonoBehaviour
{
    [SerializeField] private string nextScene;
    [SerializeField] private string previousScene = "TeamSelect";

    public bool isFreeForAllMode = false;

    [Header("Free For All Settings")]
    [SerializeField] private List<Transform> avatarsFFA;
    [SerializeField] private List<Image> heroImagesFFA;
    [SerializeField] private List<Image> readyIconsFFA;
    public Transform parentFFA;

    private Dictionary<SelectorUI, int> uiSelectorsFFA = new Dictionary<SelectorUI, int>();
    private Dictionary<SelectorUI, int> ffaPlayerIndices = new Dictionary<SelectorUI, int>();




    [Header("Prefab")]
    [SerializeField] private GameObject selectorPrefab;
    [SerializeField] private bool flipTeamB;


    // Avatar Disctionaries
    private Dictionary<SelectorUI, int> uiSelectorsTeamA = new Dictionary<SelectorUI, int>();
    private Dictionary<SelectorUI, int> uiSelectorsTeamB = new Dictionary<SelectorUI, int>();

    [Header("Team Deathmatch")]
    [Header("Selectors Spawn Points")]
    [SerializeField] private List<Transform> avatarsTeamA;
    [SerializeField] private List<Transform> avatarsTeamB;
    [SerializeField] private Transform teamAParent;
    [SerializeField] private Transform teamBParent;

    private int teamASpawnIndex = 0;
    private int teamBSpawnIndex = 0;

    private Dictionary<Team, int> teamPlayerCounts = new Dictionary<Team, int>
    {
        //{ Team.FreeForAll, 0 },
        { Team.TeamA, 0 },
        { Team.TeamB, 0 },
        { Team.Bot, 0 }
    };

    [Header("Hero Images")]
    [SerializeField] private List<Image> heroImagesA;
    [SerializeField] private List<Image> heroImagesB;

    [Header("Ready Icons")]
    [SerializeField] private List<Image> readyIconsA;
    [SerializeField] private List<Image> readyIconsB;

    // Team Dictionaries
    private Dictionary<SelectorUI, int> teamAPlayerIndices = new Dictionary<SelectorUI, int>();
    private Dictionary<SelectorUI, int> teamBPlayerIndices = new Dictionary<SelectorUI, int>();


    private CountdownUI countdownUI;

    private Dictionary<PlayerConfig, Image> playerConfigToHeroImageMap = new Dictionary<PlayerConfig, Image>();


    [Header("Bots")]
    [SerializeField] private List<Transform> avatarsBot;
    [SerializeField] private Image heroImageBot;
    [SerializeField] private Image readyIconBot;

    private Dictionary<SelectorUI, int> botsIndices = new Dictionary<SelectorUI, int>();



    private void Awake ()
    {
        countdownUI = GetComponentInChildren<CountdownUI>();
        countdownUI.OnCountdownFinished += NextScene;
    }

    private void Start ()
    {
        PlayerManager.Instance.SetHeroSelectController(this);
        PlayerManager.Instance.InitializeHeroSelectors();
    }

    private void OnDestroy ()
    {
        countdownUI.OnCountdownFinished -= NextScene;
    }

    public PlayerInput InstantiateSelector ( PlayerConfig config, int playerCount )
    {
        Transform spawnPoint = isFreeForAllMode ? GetFreeForAllSpawnPoint(playerCount) : GetSpawnPoint(config);

        teamPlayerCounts[config.team]++;

        if (spawnPoint == null)
        {
            if (config.controlScheme != ControlScheme.Bot)
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

        // Set the instantiated player's position and rotation
        instantiatedPlayer.transform.position = spawnPoint.position;
        instantiatedPlayer.transform.SetParent(GetParentGO(config), false);
        instantiatedPlayer.transform.SetSiblingIndex(0);
        instantiatedPlayer.transform.localScale = new Vector3(4, 4, 4);

        SetupHeroSelector(config, instantiatedPlayer);

        return instantiatedPlayer;
    }

    public void SetupHeroSelector ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        SelectorUI heroSelector = instantiatedPlayer.GetComponent<SelectorUI>();
        heroSelector.SetHeroSelectController(this);
        heroSelector.SetPlayerConfig(config);
        //heroSelector.GetOptionList();
        heroSelector.ColorFrame(config);
        heroSelector.flipTeamB = flipTeamB;

        RegisterSelector(heroSelector, config);
    }

    public void RegisterSelector ( SelectorUI selector, PlayerConfig config )
    {
        var team = selector.GetSelectorConfig().team;
        int nextAvailableIndex = FindNextAvailableAvatarIndex(team);
        var selectorMap = GetSelectorMap(team);
        selectorMap[selector] = nextAvailableIndex;

        Image heroImage = DetermineHeroImageForPlayer(config, selectorMap);
        playerConfigToHeroImageMap[config] = heroImage;

        // Assign team-specific index
        var playerIndices = GetTeamPlayerIndiceList(team);
        playerIndices[selector] = playerIndices.Count; // Assign the next index in the team

        selector.MoveSelectorToOption(nextAvailableIndex);
        UpdateHeroVisuals();
    }

    private Image DetermineHeroImageForPlayer ( PlayerConfig config, Dictionary<SelectorUI, int> selectorMap )
    {
        List<Image> heroImages = GetHeroImagesByTeam(config.team);

        // Check if it's Free For All mode
        if (config.team == Team.FreeForAll)
        {
            if (config.playerIndex >= 0 && config.playerIndex < heroImages.Count)
            {
                return heroImages[config.playerIndex];
            }
            else
            {
                Debug.LogError("Invalid player index for Free For All mode: " + config.playerIndex);
                return null; // or a default image if you have one
            }
        }
        else
        {
            int avatarIndex = -1;
            // Find the avatar index for the given player config
            foreach (var pair in selectorMap)
            {
                if (pair.Key.GetSelectorConfig().playerIndex == config.playerIndex)
                {
                    avatarIndex = pair.Value;
                    break;
                }
            }

            if (avatarIndex >= 0 && avatarIndex < heroImages.Count)
            {
                return heroImages[avatarIndex];
            }
            else
            {
                Debug.LogError("Invalid avatar index for player: " + config.playerIndex);
                return null; // or a default image if you have one
            }
        }
    }

    public void UpdateSelectorAvatar ( SelectorUI selector, int avatarIndex, PlayerConfig playerConfig )
    {
        var uiSelectors = GetSelectorMap(playerConfig.team);
        uiSelectors[selector] = avatarIndex;

        UpdateHeroVisuals();
        UpdateHeroImages(playerConfig);
    }

    private void UpdateHeroVisuals ()
    {
        // Reset visual for all selectors in both teams
        ResetSelectorFrameUI(uiSelectorsTeamA);
        ResetSelectorFrameUI(uiSelectorsTeamB);
        ResetSelectorFrameUI(uiSelectorsFFA);

        // Update visuals for each team
        UpdateSelectorUI(uiSelectorsTeamA, uiSelectorsTeamA.Count);
        UpdateSelectorUI(uiSelectorsTeamB, uiSelectorsTeamB.Count);
        UpdateSelectorUI(uiSelectorsFFA, uiSelectorsFFA.Count);
    }


    private void UpdateHeroImages ( PlayerConfig playerConfig )
    {
        var selectorMap = GetSelectorMap(playerConfig.team);
        List<Image> heroImages = GetHeroImagesByTeam(playerConfig.team);

        UpdateStatueColor(playerConfig);

        int teamPlayerIndex = 0;

        foreach (var pair in selectorMap)
        {
            SelectorUI selector = pair.Key;
            int avatarIndex = pair.Value;

            if (avatarIndex >= 0)
            {
                Image playerImage = heroImages[teamPlayerIndex % heroImages.Count];
                Animator animator = playerImage.GetComponent<Animator>();
                AspectRatioFitter aspectRatioFitter = playerImage.GetComponent<AspectRatioFitter>();

                // Temporarily disable Animator and AspectRatioFitter
                if (animator != null) animator.enabled = false;

                // Reset animator bools and set the correct animation state
                animator.SetBool("Archy", avatarIndex == 0);
                animator.SetBool("Turtle", avatarIndex == 1);
                animator.SetBool("Mage", avatarIndex == 2);

                // Force native size
                StartCoroutine(SetImageToNativeSizeNextFrame(playerImage));


                // Re-enable Animator and AspectRatioFitter
                if (animator != null) animator.enabled = true;

                playerImage.enabled = true;
                teamPlayerIndex++;
            }
        }
    }

    private void UpdateStatueColor ( PlayerConfig config )
    {
        Image playerImage;

        if (!playerConfigToHeroImageMap.TryGetValue(config, out playerImage))
            return;


        // Update the color of the parent of the Image
        if (playerImage != null && playerImage.transform.parent != null)
        {
            var parentImageComponent = playerImage.transform.parent.GetComponent<Image>();
            if (parentImageComponent != null)
            {
                parentImageComponent.color = config.playerColor;
            }
        }
    }

    private IEnumerator SetImageToNativeSizeNextFrame ( Image image )
    {
        // Wait until the end of the frame
        yield return new WaitForEndOfFrame();

        // Apply SetNativeSize
        image.SetNativeSize();
    }

    private void ResetSelectorFrameUI ( Dictionary<SelectorUI, int> selectorMap )
    {
        foreach (var selector in selectorMap.Keys)
        {
            selector.UpdateFrameUI(1, 0); // Default to a full frame
        }
    }

    private void UpdateSelectorUI ( Dictionary<SelectorUI, int> selectorMap, int totalPlayerCount )
    {
        var avatarSelectorsMap = new Dictionary<int, List<SelectorUI>>();

        // Populate the map
        foreach (var pair in selectorMap)
        {
            int avatarIndex = pair.Value;
            if (!avatarSelectorsMap.ContainsKey(avatarIndex))
            {
                avatarSelectorsMap[avatarIndex] = new List<SelectorUI>();
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
                    // Pass both the index of the selector and the total count of selectors on this avatar
                    selectorsOnAvatar[i].UpdateFrameUI(i, selectorsOnAvatar.Count);
                }
            }
        }
    }

    private int FindNextAvailableAvatarIndex ( Team team )
    {
        List<Transform> avatars = GetTeamAvatarList(team);
        var avatarSelectorCount = new Dictionary<int, int>();

        // Initialize counts for each avatar
        for (int i = 0; i < avatars.Count; i++)
        {
            avatarSelectorCount[i] = 0;
        }

        // Determine the correct selector map based on the team
        var selectorMap = team == Team.TeamA ? uiSelectorsTeamA : uiSelectorsTeamB;

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

    public void UpdateReadyIcon ( SelectorUI selector, bool isReady )
    {
        var team = selector.GetSelectorConfig().team;

        if (team == Team.Bot)
        {
            readyIconBot.enabled = isReady;
            return;
        }

        var playerIndices = GetTeamPlayerIndiceList(team);


        if (playerIndices.TryGetValue(selector, out int teamPlayerIndex))
        {
            List<Image> readyIcons = GetTeamReadyIconList(team);
            if (teamPlayerIndex >= 0 && teamPlayerIndex < readyIcons.Count)
            {
                readyIcons[teamPlayerIndex].enabled = isReady;
            }
        }

        if (!isReady)
        {
            StopCountdownTimer();
        }
    }

    public bool AreAllPlayersReady ()
    {
        List<PlayerConfig> playerConfigList = PlayerManager.Instance.GetPlayerConfigList();

        foreach (var playerConfig in playerConfigList)
        {
            if (playerConfig.playerState != PlayerState.Ready)
            {
                return false; // If any player is not ready
            }
        }
        return true; // All players are ready
    }

    public bool AreAllPlayersSelecting ()
    {
        List<PlayerConfig> playerConfigList = PlayerManager.Instance.GetPlayerConfigList();

        foreach (var playerConfig in playerConfigList)
        {
            if (playerConfig.playerState != PlayerState.SelectingHero)
            {
                return false; // If any player is not selcting hero
            }
        }
        return true; // All players are selcting hero
    }

    public void StartCountdownTimer ()
    {
        countdownUI.StartTimer();
    }

    public void StopCountdownTimer ()
    {
        countdownUI.StopTimer();
    }

    public void PreviousScene ()
    {
        PlayerManager.Instance.ResetPlayerConfigs();
        PlayerStatsManager.Instance.ClearAllStatsList();

        UnityEngine.SceneManagement.SceneManager.LoadScene(previousScene);
    }

    public void NextScene ()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }


    #region Getters

    private Dictionary<SelectorUI, int> GetSelectorMap ( Team team )
    {
        switch (team)
        {
            case Team.TeamA:
                return uiSelectorsTeamA;
            case Team.TeamB:
                return uiSelectorsTeamB;
            case Team.FreeForAll:
                return uiSelectorsFFA;
            default: return null;
        }
    }

    private Transform GetSpawnPoint ( PlayerConfig config )
    {
        Transform spawnPoint = null;

        // Determine the spawn point based on the team
        if (config.team == Team.TeamA)
        {
            if (teamASpawnIndex < avatarsTeamA.Count)
            {
                spawnPoint = avatarsTeamA[teamASpawnIndex];
            }
        }
        else if (config.team == Team.TeamB)
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
        return config.team == Team.TeamA ? teamAParent : teamBParent;
    }

    public List<Transform> GetTeamAvatarList ( Team team )
    {
        switch (team)
        {
            case Team.TeamA:
                return avatarsTeamA;

            case Team.TeamB:
                return avatarsTeamB;

            case Team.FreeForAll:
                return avatarsFFA;

            case Team.Bot:
                return avatarsBot;

            default: return null;

        }
    }

    public List<Image> GetTeamReadyIconList ( Team team )
    {
        switch (team)
        {
            case Team.TeamA:
                return readyIconsA;

            case Team.TeamB:
                return readyIconsB;

            case Team.FreeForAll:
                return readyIconsFFA;

            default: return null;

        }
    }

    public Dictionary<SelectorUI, int> GetTeamPlayerIndiceList ( Team team )
    {
        switch (team)
        {
            case Team.TeamA:
                return teamAPlayerIndices;

            case Team.TeamB:
                return teamBPlayerIndices;

            case Team.FreeForAll:
                return ffaPlayerIndices;

            default: return null;

        }
    }

    private List<Image> GetHeroImagesByTeam ( Team team )
    {
        switch (team)
        {
            case Team.TeamA:
                return heroImagesA;
            case Team.TeamB:
                return heroImagesB;
            case Team.FreeForAll:
                return heroImagesFFA;
            default:
                return new List<Image>();
        }
    }

    private Transform GetFreeForAllSpawnPoint ( int playerCount )
    {
        // Ensure that the playerCount is within the range of available spawn points
        if (avatarsFFA != null && avatarsFFA.Count > 0)
        {
            int spawnIndex = playerCount % avatarsFFA.Count;
            return avatarsFFA[spawnIndex];
        }
        else
        {
            Debug.LogError("No available spawn points for Free For All mode.");
            return null; // Return null if no spawn points are available
        }
    }



    #endregion

}
