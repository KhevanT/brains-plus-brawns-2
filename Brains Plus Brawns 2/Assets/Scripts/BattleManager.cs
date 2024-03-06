using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BattleEntity;
using static Unity.Burst.Intrinsics.X86.Avx;
using UnityEngine.UIElements;
using System;

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
    BattleEntity currEntity;
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
    int mvC;
    int currSelection;
    TurnChoice currChoice;
    List<int> validInputs;
    enum ButtonMenu
    {
        Main,
        Fight,
        Players,
        Heal
    } ButtonMenu currMode;

    enum MainMenu
    {
        Fight,
        Guard,
        Heal,
        Escape
    }

    enum FightMenu
    {
        Targeted = 2,
        Sweeping = 3
    }

    enum HealMenu
    {
        HP = 2,
        MP = 3
    }

    public TMP_Text turnIndicator;
    public TMP_Text selector;
    public TMP_Text opt1;
    public TMP_Text opt2;
    public TMP_Text opt3;
    public TMP_Text opt4;
    private bool playerIsSelecting = true;
    List<UnityEngine.UI.Button> turnButtons = new List<UnityEngine.UI.Button>();

    // Start is called before the first frame update
    void Start()
    {
        // Set Button texts and initiialisations based on which screen we are in
        SetMenu(ButtonMenu.Main);

        // Spawn a scene with x entities
        // initialiseBattle(EnemyType.Dwarf, 4); // only minions
        initialiseBattle(BossName.Hephaestus); // only boss
        allEntities.AddRange(playerPartyMemberComponents);
        allEntities.AddRange(enemyPartyComponents);
        entityCount = allEntities.Count;

        // Disable turn choice buttons
        //turnButtons.Add(attackButton);
        //turnButtons.Add(guardButton);
        //turnButtons.Add(healButton);

        foreach(UnityEngine.UI.Button button in turnButtons)
        {
            button.interactable = false;
        }


        // Get battle order & display on screen
        battleOrder();
        battleOrderTextObject.SetText(battleOrderString);

        // Start combat
        StartCoroutine(turnManager());
    }

    // Update is called once per frame
    void Update()
    {

        HandleKeyboardInput();
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

    private void SetMenu(ButtonMenu Mode)
    {
        currMode = Mode;

        if (Mode == ButtonMenu.Main)
        {
            validInputs = new List<int> { 0, 1, 2, 3 };
            selector.alignment = TextAlignmentOptions.TopLeft;
            currSelection = 0;

            opt1.text = "Fight";
            opt2.text = "Heal";
            opt3.text = "Guard";
            opt4.text = "Run Away";

        } else if (Mode == ButtonMenu.Players)
        {
            selector.alignment = TextAlignmentOptions.TopLeft;
            if (!currEntity.alive)
            {
                // pass because entity isn't alive
                return;
            }

            int totalOppSize = 0;
            TMP_Text[] texts = new TMP_Text[] { opt1, opt2, opt3, opt4 };
            validInputs = new List<int> {};

            if (currEntity.entityType == EntityType.PartyMember)
            {
                for (int i = 0; i < enemyPartyComponents.Count; ++i)
                {
                    if (enemyPartyComponents[i].alive)
                    {
                        validInputs.Add(totalOppSize);
                        texts[totalOppSize++].text = enemyPartyComponents[i].entityName;
                    }
                    else
                        texts[totalOppSize++].text = "";
                }

            } else if (currEntity.entityType == EntityType.Boss)
            {
                for (int i = 0; i < playerPartyMemberComponents.Count; ++i)
                {
                    if (playerPartyMemberComponents[i].alive)
                    {
                        validInputs.Add(totalOppSize);
                        texts[totalOppSize++].text = playerPartyMemberComponents[i].entityName;
                    }
                    else
                        texts[totalOppSize++].text = "";
                }
            }

        } else
        {
            validInputs = new List<int> { 2, 3 };
            selector.alignment = TextAlignmentOptions.TopRight;
            currSelection = 2;

            opt1.text = "";
            opt2.text = "";

            if (Mode == ButtonMenu.Fight)
            {

                opt3.text = "Targeted";
                opt4.text = "Sweeping";

            } else if (Mode == ButtonMenu.Heal)
            {

                opt3.text = "Heal HP";
                opt4.text = "Heal MP";

            }
        }
    }

    private void HandleKeyboardInput()
    {

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            ChangeMenu();

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currSelection += 2;                         // right arrow is equivalent to incrementing the selection int by 2
            if (IsValid(currSelection))
                UpdateSelector();
            else
                currSelection -= 2;                     // undo if not valid
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currSelection -= 2;                         // left arrow is equivalent to decrementing the selection int by 2
            if (IsValid(currSelection))
                UpdateSelector();
            else
                currSelection += 2;                     // undo if not valid
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currSelection += 1;                         // down arrow is equivalent to incrementing the selection int by 1
            if (IsValid(currSelection))
                UpdateSelector();
            else
                currSelection -= 1;                     // undo if not valid
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currSelection -= 1;                         // up arrow is equivalent to decrementing the selection int by 1
            if (IsValid(currSelection))
                UpdateSelector();
            else
                currSelection += 1;                     // undo if not valid
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
            ChangeMenu(esc_flg: true);

    }

    private bool IsValid(int ButtonIndex)
    {
        if (validInputs.Contains(ButtonIndex))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateSelector()
    {
        if (currSelection == 0)
            selector.alignment = TextAlignmentOptions.TopLeft;
        else if (currSelection == 1)
            selector.alignment = TextAlignmentOptions.BottomLeft;
        else if (currSelection == 2)
            selector.alignment = TextAlignmentOptions.TopRight;
        else if (currSelection == 3)
            selector.alignment = TextAlignmentOptions.BottomRight;
    }

    private void ChangeMenu(bool esc_flg = false)
    {
        if (esc_flg)
            SetMenu(ButtonMenu.Main);
        else if (currMode == ButtonMenu.Main)
        {
            if (currSelection == 0)
                SetMenu(ButtonMenu.Fight);
            else if (currSelection == 1)
                SetMenu(ButtonMenu.Heal);
            else if (currSelection == 2)
            {
                /* SKIP, NO OPTIONS */
                currChoice = TurnChoice.Guard;
                /* Guard and end attack */
            }
            else if (currSelection == 3)
            {
                /* [TODO] EXIT BATTLE SCREEN */
                /* [TODO] CHANGE SCENE BACK TO THE PREVIOUS SCENE */
            }
        }
        else if (currMode  == ButtonMenu.Fight)
        {
            currChoice = TurnChoice.Attack;
            if (currSelection == 2)
            {
                SetMenu(ButtonMenu.Players);
            }
            else if (currSelection == 3)
            {
                mvC = 0;
                playerIsSelecting = false;
            }
        }
        else if (currMode == ButtonMenu.Heal)
        {
            currChoice = TurnChoice.Heal;
            if (currSelection == 2)
            {
                // Heal HP and end turn
                playerIsSelecting = false;
            }
            else if (currSelection == 3)
            {
                // Heal MP and end turn
                playerIsSelecting = false;
            }
        }
        else if (currMode == ButtonMenu.Players)
        {
            mvC = 1;
            playerIsSelecting = false;
        }
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
    IEnumerator turnManager()
    {
        // Entity management
        bool partyDead = false;
        bool enemyDead = false;

        // Turn management
        int currentTurn = 0; // index of entity whose turn it is, 0 to (entityCount - 1)
        int curRound = 1;

        Debug.Log("Combat has begun!");
        Debug.Log("Current Round: " + curRound);
        while(!partyDead && !enemyDead)
        {
            yield return new WaitForSecondsRealtime(2);
            currEntity = allEntities[currentTurn];
            if (currEntity.entityType == EntityType.PartyMember)
                playerIsSelecting = true;
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
            yield return StartCoroutine(executeMoveChoice());
            

            // Manage entitities with multiple turns
            currEntity.turnsLeft--;
            if (currEntity.turnsLeft > 0)
            {
                turnIndicator.SetText(currEntity.entityName + "'s Second Turn: ");

                yield return StartCoroutine(executeMoveChoice());
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

    private IEnumerator WaitForPlayerInput()
    {
        Debug.Log(playerIsSelecting + " " + currEntity.alive);
        while (playerIsSelecting && currEntity.alive)
        {
            yield return null;
        }
        Debug.Log(playerIsSelecting + " " + currEntity.alive);
    }

    private IEnumerator executeMoveChoice()
    {
        if (currEntity.entityType == EntityType.PartyMember)
            yield return StartCoroutine(WaitForPlayerInput());      // plain wrapper around an empty while loop to make sure the variables used below don't contain dummy/old values

        if (currChoice == TurnChoice.Attack)                                                    // [TODO] Update iski value
        {
            if (currEntity.entityType == EntityType.Enemy || currEntity.entityType == EntityType.Boss) // enemies
            {
                currEntity.turnHandler(TurnChoice.Attack, ref playerPartyMemberComponents);
                ChangeMenu(esc_flg: true);
            }
            else
            {
                currEntity.turnHandler(TurnChoice.Attack, ref enemyPartyComponents, mvC, currSelection);
            }

        } else if (currChoice == TurnChoice.Heal)
        {
            currEntity.turnHandler(TurnChoice.Heal, ref enemyPartyComponents);

        } else if (currChoice == TurnChoice.Guard)
        {
            currEntity.turnHandler(TurnChoice.Guard, ref enemyPartyComponents);
        }
    }

    // Calls functions based on turn choice for given entity
    // getTurnChoice
    // Called by playable entity to display buttons and get turn choice

    //BattleEntity.TurnChoice checkForButtonClick()
    //{
    //    if (attackClicked)
    //        return BattleEntity.TurnChoice.Attack;
    //    else if (guardClicked)
    //        return BattleEntity.TurnChoice.Guard;
    //    else if (healClicked)
    //        return BattleEntity.TurnChoice.Heal;
    //}

    // reviveTheFallen, Revives dead party members to x% of their mHP and mMP
    void reviveTheFallen(PartyMember deadPartyMember)
    {
        deadPartyMember.alive = true;
        deadPartyMember.cHP = (int) reviveHPPercent * deadPartyMember.mHP;
        deadPartyMember.cMP = (int)reviveMPPercent * deadPartyMember.mMP;
    }
}
