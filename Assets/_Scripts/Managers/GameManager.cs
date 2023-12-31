// A manager script that would go on a game manager object
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<LivesManager> livesManagerList = new List<LivesManager>();
    public List<Inventory> inventoryList = new List<Inventory>();
    public PlayerSpawner playerSpawner;
    public CameraManager cameraManager;
    public List<Bar> manaBarList = new List<Bar>();
    public DangerZone safeZone;
    public InfinitePlatformGenerator platformGenerator;

    public PauseMenuController pauseMenuController;

    private bool isPauseActive;
    private InputManager activePauseInputManager;


    private void Awake ()
    {
        Singleton();
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

    public void PauseMenu ( InputManager _inputManager )
    {
        if (!isPauseActive)
        {
            activePauseInputManager = _inputManager; // Store the reference

            pauseMenuController.SetInputManager(_inputManager);
            pauseMenuController.ShowPauseMenu(_inputManager);
            StopTime(true);
        }
        else
        {
            // Check if the _inputManager is the same instance that activated the pause
            if (_inputManager == activePauseInputManager)
            {
                pauseMenuController.HidePauseMenu();

                StopTime(false);


                activePauseInputManager = null; // Reset the reference
            }
        }
    }

    public void StopTime ( bool _isPause )
    {
        if (_isPause)
        {
            Time.timeScale = 0f;
            isPauseActive = true;
        }
        else
        {
            Time.timeScale = 1f;
            isPauseActive = false;
        }
    }

    public void Rematch ()
    {
        AssignAllManagers();

        safeZone.ResetSafeZone();
        ResetInputManager();
        ResetPlayerSpawner();
        ResetLives();
        ResetSMBars();
        ResetInventories();
        ResetMonsterList();
        InitializePlayers();

        if (platformGenerator != null)
            platformGenerator.ResetGenerator();

        ClearManagerAssignments();
        AssignAllManagers();

        StopTime(false);

        isPauseActive = false;
    }

    public void ClearManagerAssignments ()
    {
        livesManagerList.Clear();
        manaBarList.Clear();
        inventoryList.Clear();

        playerSpawner = null;
        cameraManager = null;
    }

    private void AssignAllManagers ()
    {
        livesManagerList.AddRange(FindObjectsOfType<LivesManager>());
        inventoryList.AddRange(FindObjectsOfType<Inventory>());
        playerSpawner = FindObjectOfType<PlayerSpawner>();
        cameraManager = FindObjectOfType<CameraManager>();
        //platformGenerator = FindObjectOfType<InfinitePlatformGenerator>();


        if (playerSpawner == null)
            Debug.LogError("PlayerSpawner not found in the scene!");
        if (livesManagerList.Count == 0)
            Debug.LogError("No LivesManager found in the scene!");
        if (inventoryList.Count == 0)
            Debug.LogError("No Inventory found in the scene!");

    }

    #region Resets

    private void ResetInputManager ()
    {

        activePauseInputManager = null;
    }

    private void ResetLives ()
    {
        foreach (LivesManager livesManager in livesManagerList)
        {
            livesManager.ResetLives();
        }
    }

    private void ResetSMBars ()
    {
        foreach (Bar smBar in manaBarList)
        {
            smBar.ResetBar();
        }
    }

    private void ResetInventories ()
    {
        foreach (Inventory inventory in inventoryList)
        {
            inventory.ResetInventory();
        }
    }

    private void ResetPlayerSpawner ()
    {
        playerSpawner.ResetPlayerSpawner();
    }

    private void InitializePlayers ()
    {
        PlayerManager.Instance.InitializeHeroes();
    }

    private void ResetMonsterList ()
    {
        MinionsManager.Instance.ResetMinionList();
    }

    #endregion

    #region Getters & Setters

    public void AddInventory ( Inventory _inventory )
    {
        inventoryList.Add(_inventory);
    }

    public void AddPlayerSpawner ( PlayerSpawner _playerSpawner )
    {
        playerSpawner = _playerSpawner;
    }

    public void AddLivesManager ( LivesManager _livesManager )
    {
        livesManagerList.Add(_livesManager);
    }

    public void AddSMBar ( Bar _smBar )
    {
        manaBarList.Add(_smBar);
    }

    public void SetPauseMenu ( PauseMenuController _pmController )
    {
        pauseMenuController = _pmController;
    }

    public void SetSafeZone ( DangerZone _safeZone )
    {
        safeZone = _safeZone;
    }

    public void SetPlatformGenerator ( InfinitePlatformGenerator _platformGenerator )
    {
        platformGenerator = _platformGenerator;
    }

    #endregion
}
