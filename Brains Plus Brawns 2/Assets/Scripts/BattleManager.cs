using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BattleEntity;
using static Unity.Burst.Intrinsics.X86.Avx;
using UnityEngine.UIElements;

public class BattleManager : MonoBehaviour
{
    // Entity management
    public GameObject playerPartyEmpty;
    public GameObject enemyPartyEmpty;
    public GameObject emptyPlayerPrefab;
    public GameObject emptyMinionPrefab;
    public GameObject emptyBossPrefab;

    // Entity management
    int entityCount;
    List<BattleEntity> allEntities = new List<BattleEntity>();
    List<BattleEntity> playerPartyMemberComponents  = new List<BattleEntity>();
    List<BattleEntity> enemyPartyComponents = new List<BattleEntity>();

    // Player Party Manager
    public string[] playerNames = { "Gandalf", "Arthur", "Robin Hood", "Bruce Lee" }; // to be saved from game start and passed in function
    PlayerClass[] playerClasses = { PlayerClass.Wizard, PlayerClass.Knight, PlayerClass.Archer, PlayerClass.Brawler };

    // Current Party Stats 
    // An array of arrays that stores [cHP, CMP] for Wizard, Knight, Archer, Brawler respectively
    int[] currHPs = { -1, -1, -1, -1 };
    int[] currMPs = { -1, -1, -1, -1 };

    // Game balance stats
    float reviveHPPercent = 0.25f;
    float reviveMPPercent = 0.5f;

    // Text Management
    public TMP_Text battleOrderTextObject;
    string battleOrderString;
    public TMP_Text playerStatsTextObject;
    string playerStatsString = "Party Stats: \n";
    
    // Button Management
    public TMP_Text turnIndicator;
    public UnityEngine.UI.Button attackButton;
    public UnityEngine.UI.Button guardButton;
    public UnityEngine.UI.Button healButton;
    List<UnityEngine.UI.Button> turnButtons = new List<UnityEngine.UI.Button>();
    bool attackClicked = false;
    bool guardClicked = false;
    bool healClicked = false;
    bool checkingForButtons = false;

    // Start is called before the first frame update
    void Start()
    {
        // Spawn a scene with x entities
        initialiseBattle(EnemyType.Dwarf, 4); // only minions
        // initialiseBattle(BossName.Hephaestus); // only boss
        allEntities.AddRange(playerPartyMemberComponents);
        allEntities.AddRange(enemyPartyComponents);
        entityCount = allEntities.Count;

        // Disable turn choice buttons
        turnButtons.Add(attackButton);
        turnButtons.Add(guardButton);
        turnButtons.Add(healButton);

        foreach(UnityEngine.UI.Button button in turnButtons)
        {
            button.interactable = false;
        }


        // Get battle order & display on screen
        battleOrder();
        battleOrderTextObject.SetText(battleOrderString);

        // Start combat
        // turnManager();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(checkingForButtons)
        {
            BattleEntity.TurnChoice turnChoice = checkForButtonClick();

            if(turnChoice != null) // handle current entity's turn here only?
            {
                if (currEntity.entityType == BattleEntity.EntityType.PartyMember) // party members
                    currEntity.turnHandler(turnChoice, ref enemyPartyComponents);
                else if (currEntity.entityType == BattleEntity.EntityType.Boss) // boss
                    currEntity.turnHandler(turnChoice, ref playerPartyMemberComponents);
            }

        }
        */
    }

