using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

// BattleEntity class, parent class for BattleEnemy and PartyMember
// Manages health, basic stats, moves, etc for all entities involved in combat
public class BattleEntity : MonoBehaviour
{
    // 1. Stats

    // Identifier
    public string entityName;
    public EntityType entityType;

    // DOA status
    public bool alive = true;

    // Basic stats (differ per class)
    public int mHP;
    public int mMP;
    protected int attack;
    protected int defense;
    public int speed;

    // Current stats
    public int cHP;
    public int cMP;

    // Other stats
    public bool guarding = false;
    float guardingMultiplier = 0.5f;

    // Moves
    protected Move[] moves = new Move[1]; // default 1 move

    // Turn management
    public int noOfTurns = 1; // 2 for boss, 1 for everyone else
    public int turnsLeft = 1; // 2 for boss
    public TurnChoice turnChoice;

    // Temporary inventory <REMOVE>
    public int hpPotionCount = 5;
    public int mpPotionCount = 5;

    // Replenish stats
    int mpGuardReplenish = 1;
    int mpPotionReplenish = 5;
    int hpPotionReplenish = 10;


    // 2. Battle Functions

    // Turn Handling
    // returns turnChoice based on playerinput or random, based on entityType
    public TurnChoice getTurnChoice() 
    {
        if (entityType == EntityType.Enemy)
        {
            // For minions, they can only attack
            turnChoice = TurnChoice.Attack;
        }
        else
        {
            // For boss and player, turn choice will be displayed as buttons and they will choose
            // turnChoice = battleManager.getTurnChoice();
        }
        return turnChoice;
    }
    
    // Calls functions based on turn choice
    public void turnHandler(TurnChoice turnChoice,ref List<BattleEntity> oppTeam, int moveChoice=0, int oppIndex=0)
    {
        if(!alive)
        {
            Debug.Log(entityName + " is dead and cannot make a turn.");
        }
        else if (alive && turnsLeft > 0)
        {
            //store turn info
            string turnInfo = " ";

            // Turn off previous guard
            // POTENTIAL BUG: for boss doing guard in 1 turn and attack in next, this will not work!
            if (guarding && turnsLeft == 0)
            {
                guarding = false;
                Debug.Log(entityName + "'s previously raised guard has been lowered");
            }

            // Picks and calls relevant function
            // Acts on turn choice
            switch (turnChoice)
            {
                case BattleEntity.TurnChoice.Attack:  // 1. Attack
                    turnInfo = attackHandler(ref oppTeam, moveChoice, oppIndex);
                    break;

                case BattleEntity.TurnChoice.Guard:  // 2. Guard
                    turnInfo = setGuard();
                    break;

                case BattleEntity.TurnChoice.Heal:  // 3. Heal
                    if (hpPotionCount > 0 && cHP != mHP)
                        turnInfo = performHeal();
                    else
                        Debug.Log("Out of potions, or health is at maximum. Choose turn again.");
                    // turnHandler(turnChoice, ref oppTeam); // NEED TO GET TURN CHOICE AGAIN! 
                    break;
            }
            Debug.Log(turnInfo);

            // turn counter in battle manager
        }
    }

    // attackHandler Function, maps correct attack function for different entities
    // Returns a turn's info
    public string attackHandler(ref List<BattleEntity> oppTeam, int moveChoice, int oppIndex) // CHANGE NAME TO ATTACK HANDLER
    {
        string turnInfo;

        // Map correct attack function based on entity type
        if(entityType == EntityType.Enemy)
            turnInfo = performAttackNPC(ref oppTeam);
        else
            turnInfo = performAttackPlayer(ref oppTeam, moveChoice, oppIndex);

        return turnInfo;
    }

