using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PersistentSceneManager
{
    public static string currScene { get; private set; }                                         // always other than BattleScene
    public static Vector3 playerPos { get; private set; }
    public static List<GameObject> enemyObjs { get; private set; }
    public static int enemyIndex { get; private set; }

    public static void InitScene(string scene, Vector3 playerPos, ref List<GameObject> enemyObjs, string currEnemy)
    {
        enemyObjs.Clear();

        currScene = scene;
        PersistentSceneManager.playerPos = playerPos;
        PersistentSceneManager.enemyObjs = enemyObjs;

        SetEnemyIndex(currEnemy);
    }

    public static int GetIndexFromEnemy(string enemy)
    {
        for (int index = 0; index < enemyObjs.Count; ++index)
        {
            if (enemyObjs[index].name == enemy)
            {
                return index;
            }
        }

        return -1;
    }

    public static bool SetEnemyIndex(string enemy)
    {
        int index = GetIndexFromEnemy(enemy);
        enemyIndex = index;

        if (index == -1)
            return false;
        else
            return true;
    }

    public static void DisableEnemyWithCurrIndex()
    {
        if (enemyIndex != -1)
        {
            enemyObjs[enemyIndex].SetActive(false);
            enemyIndex = -1;
        }
    }
}

public enum Kingdom
{
    Overworld,
    GICT,
    AMSOM,
    SAS,
    BATTLE
}