    // Sets up a basic battle scene with given list of enemies and players
    // Overloaded function for fight only with minions
    // minionCount must be between 1 & 4
    public void initialiseBattle(EnemyType minionType, int minionCount) // Battle With Minions
    {
        // Empty previous lists
        playerPartyMemberComponents = new List<BattleEntity>();
        enemyPartyComponents = new List<BattleEntity>();

        // 1. Player Section
        
        // Initialise a player prefab at given position
        // Access its party member component and intialise it
        // Make it a child of playerPartyEmpty
        for (int i = 0; i < 4; i++)
        {
            GameObject emptyPlayer = Instantiate(emptyPlayerPrefab, new Vector3(4.5f, 3.5f - (1 * i), 0), Quaternion.identity);
            emptyPlayer.transform.parent = playerPartyEmpty.transform;

            PartyMember playerComponent = emptyPlayer.GetComponent<PartyMember>();
            playerComponent.initialiseMember(playerNames[i], playerClasses[i]);
            emptyPlayer.name = playerComponent.baseClass.ToString();

            playerPartyMemberComponents.Add(playerComponent);

            // Set cHP, cMP to previously stored (if -1, it remains at maxHP and maxMP as per initialisation)
            if (currHPs[i] != -1 && currMPs[i] != -1)
            {
                playerComponent.cHP = currHPs[i];
                playerComponent.cMP = currMPs[i];
            }

            // If party member is dead, revive them
            if (playerComponent.cHP <= 0)
                reviveTheFallen(playerComponent);

            // Text management - Player Stats
            playerStatsString += $"{i + 1}. {playerComponent.entityName} " +
                $"\n HP: {playerComponent.cHP}/{playerComponent.mHP}" +
                $"\n MP: {playerComponent.cMP}/{playerComponent.mMP} \n\n";
        }

        // Text management - Player Stats
        playerStatsTextObject.SetText(playerStatsString);

        // 2. Enemy 

        Vector3[] enemyPos = {new Vector3(-4, 3, 0), new Vector3(-4, 1, 0),
                                new Vector3(-1, 3, 0),  new Vector3(-1, 1, 0) };
        for(int i = 0; i < minionCount; i++)
        {
            GameObject emptyMinion = Instantiate(emptyMinionPrefab, enemyPos[i], Quaternion.identity);
            emptyMinion.transform.parent = enemyPartyEmpty.transform;

            BattleEnemy minionComponent = emptyMinion.GetComponent<BattleEnemy>();
            minionComponent.initialiseEnemy(minionType);
            minionComponent.entityName = minionComponent.entityName + " " + (i + 1);
            emptyMinion.name = minionComponent.entityName;

            enemyPartyComponents.Add(minionComponent);
        }
    }

    // Overloaded function to initialise a battle with a boss only
    public void initialiseBattle(BossName bossName)
    {
        // Empty previous lists
        playerPartyMemberComponents = new List<BattleEntity>();
        enemyPartyComponents = new List<BattleEntity>();

        // 1. Player Section

        // Initialise a player prefab at given position
        // Access its party member component and intialise it
        // Make it a child of playerPartyEmpty
        for (int i = 0; i < 4; i++)
        {
            GameObject emptyPlayer = Instantiate(emptyPlayerPrefab, new Vector3(4.5f, 3.5f - (1 * i), 0), Quaternion.identity);
            emptyPlayer.transform.parent = playerPartyEmpty.transform;

            PartyMember playerComponent = emptyPlayer.GetComponent<PartyMember>();
            playerComponent.initialiseMember(playerNames[i], playerClasses[i]);
            emptyPlayer.name = playerComponent.baseClass.ToString();

            playerPartyMemberComponents.Add(playerComponent);

            // Set cHP, cMP to previously stored (if -1, it remains at maxHP and maxMP as per initialisation)
            if (currHPs[i] != -1 && currMPs[i] != -1)
            {
                playerComponent.cHP = currHPs[i];
                playerComponent.cMP = currMPs[i];
            }

            // If party member is dead, revive them
            if (playerComponent.cHP <= 0)
                reviveTheFallen(playerComponent);

            // Text management - Player Stats
            playerStatsString += $"{i + 1}. {playerComponent.entityName} " +
                $"\n HP: {playerComponent.cHP}/{playerComponent.mHP}" +
                $"\n MP: {playerComponent.cMP}/{playerComponent.mMP} \n\n";
        }

        // Text management - Player Stats
        playerStatsTextObject.SetText(playerStatsString);

        // 2. Boss Section (can only be 1 boss and no other enemies)

        GameObject emptyBoss = Instantiate(emptyBossPrefab, new Vector3(-2, 2, 0), Quaternion.identity);
        emptyBoss.transform.parent = enemyPartyEmpty.transform;
        BattleBoss bossComponent = emptyBoss.GetComponent<BattleBoss>();
        bossComponent.initialiseBoss(bossName);
        emptyBoss.name = bossComponent.entityName;
        enemyPartyComponents.Add(bossComponent);
    }

