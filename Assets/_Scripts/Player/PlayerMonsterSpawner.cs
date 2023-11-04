using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMonsterSpawner : MonoBehaviour
{
    private Monster selectedMonster;
    private Transform spawnPoint;
    private string tagToAttack;
    [SerializeField] private Inventory teamInventory;
    private InputManager inputManager;

    private void Awake ()
    {
        inputManager = GetComponent<InputManager>();
    }

 

    private void Update ()
    {
        if (inputManager.IsSpawnMonsterPressed && spawnPoint != null && selectedMonster != null && tagToAttack != null)
        {
            if (teamInventory.GetSelectedMonsterAmount(selectedMonster) <= 0) return;
            spawnPoint.GetComponent<MonsterSpawnPoint>().SpawnMonster(selectedMonster, tagToAttack, this);

        }
    }


    public void ConfigMonsterSpawner ()
    {

        tagToAttack = (gameObject.tag == "TeamA") ? "TeamB" : "TeamA";

        Inventory[] allInventories = FindObjectsOfType<Inventory>();

        foreach (var inventory in allInventories)
        {
            if (inventory.tag == gameObject.tag)
            {
                teamInventory = inventory;
                inventory.AddInventoryOwner(this);
                break;
            }
        }
    }

    public void UpdateInventory ()
    {
        teamInventory.UpdateInventoryAfterSpawn();

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

    public void SetSelectedMonster ( Monster monsterPrefab )
    {
        selectedMonster = monsterPrefab;
    }
}
