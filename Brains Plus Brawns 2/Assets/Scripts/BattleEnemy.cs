using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;

// BattleEnemy class, child of BattleEntity
// Gives functionality for Minions, Boss enemies
public class BattleEnemy : BattleEntity
{
    // Basic identifiers
    string enemyType; // Minion / Boss

    // Moves (Minions have 1, Boss have 2)
    Move move1 = null;
    Move move2 = null;

    // CSV
    string enemyStat_filePath = "/CSV/Enemy_Stats.csv";

    // Constructor
    public BattleEnemy(string name, string type)
    {
        // Identifiers
        entityType = "Enemy";
        entityName = name;
        enemyType = type;

        // Bosses get 2 turns
        if (type == "Boss")
            noOfTurns = 2;

        // Read stats and move data from csv
        ReadFromCSV(enemyStat_filePath);

        // Current stats
        cHP = mHP;
        cMP = mMP;
    }

    void ReadFromCSV(string filePath)
    {
        string fullPath = Application.dataPath + filePath;
        Debug.Log(fullPath);

        if (File.Exists(fullPath))
        {
            // Open file and extract array of all lines
            string[] allLines = File.ReadAllLines(fullPath);

            foreach (string s in allLines) // Go through all lines
            {
                string[] vals = s.Split(',');
                if (vals[0] == entityName) // find line for correct class
                {
                    // Assign values to party member

                    // Base Stats 
                    mHP = int.Parse(vals[2]);
                    mMP = int.Parse(vals[3]);
                    attack = int.Parse(vals[4]);
                    defense = int.Parse(vals[5]);
                    speed = int.Parse(vals[6]);

                    // Moves
                    move1 = new Move(vals[7], vals[8], int.Parse(vals[9]), int.Parse(vals[10]));
                    if (enemyType == "Boss")
                        move2 = new Move(vals[11], vals[12], int.Parse(vals[13]), int.Parse(vals[14]));
                }
            }
        }
        else
        {
            Debug.Log("File not found.");
        }
    }

    // Debug function to check all enemy stats
    public void PrintAllStats()
    {
        Debug.Log("Name: " + entityName);
        Debug.Log("Class: " + entityType);
        Debug.Log("mHP: " + mHP);
        Debug.Log("mMP: " + mMP);
        Debug.Log("cHP: " + cHP);
        Debug.Log("cMP: " + cMP);
        Debug.Log("Attack: " + attack);
        Debug.Log("Defense: " + mMP);
        Debug.Log("Speed: " + cHP);
        Debug.Log("Move 2: " + move1.ToString());
        if(entityType == "Boss")
            Debug.Log("Move 2: " + move2.ToString());
    }

    // Overrides to string method, displays name, hp and mp
    public override string ToString()
    {
        string enemyData = "Name: " + entityName
            + "HP: " + cHP + "/" + mHP
            + "MP: " + cMP + "/" + mMP;

        return enemyData;
    }
}