    // Handles attack logic for players who can choose their moves
    // uses MP, calculates dmg, asks entities to take damage
    // returns a turn's info
    public string performAttackPlayer(ref List<BattleEntity> oppTeam, int moveChoice, int oppIndex)
    {
        // Return info
        string turnInfo;

        // Ask for move 0 or 1 <use buttons>
        // DIFF BUTTONS FOR BOSS AND PLAYER, USE DIFF FUNCTIONS
        // int moveChoice = UnityEngine.Random.Range(0, 1); 

        // MP Management
        if (cMP >= moves[moveChoice].mpCost)
        {
            // Decrement MP
            cMP -= moves[moveChoice].mpCost;

            // Calculate and deal damage according to move type
            if (moves[moveChoice].type == Move.MoveType.Target) // 1. Target move
            {
                // Calculate damage for this randomly selected entity
                int dmg = calculateDMG(oppTeam[oppIndex], moves[moveChoice]);

                // Ask player to receive damage (guard breaking, death, all handled inside takeDMG)
                // POTENTIAL PROBLEM: since guard break & faint will be stated before turn info, sequence of logs may be screwed up
                oppTeam[oppIndex].takeDamage(dmg);

                // Turn info for 1 target
                turnInfo = entityName + " used " + moves[moveChoice].name
                    + " on " + oppTeam[oppIndex].entityName
                    + " and dealt " + dmg + " points of damage";
            }
            else // 2. Sweeping move
            {
                Debug.Log("SWEEPING MOVE");
                // Save victim info
                string oppList = "";
                string dmgList = "";

                // Calculate damage for each player
                // Ask them to take damage
                foreach (BattleEntity player in oppTeam)
                {
                    int dmg = calculateDMG(player, moves[moveChoice]);

                    // POTENTIAL PROBLEM: since guard break & faint will be stated before turn info, sequence of logs may be screwed up
                    player.takeDamage(dmg);

                    // player and dmg info
                    oppList += player.entityName + ", ";
                    dmgList += dmg + ", ";
                }

                // Turn info for all targets
                turnInfo = entityName + " used " + moves[moveChoice].name
                    + " on " + oppList + " and dealt " + dmgList + " points of damage respectively";
            }
        }
        else
        {
            turnInfo = entityName + " does not have enough MP to perform the move. Move unsuccessful.";
        }

        return turnInfo;
    }

    // Handles attack logic for NPCs who choose move and opponent at random
    // uses MP, calculates dmg, asks entities to take damage
    // returns a turn's info
    // Assumes NPCs only have 1 move and it is targetted type
    // QUESTION: Should enemies be granted unlimited MP because otherwise players can stall them out?
    public string performAttackNPC(ref List<BattleEntity> oppTeam)
    {
        string turnInfo;

        // MP management
        if (cMP >= moves[0].mpCost)
        {
            // Decrement MP
            cMP -= moves[0].mpCost;

            // Pick random index for a player from party (and make sure they are alive)
            int defenderChoice;
            do {
                defenderChoice = UnityEngine.Random.Range(0, oppTeam.Count);
            } while (!oppTeam[defenderChoice].alive);

            // Calculate damage for this randomly selected player
            int dmg = calculateDMG(oppTeam[defenderChoice], moves[0]);

            // Ask player to receive damage (guard breaking, death, all handled inside takeDMG)
            // POTENTIAL PROBLEM: since guard break & faint will be stated before turn info, sequence of logs may be screwed up
            oppTeam[defenderChoice].takeDamage(dmg);

            // Return turn info
            turnInfo = entityName + " used " + moves[0].name
                + " on " + oppTeam[defenderChoice].entityName
                + " and dealt " + dmg + " points of damage";
        }
        else
        {
            turnInfo = entityName + " does not have enough MP to perform the move. Move unsuccessful.";
        }

        return turnInfo;
    }

    // Guard function, turns on guard, replenishes MP
    // QUESTION: Should enemies be allowed to guard?
    public string setGuard()
    {
        guarding = true;
        cMP += mpGuardReplenish;

        string turnInfo = entityName + " put up its guard and replenished upto" + mpGuardReplenish + "points of MP";

        return turnInfo;

        // POTENTIAL EXPLOIT:
        // Bosses with 2 turns can set guard and attack too, pls fix that
    }

    // Heal, replenishes HP, uses potions (needs inventory access)
    public string performHeal()
    {
        string turnInfo;

        cHP += hpPotionReplenish;
        hpPotionCount--;

        turnInfo = entityName + " healed themselves and restored upto" + hpPotionReplenish + "points of HP";
        return turnInfo;
    }

    // calculateDMG, calculates damage to be dealt to an individual defender
    // (must be called once per alive defender even in case of sweeping move)
    public int calculateDMG(BattleEntity defender, Move move)
    {
        int dmg = 0;
        float randomFac = UnityEngine.Random.value + 0.5f; // rand val b/w 0.5 and 1.5

        // < NON FINAL DAMAGE FORMULA >
        dmg = (int)((this.attack * move.power * randomFac) / defender.defense);

        return dmg;
    }

