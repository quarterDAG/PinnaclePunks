using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using static PlayerConfigData;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    [SerializeField] PlayerConfigData playerConfigData;

    private HeroSelectManager heroSelectManager;
    private PlayerSpawner playerSpawner;
    private CameraManager cameraManager;

    [Header("Players Settings")]
    public List<PlayerConfig> playerConfigs;

    public int playerCount { get; private set; }


    private void Awake ()
    {
        Singleton();
        playerConfigs = new List<PlayerConfig>();
    }

    private void Singleton ()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void InitializeSelectors ()
    {
        playerCount = 0;

        foreach (var config in playerConfigs)
        {

            if (config.team == Team.Spectator)
            {
                playerConfigData.RemovePlayerConfig(config);
                return;
            }
            else
            {
                var instantiatedSelector = heroSelectManager.InstantiateSelector(config, playerCount);
                if (instantiatedSelector != null)
                {
                    // Set up the player components that are specific to PlayerManager's responsibilities
                    //SetupPlayer(config, instantiatedPlayer);

                    instantiatedSelector.GetComponent<InputManager>().UpdateCurrentControlScheme(config.controlScheme.ToString());


                    // Create a reference to the config in the player stats (mainly to access the team when calculating the score)
                    PlayerStatsManager.Instance.allPlayerStats[config.playerIndex].SetPlayerConfig(config);
                    playerCount++;
                }
            }
        }
    }

    public void InitializePlayers ()
    {
        playerCount = 0;

        foreach (var config in playerConfigs)
        {

            if (config.team == Team.Spectator)
            {
                playerConfigData.RemovePlayerConfig(config);
                return;
            }
            else
            {
                var instantiatedPlayer = playerSpawner.InstantiatePlayer(config, playerCount);
                if (instantiatedPlayer != null)
                {
                    // Set up the player components that are specific to PlayerManager's responsibilities
                    SetupPlayer(config, instantiatedPlayer);

                    // Create a reference to the config in the player stats (mainly to access the team when calculating the score)
                    PlayerStatsManager.Instance.allPlayerStats[config.playerIndex].SetPlayerConfig(config);
                    playerCount++;
                }
            }
        }
    }

    private void SetupPlayer ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        instantiatedPlayer.GetComponent<PlayerController>().SetPlayerConfig(config);
        instantiatedPlayer.gameObject.tag = config.team.ToString();

        instantiatedPlayer.GetComponent<InputManager>().UpdateCurrentControlScheme(config.controlScheme.ToString());
        cameraManager.AddPlayerToCinemachineTargetGroup(instantiatedPlayer.transform);
        instantiatedPlayer.GetComponentInChildren<PlayerMonsterSpawner>().ConfigMonsterSpawner();

        // Instantiate player status component
        playerSpawner.InstantiatePlayerStatusComponent(config, instantiatedPlayer);

        SlowmotionController slowmotionController = instantiatedPlayer.GetComponentInChildren<SlowmotionController>();
        TimeManager.Instance.AssignBarToSMController(config, slowmotionController);
    }

    #region Getters & Setters

    public int GetUniquePlayerIndex ()
    {
        int newIndex = 0;

        // Loop through existing configs to find an unused index.
        while (playerConfigs.Any(p => p.playerIndex == newIndex))
        {
            newIndex++;
        }
        return newIndex;
    }

    public List<PlayerConfig> GetPlayerConfigList ()
    {
        return playerConfigs;
    }

    public PlayerConfig GetPlayerConfig ( int playerIndex )
    {
        return playerConfigs.FirstOrDefault(pc => pc.playerIndex == playerIndex);
    }

    public void SetHeroSelectManager ( HeroSelectManager manager )
    {
        heroSelectManager = manager;
    }
    public void SetPlayerSpawner ( PlayerSpawner _playerSpawner )
    {
        playerSpawner = _playerSpawner;
    }

    public void SetCameraManager ( CameraManager _cameraManager )
    {
        cameraManager = _cameraManager;
    }
    public void SetTeam ( int playerIndex, Team team )
    {
        PlayerConfig playerConfig = playerConfigs[playerIndex];
        playerConfig.team = team;
        playerConfigs[playerIndex] = playerConfig;
    }

    public void SetPlayerState ( int playerIndex, PlayerState playerState )
    {
        PlayerConfig playerConfig = playerConfigs[playerIndex];
        playerConfig.playerState = playerState;
        playerConfigs[playerIndex] = playerConfig;
    }

    public void SetAllPlayerState ( PlayerState playerState )
    {
        for (int i = 0; i < playerConfigs.Count; i++)
        {
            SetPlayerState(i, playerState);
        }
    }

    public void SetPlayerSelectedHero ( int _selectedHero, PlayerConfig config )
    {
        var configCopy = playerConfigs[config.playerIndex];
        configCopy.selectedPlayer = _selectedHero;
        configCopy.playerState = PlayerState.Ready;
        playerConfigs[config.playerIndex] = configCopy;

        if (heroSelectManager.AreAllPlayersReady())
            heroSelectManager.StartCountdownTimer();
    }
    #endregion

}
