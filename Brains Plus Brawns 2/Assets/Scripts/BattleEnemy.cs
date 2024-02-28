using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor.Experimental.GraphView;

// BattleEnemy class, child of BattleEntity
// Gives functionality for Minion enemies
public class BattleEnemy : BattleEntity
{
    // Basic identifiers
    public EnemyType enemyType;

    // CSV
    string enemyStat_filePath = "/CSV/Enemy_Stats.csv";

    // Initialise enemy
    public void initialiseEnemy(EnemyType type)
    {
        // Identifiers
        entityType = EntityType.Enemy;
        enemyType = type;
        entityName = enemyType.ToString();

        moves = new Move[1];

        // Read stats and move data from csv
        ReadFromCSV(enemyStat_filePath);

        // Current stats
        cHP = mHP;
        cMP = mMP;
    }

    /*
    // Constructor (for starting battle screens via script
    // IT GIVES ERROR, PLS FIX
    public BattleEnemy(EnemyType enemytype)
    {
        enemyType = enemytype;
    }
    */

    // Start is called before first frame update
    void Start()
    {

    }
}

// Enumeration for all enemy variety
public enum EnemyType
{
    Dwarf
}
