using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMonsterSpawner : MonoBehaviour
{
    private Enemy selectedMonster;
    private Transform spawnPoint;

    private void Update ()
    {
        if (Input.GetKeyDown(KeyCode.R) && spawnPoint != null && selectedMonster != null)
        {
            Instantiate(selectedMonster, spawnPoint.position, Quaternion.identity);
            // Optionally clear the selection after spawning
            selectedMonster = null;
        }
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

    public void SetSelectedMonster ( Enemy monsterPrefab )
    {
        selectedMonster = monsterPrefab;
    }
}
