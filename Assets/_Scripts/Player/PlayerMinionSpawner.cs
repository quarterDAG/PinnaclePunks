using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMinionSpawner : MonoBehaviour
{
    private Minion selectedMinion;
    private Transform spawnPoint;
    [SerializeField] private string tagToAttack;
    [SerializeField] private Inventory playerInventory;
    private InputManager inputManager;

    private void Awake ()
    {
        inputManager = GetComponent<InputManager>();
    }



    private void Update ()
    {
        if (inputManager.IsSpawnMinionPressed && spawnPoint != null && selectedMinion != null && tagToAttack != null)
        {
            if (playerInventory.GetSelectedMinionAmount(selectedMinion) <= 0) return;
            spawnPoint.GetComponent<MinionSpawnPoint>().SpawnMinion(selectedMinion, tagToAttack, this);


        }
    }


    public void SetTagToAttack ()
    {
        switch (gameObject.tag)
        {
            case "TeamA":
                tagToAttack = "TeamB";
                break;
            case "TeamB":
                tagToAttack = "TeamA";
                break;
            case "FreeForAll":
                tagToAttack = "FreeForAll";
                break;
        }
    }

    public void UpdateInventory ()
    {
        playerInventory.UpdateInventoryAfterSpawn();

    }

    private void OnTriggerEnter2D ( Collider2D other )
    {
        if (other.CompareTag("EnemySpawn"))
        {
            spawnPoint = other.transform;
        }
    }

    private void OnTriggerExit2D ( Collider2D other )
    {
        if (other.CompareTag("EnemySpawn"))
        {
            if (spawnPoint == other.transform)
            {
                spawnPoint = null;
            }
        }
    }

    public void SetSelectedMinion ( Minion minionPrefab )
    {
        selectedMinion = minionPrefab;
    }
}
