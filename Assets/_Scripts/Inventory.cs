using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public PlayerMonsterSpawner spawner;
    public List<Enemy> monsterPrefabs;

    // Call this method when a button is clicked
    public void OnMonsterSelected ( int monsterIndex )
    {
        if (monsterIndex >= 0 && monsterIndex < monsterPrefabs.Count)
        {
            Enemy selectedMonsterPrefab = monsterPrefabs[monsterIndex];
            spawner.SetSelectedMonster(selectedMonsterPrefab);
        }
        else
        {
            Debug.LogError("Monster index out of range: " + monsterIndex);
        }
    }
}
