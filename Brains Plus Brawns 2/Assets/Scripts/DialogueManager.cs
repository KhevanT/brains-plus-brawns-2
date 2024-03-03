using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        {Kingdom.GICT, "Hephaestus" },
        {Kingdom.AMSOM, "Jeff Bezos" },
        {Kingdom.SAS, "Marie Curie" },
    };


    private void SetChildren(bool activeStat)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(activeStat);
        }
    }

    public IEnumerator StartDialogue(DialogueTrigger trigger, List<DialogueLine> dialogue)
    {
        currentTrigger = trigger;

        isDialogueActive = true;

        SetChildren(true);              // Makes the UI visible when dialogue starts

        foreach (DialogueLine line in dialogue)
        {
            currentLine = line;
            if(line.isTriggerToGate)
            {
                // Enable password child, then make it child of canvas to make it visible
                GameObject passwordObject = trigger.GetPasswordObject();
                Debug.Log(passwordObject);
                passwordObject.SetActive(true);
                passwordObject.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, true);
                Debug.Log("Asking for GICT password");

                // Take input from field, check if its correct
                while (!currentTrigger.isPasswordCorrect)
                {
                    yield return null;
                }

                // Allow players to exit dialogue if they don't want to enter this kingdom rn
                // EndDialogue();

                // Disable passsword after done
                passwordObject.SetActive(false);

            }

            lines.Enqueue(line);
        }

        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = lines.Dequeue();

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
            Debug.Log("Loading next kingdom");
        }
    }

    // Reads password and verifies, for entry into kingdom using gate
    public void ReadPassword()
    {
        // Access npc's kingdom (temp)
        Kingdom kingdom = currentTrigger.locationKingdom;
        // Text input = inputField.GetComponent<Text>(); // THIS DOES NOT WORK RN
        string input = "BLAH";

        if(input == passwordList[kingdom])
        {
            Debug.Log("Password is correct");
            currentTrigger.isPasswordCorrect = true;
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
        if (Input.GetKeyDown(KeyCode.Space) && !currentLine.isTriggerToGate)
        {
            DisplayNextDialogueLine();
        }
    }
}
