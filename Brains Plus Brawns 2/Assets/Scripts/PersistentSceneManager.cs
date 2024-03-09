using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PersistentSceneManager
{
    public static string currScene { get; private set; }                                         // always other than BattleScene
    public static Vector3 playerPos { get; private set; } = new Vector3(0.5f, 0.5f, 0);
    public static List<string> enemyObjs { get; private set; } = new List<string>();
    public static List<string> disabledEnemyObjs { get; private set; } = new List<string>();
    public static int enemyIndex { get; private set; } = -1;

    public static void InitScene(string scene, Vector3 playerPos, List<GameObject> enemyObjs, string currEnemy)
    {
        Debug.Log("Player Pos " + playerPos);
        currScene = scene;
        PersistentSceneManager.playerPos = playerPos;

        foreach (GameObject obj in enemyObjs)
            PersistentSceneManager.enemyObjs.Add(obj.name);

        Debug.Log(enemyObjs.Count);
        SetEnemyIndex(currEnemy);
    }

    public static int GetIndexFromEnemy(string enemy)
    {
        Debug.Log(enemyObjs.Count);
        for (int index = 0; index < enemyObjs.Count; ++index)
        {
            Debug.Log("REACHED");
            Debug.Log(enemy + " " + enemyObjs[index]);
            if (enemyObjs[index] == enemy)
            {
                return index;
            }
        }

        return -1;
    }

    public static string GetCurrEnemy()
    {
        return enemyObjs[enemyIndex];
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

    private static void DisableEnemyWithCurrIndex()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("NPC");
        foreach (string dso in disabledEnemyObjs)
        {
            foreach (GameObject go in gameObjects)
            {
                if (go.name == dso)
                {
                    go.SetActive(false);
                }
            }
        }

        if (enemyIndex != -1)
        {
            string co = GetCurrEnemy();

            foreach (GameObject go in gameObjects)
            {
                if (go.name == co)
                {
                    go.SetActive(false);
                    disabledEnemyObjs.Add(co);
                }
            }
        }
    }

    private static void MovePlayerToPosXY(ref GameObject pl, ref GameObject plmvpt)
    {
        Debug.Log(playerPos);
        pl.transform.position = playerPos;
        plmvpt.transform.position = playerPos;
    }

    public static void SetupScene(ref GameObject pl, ref GameObject plmvpt)
    {
        MovePlayerToPosXY(ref pl, ref plmvpt);
        DisableEnemyWithCurrIndex();
    }
}

public enum Kingdom
{
    Overworld,
    GICT,
    AMSOM,
    SAS,
    BATTLE,
    Demo
}
