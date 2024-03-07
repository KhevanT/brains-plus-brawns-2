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
}