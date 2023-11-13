using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonstersManager : MonoBehaviour
{
    public static MonstersManager Instance { get; private set; }

    public List<Monster> monsterList = new List<Monster>();

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

    public void AddMonster ( Monster _monster )
    {
        monsterList.Add(_monster);
    }

    public void ResetMonsterList ()
    {
        DestroyAllMonsters();
        monsterList.Clear();
    }

    private void DestroyAllMonsters ()
    {
        foreach (Monster _monster in monsterList)
        {
            Destroy(_monster.gameObject);
        }
    }

}
