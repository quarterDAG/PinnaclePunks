using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] HPBar hpBar;

    [System.Serializable]
    public class EnemyStates
    {
        public int Health = 100;
    }

    public EnemyStates stats = new EnemyStates();

    public void DamageEnemy ( int damage )
    {
        stats.Health -= damage;

        hpBar.UpdateHPFillUI(stats.Health);

        if (stats.Health <= 0)
        {
            GameMaster.KillEnemy(this);
        }

    }


}