    // battleOrder function, calculates order of given combat encounter 
    // by sorting in descending order of speed
    void battleOrder()
    {
        // Using System.Linq
        // Sort in ascending order with speed as parameter, then reverse it to get descending order
        allEntities = allEntities.OrderBy(BattleEntity => BattleEntity.speed).ToList();
        allEntities.Reverse();

        // Print order & create string for display
        battleOrderString = "Battle Order: \n";
        Debug.Log("The order of turns in combat is: ");
        foreach(BattleEntity entity in allEntities)
        {
            Debug.Log(entity.entityName + ": " +  entity.speed);
            battleOrderString += entity.entityName + "\n";
        }
    }

    // turnManager function, uses sorted entity order to initiate turns for each battle entity
    void turnManager()
    {
        // Entity management
        BattleEntity currEntity;
        bool partyDead = false;
        bool enemyDead = false;

        // Turn management
        int currentTurn = 0; // index of entity whose turn it is, 0 to (entityCount - 1)
        int curRound = 1;

        Debug.Log("Combat has begun!");
        Debug.Log("Current Round: " + curRound);
        while(!partyDead && !enemyDead)
        {
            currEntity = allEntities[currentTurn];
            turnIndicator.SetText(currEntity.entityName + "'s Turn: ");

            /*
            // Calls entity's turn function and pass relevant team info
            if(currEntity.entityType == BattleEntity.EntityType.PartyMember) // party members
            {
                //TurnChoice turnChoice = getTurnChoice(); // TRANSFER CONTROL TO UPDATE() 
                // TurnChoice turnChoice = TurnChoice.Attack;
                enableButtons();

                // currEntity.turnHandler(turnChoice, ref enemyPartyComponents);
            }  
            else if (currEntity.entityType == BattleEntity.EntityType.Boss) // boss
            {
                TurnChoice turnChoice = getTurnChoice();
                // TurnChoice turnChoice = TurnChoice.Attack;
                currEntity.turnHandler(turnChoice, ref playerPartyMemberComponents);
            }
            else if (currEntity.entityType == BattleEntity.EntityType.Enemy) // enemies
            {
                currEntity.turnHandler(TurnChoice.Attack, ref playerPartyMemberComponents);
            }
            */

            if (currEntity.entityType == BattleEntity.EntityType.Enemy) // enemies
            {
                currEntity.turnHandler(TurnChoice.Attack, ref playerPartyMemberComponents);
            }
            else
            {
                enableButtons(); // transfer control to update and wait for it to call entity's turn handler with the input
                // but then how will control come back here?!
            }

            // Reset clicked buttons
            attackClicked = false;
            guardClicked = false;
            healClicked = false;

            // Manage entitities with multiple turns
            currEntity.turnsLeft--;
            if (currEntity.turnsLeft > 0)
            {
                turnIndicator.SetText(currEntity.entityName + "'s Second Turn: ");

                //TurnChoice turnChoice = getTurnChoice();
                //// TurnChoice turnChoice = TurnChoice.Attack;
                //currEntity.turnHandler(turnChoice, ref playerPartyMemberComponents); // bosses only
            }

            // Update playerPartyMemberComponents and enemyPartyComponents (if anyone died) after each turn
            foreach (BattleEntity entity in allEntities)
            {
                if(!entity.alive)
                {
                    // entityCount--; // this is causing missed turns in non dead entities
                    entity.turnsLeft = 0;
                    // Cannot remove from allEntities list because battle order will be screwed maybe

                    // Disable visibility
                    entity.transform.gameObject.GetComponent<Renderer>().enabled = false;
                }
            }

            // Text Management - Player Stats
            playerStatsString = "Party Stats: \n";
            for (int i = 0; i < 4; i++)
            {
                BattleEntity entity = playerPartyMemberComponents[i];
                // Text management - Player Stats
                playerStatsString += $"{i + 1}. {entity.entityName} " +
                    $"\n HP: {entity.cHP}/{entity.mHP}" +
                    $"\n MP: {entity.cMP}/{entity.mMP} \n\n";
            }
            playerStatsTextObject.SetText(playerStatsString);

            // Checks if all members of either team are dead, if so, end combat, exit loop
            partyDead = playerPartyMemberComponents.All(x => !x.alive);
            enemyDead = enemyPartyComponents.All(x => !x.alive);
            
            // Check if either party is fully dead
            if (partyDead)
            {
                Debug.Log("The enemies have won.");
            }
            else if(enemyDead)
            {
                Debug.Log("The players have won.");
            }
            else
            {
                // Keep going with combat

                // Increment turn
                if (currentTurn == entityCount - 1) // if turn x and entity
                {
                    // End of a round
                    Debug.Log("Round " + curRound + " has ended.");
                    currentTurn = 0;
                    curRound++;
                    Debug.Log("Current Round: " + curRound);

                    // Reset turns left for all entities
                    foreach (BattleEntity entity in allEntities)
                    {
                        entity.turnsLeft = entity.noOfTurns;
                    }

                }
                else
                    currentTurn++;
            }
        }

        // Store current party member info (for later battles)
        for (int i = 0; i < 4; i++)
        {
            currHPs[i] = playerPartyMemberComponents[i].cHP;
            currMPs[i] = playerPartyMemberComponents[i].cMP;
        }

        Debug.Log("Combat has ended.");
    }

