using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PersistenceManager
{
    public enum PersistentEnemyId
    {
        Minion,
        Boss
    }
    public static PersistentEnemyId enemyId;

    public static int enemyCount { get; private set; }
    public static EnemyType enemyType { get; private set; }
    public static BossName bossName { get; private set; }
    public static Dictionary<string, List<int>> playerStats { get; private set; } = new Dictionary<string, List<int>>();

    public static void SetPersistentStateMinion(EnemyType enemyType, int enemyCount)
    {
        PersistenceManager.enemyId = PersistentEnemyId.Minion;
        PersistenceManager.enemyType = enemyType;
        PersistenceManager.enemyCount = enemyCount;
    }

    public static void SetPersistentStateBoss(BossName bossName)
    {
        PersistenceManager.enemyId = PersistentEnemyId.Boss;
        PersistenceManager.bossName = bossName;
    }

    public static void SavePlayerStats(List<BattleEntity> plrLst)
    {
        foreach (BattleEntity plr in plrLst)
        {
            playerStats[plr.entityName] = new List<int> { plr.cHP, plr.cMP };
            Debug.Log(plr.cHP + " " + plr.cMP);
        }
    }

    public static void LoadPlayerData(ref List<BattleEntity> plrLst)
    {
        foreach (BattleEntity plr in plrLst)
        {
            foreach (string name in playerStats.Keys)
            {
                if (plr.entityName == name && plr.cHP > 0)                              // skip if not strictly greater than zero as it will otherwise mess up revive thingy
                {
                    Debug.Log(playerStats[name][0] + " " + playerStats[name][1]);
                    plr.cHP = playerStats[name][0];
                    plr.cMP = playerStats[name][1];
                }
            }
        }
    }
}