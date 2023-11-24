using System.Threading.Tasks;
using UnityEngine;

public class MinionSpawnPoint : MonoBehaviour
{
    private Minion currentMinionInstance;
    [SerializeField] private float spawnAdjustmentY = -1.2f;
    private Vector2 spawnPosition;

    private void Awake ()
    {
        spawnPosition = new Vector2(transform.position.x, transform.position.y + spawnAdjustmentY);
    }

    public void SpawnMinion ( Minion enemyPrefab, string TagToAttack, PlayerMinionSpawner playerMinionSpawner )
    {
        if (currentMinionInstance == null || currentMinionInstance.IsDead)
        {
            currentMinionInstance = Instantiate(enemyPrefab, spawnPosition, transform.rotation);
            currentMinionInstance.OnDeath += HandleMinionDeath;
            currentMinionInstance.SetTagToAttack(TagToAttack);
            currentMinionInstance.gameObject.tag = playerMinionSpawner.gameObject.tag;
            playerMinionSpawner.UpdateInventory();
            MinionsManager.Instance.AddMinion(currentMinionInstance);
        }
        else
        {
            Debug.Log("An enemy is still active. Cannot spawn another one.");
        }
    }

    private async void HandleMinionDeath ()
    {
        if (currentMinionInstance != null)
        {
            await Task.Delay(3000);
            Destroy(currentMinionInstance.gameObject);
            currentMinionInstance = null;
        }
    }

    void OnDestroy ()
    {
        if (currentMinionInstance != null)
        {
            currentMinionInstance.OnDeath -= HandleMinionDeath;
        }
    }
}
