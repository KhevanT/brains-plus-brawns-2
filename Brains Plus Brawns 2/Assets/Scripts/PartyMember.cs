using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

// PartyMember class, child of BattleEntity
// Gives functionality for all player characters in combat
public class PartyMember : BattleEntity
{
    // Basic identifiers
    string baseClass; 

    // Moves
    Move targetMove = null;
    Move sweepMove = null;

    // CSV
    string playerStat_filePath = "/CSV/Player_Stats.csv";

    // Constructor
    public PartyMember(string name, string baseclass)
    {
        // Identifiers
        entityType = "PartyMember";
        entityName = name;
        baseClass = baseclass;

        // Read stats and move data from csv
        ReadFromCSV(playerStat_filePath);

        // Current stats
        cHP = mHP;
        cMP = mMP;
    }

    // Reads data from relative file path in unity editor
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

                if (vals[0] == baseClass) // find line for correct class
                {
                    // Assign values to party member

                    // Base Stats 
                    mHP = int.Parse(vals[1]);
                    mMP = int.Parse(vals[2]);
                    attack = int.Parse(vals[3]);
                    defense = int.Parse(vals[4]);
                    speed = int.Parse(vals[5]);

                    // Moves
                    targetMove = new Move(vals[6], vals[7], int.Parse(vals[8]), int.Parse(vals[9]));
                    sweepMove = new Move(vals[10], vals[11], int.Parse(vals[12]), int.Parse(vals[13]));
                }
            }
        }
        else
        {
            Debug.Log("File not found.");
        }
    }

    // Debug function to check all party member stats
    public void PrintAllStats()
    {
        Debug.Log("Name: " + entityName);
        Debug.Log("Class: " + baseClass);
        Debug.Log("mHP: " + mHP);
        Debug.Log("mMP: " + mMP);
        Debug.Log("cHP: " + cHP);
        Debug.Log("cMP: " + cMP);
        Debug.Log("Attack: " + attack);
        Debug.Log("Defense: " + mMP);
        Debug.Log("Speed: " + cHP);
        Debug.Log("Target Move: " + targetMove.ToString());
        Debug.Log("Sweep Move: " + sweepMove.ToString());
    }

    // Overrides to string method, displays name, hp and mp
    public override string ToString()
    {
        string memberData = "Name: " + entityName
            + "HP: " + cHP + "/" + mHP
            + "MP: " + cMP + "/" + mMP;

        return memberData;
    }
}
