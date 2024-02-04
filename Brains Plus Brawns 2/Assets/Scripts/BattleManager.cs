using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Test player
        PartyMember wizard = new PartyMember("Gandalf", "Wizard");
        wizard.PrintAllStats();

        // Test enemy
        BattleEnemy dwarf = new BattleEnemy("Dwarf", "Minion");
        dwarf.PrintAllStats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ActiveEntity manager, finds all alive entities and sorts them in Enemy & Party groups

    // BattleOrder function, calculates order of given combat encounter 
    // by sorting highest speed at first turn and so on
    // uses ActiveEntity list


}
