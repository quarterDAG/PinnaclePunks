using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionsManager : MonoBehaviour
{
    public static MinionsManager Instance { get; private set; }

    public List<Minion> minionList = new List<Minion>();

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

    public void AddMinion ( Minion _minion )
    {
        minionList.Add(_minion);
    }

    public void ResetMinionList ()
    {
        DestroyAllMinions();
        minionList.Clear();
    }

    private void DestroyAllMinions ()
    {
        foreach (Minion _minion in minionList)
        {
            Destroy(_minion.gameObject);
        }
    }

}
