using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHandler : MonoBehaviour
{
    [SerializeField] private int levelWidth, levelHeight, levelStep = 1;    // excluding the 2 extra cells for walls
    private GameObject[,] grid;
    private Vector2 playerPos;

    private void Start()
    {
        grid = new GameObject[levelHeight + 2, levelWidth + 2];             // the extra 2 are for walls
        LoadLevel();
    }

    private void LoadLevel()
    {
        GameObject level = GameObject.FindGameObjectWithTag("LevelHandler");// get the level object

        for (int childIndex = 0; childIndex < level.transform.childCount; ++childIndex)
        {
            Transform child = level.transform.GetChild(childIndex);         // iterate all children and get their tags

            // basic 5 tags found inside level: player, background, obstacle, npc, door
            if (child.tag == "Player")
            {
                // set player position x and y

                // add GameObject to grid
            }

            if (child.tag == "Background" ||
                child.tag == "Obstacle" ||
                child.tag == "NPC" ||
                child.tag == "Door")
            {
                // add GameObject to grid
            }
        }
    }
}
