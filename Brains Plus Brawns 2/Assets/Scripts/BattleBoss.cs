using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class BattleBoss : BattleEntity
{
    // Basic identifiers
    public BossName bossName;

    // CSV
    string enemyStat_filePath = "/CSV/Enemy_Stats.csv";

    // Intialise boss
    public void initialiseBoss(BossName name)
    {
        bossName = name;

        // Identifiers
        entityType = EntityType.Boss;
        entityName = bossName.ToString();

        // Turn management
        noOfTurns = 2;
        turnsLeft = noOfTurns;

        // Move management
        moves = new Move[2]; // bosses have 2 moves

        // Read stats and move data from csv
        ReadFromCSV(enemyStat_filePath);

        // Current stats
        cHP = mHP;
        cMP = mMP;
    }

    /*
    // Constructor (for starting battle screens via script
    // IT GIVES ERROR, PLS FIX
    public BattleBoss(BossName bossname)
    {
        bossName = bossname;
    }
    */

    // Start is called before first frame update
    void Start()
    {
        
    }
}

// Boss names
public enum BossName
{
    Hephaestus,
    King,
    Necromancer
}
