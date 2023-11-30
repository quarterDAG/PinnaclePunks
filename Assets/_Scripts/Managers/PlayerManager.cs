using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using static PlayerConfigData;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    [SerializeField] PlayerConfigData playerConfigData;

    private HeroSelectController heroSelectController;
    private MapSelectController mapSelectController;
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

    public void InitializeHeroSelectors ()
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
                var instantiatedSelector = heroSelectController.InstantiateSelector(config, playerCount);
                if (instantiatedSelector != null)
                {
                    // Create a reference to the config in the player stats (mainly to access the team when calculating the score)
                    PlayerStatsManager.Instance.allPlayerStats[config.playerIndex].SetPlayerConfig(config);
                    playerCount++;
                }
            }
        }
    }

    public void InitializeMapSelectors ()
    {
        // Ensure that playerConfigs are already initialized and populated
        if (playerConfigs != null && playerConfigs.Count > 0)
        {
            for (int i = 0; i < playerConfigs.Count; i++)
            {
                var config = playerConfigs[i];
                mapSelectController.InstantiateSelector(config, config.playerIndex);
            }
        }
        else
        {
            Debug.LogError("Player configs are not initialized");
        }
    }

    public void InitializeHeroes ()
    {
        playerCount = 0;

        foreach (var config in playerConfigs)
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

    private void SetupPlayer ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        instantiatedPlayer.GetComponent<PlayerController>().SetPlayerConfig(config);
        instantiatedPlayer.gameObject.tag = config.team.ToString();

        cameraManager.AddPlayerToCinemachineTargetGroup(instantiatedPlayer.transform);
        instantiatedPlayer.GetComponentInChildren<PlayerMinionSpawner>().ConfigMinionSpawner();

        instantiatedPlayer.GetComponentInChildren<PlayerParticlesAndAudio>().SetColor(config.playerColor);

        // Instantiate Avatar component
        playerSpawner.InstantiateHeroAvatarComponent(config, instantiatedPlayer);
    }

    public void ResetPlayerConfigs ()
    {
        // Clear the list of player configurations
        playerConfigs.Clear();

        // Reset the player count
        playerCount = 0;
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

    public void SetHeroSelectController ( HeroSelectController controller )
    {
        heroSelectController = controller;
    }

    public void SetMapSelectController ( MapSelectController controller )
    {
        mapSelectController = controller;
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
        configCopy.selectedHero = _selectedHero;
        configCopy.playerState = PlayerState.Ready;
        playerConfigs[config.playerIndex] = configCopy;

        if (heroSelectController.AreAllPlayersReady())
            heroSelectController.StartCountdownTimer();
    }
    #endregion

}
