using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerStats", menuName = "Player/Stats")]
public class PlayerStats : ScriptableObject
{
    public string playerName;
    public int kills;
    public int deaths;
    public int damage;

    public void ResetStats ()
    {
        kills = 0;
        deaths = 0;
        damage = 0;
    }

    // You can add methods here to modify the stats as needed
    public void RecordKill () => kills++;
    public void RecordDeath () => deaths++;
    public void RecordDamage ( int amount ) => damage += amount;

}
