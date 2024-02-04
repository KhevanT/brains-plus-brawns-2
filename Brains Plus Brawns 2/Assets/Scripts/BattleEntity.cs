using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// BattleEntity class, parent class for BattleEnemy and PartyMember
// Manages health, basic stats, moves, etc for all entities involved in combat
public class BattleEntity
{
    // 1. Stats

    // Identifier
    protected string entityType;
    protected string entityName;

    // DOA status
    protected bool alive = true;

    // Basic stats (differ per class)
    protected int mHP;
    protected int mMP;
    protected int attack;
    protected int defense;
    protected int speed;

    // Current stats
    protected int cHP;
    protected int cMP;

    // Other stats
    protected bool guarding;
    protected float guardingMultiplier = 0.5f;
    protected int noOfTurns = 1; // 2 for boss, 1 for everyone else


    // 2. Functions

    // Remaining functions below will be implemented once a basic battle menu and output system is implemented

    // chooseMove function, called on entity turn, calls function based on user input

    // Attack Function, uses MP, calculates dmg and asks enemy to call take damage
    //      different for enemy and player because enemy will choose randomly and player should get prompt
    //      may require override in children to implement this difference

    // Guard function, turns on guard, replenishes MP

    // Heal, replenishes HP, uses potions (needs inventory access)

    // isAlive, Checks if battle entity is alive
    protected void isAlive()
    {
        if (cHP > 0)
        {
            alive = true;
        }
        else
        {
            cHP = 0; // not let current hp go below 0
            alive = false;
        }
    }

    // calculateDMG, calculates damage to be dealt to an individual defender
    // (must be called once per alive defender even in case of sweeping move)
    protected int calculateDMG(BattleEntity defender, Move move)
    {
        int dmg = 0;
        float randomFac = Random.value + 0.5f; // rand val b/w 0.5 and 1.5

        // < NON FINAL DAMAGE FORMULA >
        dmg = (int)((this.attack * move.power * randomFac) / defender.defense);

        // checks if guard is on, reduces damage by guard defense multiplier val
        if(guarding)
            dmg = (int)(dmg * guardingMultiplier);

        return dmg;
    }

    // Applies damage taken and checks if dead
    protected void takeDamage (int dmg)
    {
        if (alive)
        {
            cHP -= dmg;
            isAlive();
        }
    }
}

// Move class that stores organises data for a battle move
public class Move
{
    public string name;
    public string type; // targetted/sweeping, maybe i should use a boolean?
    public int power;
    public int mpCost;

    // Constructor
    public Move(string name, string type, int power, int mpCost)
    {
        this.name = name;
        this.type = type;
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
}
