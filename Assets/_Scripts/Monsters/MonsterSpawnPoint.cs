using System.Threading.Tasks;
using UnityEngine;

public class MonsterSpawnPoint : MonoBehaviour
{
    private Monster currentEnemyInstance;

    public void SpawnMonster ( Monster enemyPrefab, string TagToAttack, PlayerMonsterSpawner playerMonsterSpawner )
    {
        if (currentEnemyInstance == null || currentEnemyInstance.IsDead)
        {
            currentEnemyInstance = Instantiate(enemyPrefab, transform.position, transform.rotation) as Monster;
            currentEnemyInstance.OnDeath += HandleEnemyDeath;
            currentEnemyInstance.SetTagToAttack(TagToAttack);
            currentEnemyInstance.gameObject.tag = playerMonsterSpawner.gameObject.tag; 
            playerMonsterSpawner.UpdateInventory();
        }
        else
        {
            Debug.Log("An enemy is still active. Cannot spawn another one.");
        }
    }

    private async void HandleEnemyDeath ()
    {
        if (currentEnemyInstance != null)
        {
            await Task.Delay(3000);
            Destroy(currentEnemyInstance.gameObject);
            currentEnemyInstance = null;
        }
    }

    void OnDestroy ()
    {
        if (currentEnemyInstance != null)
        {
            currentEnemyInstance.OnDeath -= HandleEnemyDeath;
        }
    }
}
