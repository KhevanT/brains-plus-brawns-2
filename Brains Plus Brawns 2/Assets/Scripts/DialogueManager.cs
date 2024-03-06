using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;

    private Queue<DialogueLine> lines = new Queue<DialogueLine>();
    DialogueTrigger currentTrigger = null;
    DialogueLine currentLine = null;

    public bool isDialogueActive = false;

    public float typingSpeed = 0.2f;

    // Password List
    Dictionary<Kingdom, string> passwordList = new Dictionary<Kingdom, string>
    {
        {Kingdom.GICT, "Greece" },
        {Kingdom.AMSOM, "Jeff Bezos" },
        {Kingdom.SAS, "Marie Curie" },
        {Kingdom.BATTLE, "FIGHT" }
    };
    GameObject passwordObject;

    private void SetChildren(bool activeStat)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(activeStat);
        }
    }

    public void StartDialogue(DialogueTrigger trigger, List<DialogueLine> dialogue)
    {
        currentTrigger = trigger;
        passwordObject = currentTrigger.GetPasswordObject();

        isDialogueActive = true;

        SetChildren(true);              // Makes the UI visible when dialogue starts

        foreach (DialogueLine line in dialogue)
            lines.Enqueue(line);

        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = lines.Dequeue();

        characterName.text = currentLine.name;

        StopAllCoroutines();

        StartCoroutine(TypeSentence(currentLine));
    }

    IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        dialogueArea.text = "";
        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueArea.text += letter;
            yield return new WaitForSeconds(1/typingSpeed);
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        SetChildren(false);              // Makes the UI invisible when dialogue ends

        if(currentTrigger.isGateKeeper) 
        {
            // Load corresponding scene
            if (currentTrigger.locationKingdom == Kingdom.GICT)
                SceneManager.LoadScene("Volcano Kingdom");
        }

        if (currentTrigger.isBattleTrigger)
        {
            SceneManager.LoadScene("BattleScene");
        }
            

        if (currentTrigger.isStaircase)
        {
            // Change player position to target position
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject playerMovePoint = GameObject.FindGameObjectWithTag("Player Move Point");
            
            player.gameObject.transform.position = currentTrigger.nextStaircase.transform.position;
            playerMovePoint.transform.position = currentTrigger.nextStaircase.transform.position;
        }
    }

    // Reads password and verifies, for entry into kingdom using gate
    public void ReadPassword(GameObject inputField)
    {
        // Access npc's kingdom (temp)
        Kingdom kingdom = currentTrigger.locationKingdom;
        string input = inputField.GetComponent<TMP_InputField>().text;
        Debug.Log(input);

        if(input == passwordList[kingdom])
        {
            Debug.Log("Password is correct");
            currentTrigger.isPasswordCorrect = true;
            passwordObject.SetActive(false);
        }
        else
        {
            Debug.Log("Wrong password. Try again");
        }
    }

    private void Start()
    {
        SetChildren(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(currentLine.isGate + ", " +  currentLine.isAfterGate + ", " + currentTrigger.isPasswordCorrect);

            if (currentLine.isGate && currentTrigger.isGateKeeper)
            {
                // Enable password child, then make it child of canvas to make it visible
                passwordObject.SetActive(true);
                passwordObject.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, true);
                ReadPassword(passwordObject);
                Debug.Log("Asking for GICT password");

                // Allow players to exit dialogue if they don't want to enter this kingdom rn
                // EndDialogue();

                // Disable passsword after done
                // passwordObject.SetActive(false);
            }

            if ((!currentLine.isGate && !currentLine.isAfterGate) || currentTrigger.isBattleTrigger)
            {
                DisplayNextDialogueLine();
            }
            else if (currentTrigger.isPasswordCorrect)
            {
                DisplayNextDialogueLine();
            }
        }
    }
}
