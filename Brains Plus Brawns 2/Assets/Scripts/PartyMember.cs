using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

// PartyMember class, child of BattleEntity
// Gives functionality for all player characters in combat
public class PartyMember : BattleEntity
{
    // Basic identifiers 
    public string memberName;
    public PlayerClass baseClass;

    // CSV
    string playerStat_filePath = "/CSV/Player_Stats.csv";

    // Initialise function
    public void initialiseMember(string name, PlayerClass baseClass)
    {
        this.baseClass = baseClass;
        memberName = name;

        // Identifiers
        entityType = EntityType.PartyMember;
        entityName = memberName;

        // Turn management
        noOfTurns = 1;
        turnsLeft = noOfTurns;

        // Move management
        moves = new Move[2]; // players have 2 moves

        // Read stats and move data from csv
        ReadFromCSV(playerStat_filePath);

        // Current stats
        cHP = mHP;
        cMP = mMP;
    }

    // Constructors
    /*
    // Constructor for first battle / creation
    public PartyMember(string name, PlayerClass baseClass)
    {
        this.baseClass = baseClass;
        memberName = name;

        // Identifiers
        entityType = EntityType.PartyMember;
        entityName = memberName;

        // Turn management
        noOfTurns = 2;
        turnsLeft = noOfTurns;

        // Move management
        moves = new Move[2]; // players have 2 moves

        // Read stats and move data from csv
        ReadFromCSV(playerStat_filePath);

        // Current stats
        cHP = mHP;
        cMP = mMP;
    }

    
    // Constructor for subsequent battles
    public PartyMember(string name, PlayerClass baseClass, int[] curStats)
    {
        this.baseClass = baseClass;
        memberName = name;

        // Current stats
        cHP = curStats[0];
        cMP = curStats[1];

        // revive if cHP below 0

        turnsLeft = noOfTurns;
    }

    */

    // Start is called before first frame update
    void Start()
    {
        //
    }

    // Reads data from relative file path in unity editor
    // It is overriden because of this line " if (vals[0] == baseClass.ToString()) "
    // & since baseClass is a child property, we cant access it in parent class
    // POTENTIAL FIX: it may be possible to fix this in the code to minimise repetition
    public override void ReadFromCSV(string filePath)
    {
        string fullPath = Application.dataPath + filePath;
        // Debug.Log(fullPath);
        if (File.Exists(fullPath))
        {
            // Open file and extract array of all lines
            string[] allLines = File.ReadAllLines(fullPath);

            foreach (string s in allLines) // Go through all lines
            {
                string[] vals = s.Split(',');

                if (vals[0] == baseClass.ToString()) // find line for correct class
                {
                    // Assign values to party member

                    // Base Stats 
                    mHP = int.Parse(vals[1]);
                    mMP = int.Parse(vals[2]);
                    attack = int.Parse(vals[3]);
                    defense = int.Parse(vals[4]);
                    speed = int.Parse(vals[5]);

                    // Moves
                    moves[0] = new Move(vals[6], vals[7], int.Parse(vals[8]), int.Parse(vals[9]));
                    moves[1] = new Move(vals[10], vals[11], int.Parse(vals[12]), int.Parse(vals[13]));
                }
            }
        }
        else
        {
            Debug.Log("File not found.");
        }
    }

    // Stores current state of party member
    // Used to recall previous battle state when starting a new encounter
    public int[] storeCurrentState()
    {
        int[] curStats = {cHP, cMP};

        return curStats;
    }
}

// Enumeration for player classes
public enum PlayerClass
{
    Wizard,
    Knight,
    Archer,
    Brawler
}