    // Applies damage taken and checks if dead
    public void takeDamage (int dmg)
    {
        if (alive)
        {
            // checks if guard is on, reduces damage by guard defense multiplier val
            if (guarding)
            {
                // guard only lasts 1 hit
                guarding = false; 
                Debug.Log(entityName + "'s guard was broken");

                dmg = (int)(dmg * guardingMultiplier);
            }

            // Decrement health
            cHP -= dmg;
            
            // Checks if still alive
            isAlive();
        }
    }

    // isAlive, Checks if battle entity is alive
    public void isAlive()
    {
        if (cHP > 0)
        {
            alive = true;
        }
        else
        {
            // Kills entity for real
            cHP = 0; // not let current hp go below 0
            alive = false;
            Debug.Log(entityName + " has fainted");
        }
    }



    // 3. Stat Related Functions

    // Reads data from enemy csv
    public virtual void ReadFromCSV(string filePath)
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
                    moves[0] = new Move(vals[7], vals[8], int.Parse(vals[9]), int.Parse(vals[10]));
                    if (moves.Length > 1)
                        moves[1] = new Move(vals[11], vals[12], int.Parse(vals[13]), int.Parse(vals[14]));
                }
            }
        }
        else
        {
            Debug.Log("File not found.");
        }
    }

    // Overrides to string method, displays name, hp and mp
    public override string ToString()
    {
        string enemyData = "Name: " + entityName
            + "HP: " + cHP + "/" + mHP
            + "MP: " + cMP + "/" + mMP;

        return enemyData;
    }

    // Debug function to check all enemy stats
    public void PrintAllStats()
    {
        Debug.Log("Entity Name: " + entityName);
        Debug.Log("Entity Type: " + entityType);
        Debug.Log("mHP: " + mHP);
        Debug.Log("mMP: " + mMP);
        Debug.Log("cHP: " + cHP);
        Debug.Log("cMP: " + cMP);
        Debug.Log("Attack: " + attack);
        Debug.Log("Defense: " + mMP);
        Debug.Log("Speed: " + cHP);
        Debug.Log("Move 1: " + moves[0].ToString());
        if (moves.Length > 1)
            Debug.Log("Move 2: " + moves[1].ToString());
    }

    // 4. Enumerations
    // Battle Entity types
    public enum EntityType
    {
        Enemy,
        PartyMember,
        Boss
    }

    // All possible turn options
    public enum TurnChoice
    {
        Attack,
        Guard,
        Heal
    }
}


// Turn class that organises all things done in a turn
/*
public class Turn
{
    // Main
    public BattleEntity.TurnChoice turnChoice;

    // Attack variables
    public BattleEntity attacker; // MAY NEED TO REMOVE
    public BattleEntity defender; // in case of sweeping move, each turn will be displayed seperately
    public Move moveUsed;
    public int dmgDone;
    public bool guardBroken;

    // Guard variables
    public int mpRestored;

    // Heal variables
    public int hpHealed;

    public Turn(ref BattleEntity actor, BattleEntity.TurnChoice turnchoice)
    {
        turnChoice = turnchoice;
        attacker = actor;
    }

    public override string ToString()
    {
        string turnInfo = "";

        switch(turnChoice)
        {
            case BattleEntity.TurnChoice.Attack: 
                turnInfo = attacker.entityName + "used " + moveUsed.name 
                    + " on " + defender.entityName + "and dealt " + dmgDone + "points of damage"; 
                break;

            case BattleEntity.TurnChoice.Guard:
                turnInfo = attacker.entityName + "put up its guard and replenished " + mpRestored + "points of MP";
                break;

            case BattleEntity.TurnChoice.Heal:
                turnInfo = attacker.entityName + "healed itself and restored " + hpHealed + "points of HP";
                break;
        }
        return turnInfo;
    }
}
*/

// Move struct that stores organises data for a battle move


public class Move
{
    public string name;
    public MoveType type; // targetted/sweeping, maybe i should use a boolean?
    public int power;
    public int mpCost;

    // Constructor
    public Move(string name, string movetype, int power, int mpCost)
    {
        this.name = name;
        Enum.TryParse(movetype, out MoveType type); // parse string as enum
        this.power = power;
        this.mpCost = mpCost;
    }

    // Override existing to string function
    public override string ToString()
    {
        string moveData = "Move Name: " + name + " Type: " + type 
            + " Power: " + power + " MP Cost: " + mpCost;

        return moveData;
    }

    public enum MoveType
    {
        Target,
        Sweeping
    }
}
