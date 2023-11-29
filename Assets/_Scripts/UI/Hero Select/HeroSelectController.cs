using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HeroSelectController : MonoBehaviour
{
    [SerializeField] private string nextScene;
    [SerializeField] private string previousScene = "TeamSelect";

    public bool isFreeForAllMode = false;

    [Header("Free For All Settings")]
    public List<Transform> avatarsFreeForAll;
    public List<Image> heroImagesFreeForAll;
    public List<Image> readyIconsFreeForAll;
    public Transform parentFreeForAll;

    private Dictionary<SelectorUI, int> uiSelectorsFreeForAll = new Dictionary<SelectorUI, int>();


    [Header("Selectors Spawn Points")]
    public List<Transform> avatarsTeamA;
    public List<Transform> avatarsTeamB;
    private int teamASpawnIndex = 0;
    private int teamBSpawnIndex = 0;

    private Dictionary<PlayerConfigData.Team, int> teamPlayerCounts = new Dictionary<PlayerConfigData.Team, int>
    {
        { PlayerConfigData.Team.TeamA, 0 },
        { PlayerConfigData.Team.TeamB, 0 }
    };

    public Transform teamAParent;
    public Transform teamBParent;


    [Header("Prefab")]
    public GameObject selectorPrefab;
    [SerializeField] private bool flipTeamB;


    // Avatar Disctionaries
    private Dictionary<SelectorUI, int> uiSelectorsTeamA = new Dictionary<SelectorUI, int>();
    private Dictionary<SelectorUI, int> uiSelectorsTeamB = new Dictionary<SelectorUI, int>();

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

        if (isFreeForAllMode)
        {
            // Additional logic for Free for All mode
        }

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

        RegisterSelector(heroSelector);
    }

    public void RegisterSelector ( SelectorUI selector )
    {
        var team = selector.GetSelectorConfig().team;
        int nextAvailableIndex = FindNextAvailableAvatarIndex(team);
        GetSelectorMap(team)[selector] = nextAvailableIndex;

        // Assign team-specific index
        var playerIndices = team == PlayerConfigData.Team.TeamA ? teamAPlayerIndices : teamBPlayerIndices;
        playerIndices[selector] = playerIndices.Count; // Assign the next index in the team

        selector.MoveSelectorToOption(nextAvailableIndex);
        UpdateHeroVisuals();
    }


    public void UpdateSelectorAvatar ( SelectorUI selector, int avatarIndex, PlayerConfigData.Team team )
    {
        if (team == PlayerConfigData.Team.TeamA)
            uiSelectorsTeamA[selector] = avatarIndex;
        else if (team == PlayerConfigData.Team.TeamB)
            uiSelectorsTeamB[selector] = avatarIndex;

        UpdateHeroVisuals();
        UpdateHeroImages(team);
    }

    private void UpdateHeroVisuals ()
    {
        // Reset visual for all selectors in both teams
        ResetVisuals(uiSelectorsTeamA);
        ResetVisuals(uiSelectorsTeamB);

        // Update visuals for each team
        UpdateTeamVisuals(uiSelectorsTeamA);
        UpdateTeamVisuals(uiSelectorsTeamB);
    }

    private void UpdateHeroImages ( PlayerConfigData.Team team )
    {
        var selectorMap = GetSelectorMap(team);
        List<Image> heroImages = team == PlayerConfigData.Team.TeamA ? heroImagesA : heroImagesB;

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
    private IEnumerator SetImageToNativeSizeNextFrame ( Image image )
    {
        // Wait until the end of the frame
        yield return new WaitForEndOfFrame();

        // Apply SetNativeSize
        image.SetNativeSize();
    }

    private void ResetVisuals ( Dictionary<SelectorUI, int> selectorMap )
    {
        foreach (var selector in selectorMap.Keys)
        {
            selector.UpdateVisual(1, true); // Default to a full frame
        }
    }

    private void UpdateTeamVisuals ( Dictionary<SelectorUI, int> selectorMap )
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
        var selectorMap = team == PlayerConfigData.Team.TeamA ? uiSelectorsTeamA : uiSelectorsTeamB;

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
        var playerIndices = team == PlayerConfigData.Team.TeamA ? teamAPlayerIndices : teamBPlayerIndices;

        if (playerIndices.TryGetValue(selector, out int teamPlayerIndex))
        {
            List<Image> readyIcons = team == PlayerConfigData.Team.TeamA ? readyIconsA : readyIconsB;
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
            if (playerConfig.playerState != PlayerConfigData.PlayerState.Ready)
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
            if (playerConfig.playerState != PlayerConfigData.PlayerState.SelectingHero)
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

    private Dictionary<SelectorUI, int> GetSelectorMap ( PlayerConfigData.Team team )
    {
        return team == PlayerConfigData.Team.TeamA ? uiSelectorsTeamA : uiSelectorsTeamB;
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
        switch (team)
        {
            case PlayerConfigData.Team.TeamA:
                return avatarsTeamA;

            case PlayerConfigData.Team.TeamB:
                return avatarsTeamB;

            case PlayerConfigData.Team.FreeForAll:
                return avatarsFreeForAll;

            default: return null;

        }

    }
    private Transform GetFreeForAllSpawnPoint ( int playerCount )
    {
        // Ensure that the playerCount is within the range of available spawn points
        if (avatarsFreeForAll != null && avatarsFreeForAll.Count > 0)
        {
            int spawnIndex = playerCount % avatarsFreeForAll.Count;
            return avatarsFreeForAll[spawnIndex];
        }
        else
        {
            Debug.LogError("No available spawn points for Free For All mode.");
            return null; // Return null if no spawn points are available
        }
    }



    #endregion

}
