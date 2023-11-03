using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMonsterSpawner : MonoBehaviour
{
    private Monster selectedMonster;
    private Transform spawnPoint;
    [SerializeField] private string TagToAttack;
    [SerializeField] private Inventory inventory;
    private InputManager inputManager;

    private void Awake ()
    {
        inputManager = GetComponent<InputManager>();
    }

    private void Update ()
    {
        if (inputManager.IsSpawnMonsterPressed && spawnPoint != null && selectedMonster != null)
        {
            if (inventory.GetSelectedMonsterAmount(selectedMonster) <= 0) return;
            spawnPoint.GetComponent<MonsterSpawnPoint>().SpawnMonster(selectedMonster, TagToAttack, this);

        }
    }

    public void UpdateInventory ()
    {
        inventory.UpdateInventoryAfterSpawn();

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