    // Calls functions based on turn choice for given entity
    // getTurnChoice
    // Called by playable entity to display buttons and get turn choice
    public void enableButtons()
    {
        // Display all buttons
        foreach (UnityEngine.UI.Button button in turnButtons)
        {
            button.interactable = true;
        }

        // Wait for player to click one, the corresponding function to be called
        checkingForButtons = true;

        // Return value based on whichever was clicked first
        /*
        while (global turnChoice variable is null)
        {
            if not null, return turnChoice

            if (attackClicked)
                return BattleEntity.TurnChoice.Attack;
            else if (guardClicked)
                return BattleEntity.TurnChoice.Guard;
            else if (healClicked)
                return BattleEntity.TurnChoice.Heal;
        }
        */
    }

    //BattleEntity.TurnChoice checkForButtonClick()
    //{
    //    if (attackClicked)
    //        return BattleEntity.TurnChoice.Attack;
    //    else if (guardClicked)
    //        return BattleEntity.TurnChoice.Guard;
    //    else if (healClicked)
    //        return BattleEntity.TurnChoice.Heal;
    //}


    // Called when corresponding buttons are clicked
    public void attackButtonClick()
    {
        attackClicked = true;
        Debug.Log("Attack Button was clicked");
    }
    public void guardButtonClick()
    {
        guardClicked = true;
        Debug.Log("Guard Button was clicked");
    }
    public void healButtonClick()
    {
        healClicked = true;
        Debug.Log("Heal Button was clicked");
    }

    // reviveTheFallen, Revives dead party members to x% of their mHP and mMP
    void reviveTheFallen(PartyMember deadPartyMember)
    {
        deadPartyMember.alive = true;
        deadPartyMember.cHP = (int) reviveHPPercent * deadPartyMember.mHP;
        deadPartyMember.cMP = (int)reviveMPPercent * deadPartyMember.mMP;
    }
}
