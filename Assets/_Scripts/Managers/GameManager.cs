// A manager script that would go on a game manager object
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<LivesManager> livesManagerList = new List<LivesManager>();
    public List<Inventory> inventoryList = new List<Inventory>();
    public PlayerSpawner playerSpawner;
    public CameraManager cameraManager;


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


    public void Rematch ()
    {
        AssignAllMaanger();

        ResetLives();
        ResetInventories();
        ResetPlayerSpawner();
        //ResetCameraManager();
        ResetMonsterList();
        InitializePlayers();

        ClearManagerAssignments();
        AssignAllMaanger();
    }

    public void ClearManagerAssignments ()
    {
        livesManagerList.Clear();
        inventoryList.Clear();
        playerSpawner = null;
        cameraManager = null;
    }

    private void AssignAllMaanger ()
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

    private void ResetLives ()
    {
        foreach (LivesManager livesManager in livesManagerList)
        {
            livesManager.ResetLives();
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

    private void ResetCameraManager ()
    {
        cameraManager.ResetCameraManager();
    }

    private void InitializePlayers ()
    {
        PlayerManager.Instance.InitializePlayers();
    }

    private void ResetMonsterList ()
    {
        MonstersManager.Instance.ResetMonsterList();
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

    #endregion
}
