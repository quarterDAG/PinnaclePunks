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
    public List<SlowmotionController> slowmotionControllerList;
    public List<Bar> smBarList = new List<Bar>();
    public SafeZone safeZone;

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
                foreach (SlowmotionController slowmotionController in slowmotionControllerList)
                {
                    slowmotionController.SetIsPauseActive(false);
                }

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

        foreach (SlowmotionController slowmotionController in slowmotionControllerList)
        {
            slowmotionController.SetIsPauseActive(isPauseActive);
        }

    }

    public void Rematch ()
    {
        AssignAllManagers();

        ResetInputManager();
        ResetPlayerSpawner();
        ResetLives();
        ResetSMBars();
        ResetInventories();
        ResetMonsterList();
        InitializePlayers();
        safeZone.ResetSafeZone();

        ClearManagerAssignments();
        AssignAllManagers();

        StopTime(false);

        isPauseActive = false;
    }

    public void ClearManagerAssignments ()
    {
        livesManagerList.Clear();
        smBarList.Clear();
        slowmotionControllerList.Clear();
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


        if (playerSpawner == null)
            Debug.LogError("PlayerSpawner not found in the scene!");
        if (livesManagerList.Count == 0)
            Debug.LogError("No LivesManager found in the scene!");
        if (inventoryList.Count == 0)
            Debug.LogError("No Inventory found in the scene!");

    }

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
        foreach (Bar smBar in smBarList)
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
        PlayerManager.Instance.InitializePlayers();
    }

    private void ResetMonsterList ()
    {
        MinionsManager.Instance.ResetMinionList();
    }


    #region Getters & Setters

    public void AddInventory ( Inventory _inventory )
    {
        inventoryList.Add(_inventory);
    }

    public void AddLivesManager ( LivesManager _livesManager )
    {
        livesManagerList.Add(_livesManager);
    }

    public void AddSMController ( SlowmotionController _smController )
    {
        slowmotionControllerList.Add(_smController);
    }

    public void AddSMBar ( Bar _smBar )
    {
        smBarList.Add(_smBar);
    }

    public void SetPauseMenu ( PauseMenuController _pmController )
    {
        pauseMenuController = _pmController;
    }

    public void SetSafeZone ( SafeZone _safeZone )
    {
        safeZone = _safeZone;
    }

    #endregion
}
