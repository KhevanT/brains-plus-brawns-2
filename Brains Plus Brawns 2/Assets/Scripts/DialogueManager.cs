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

    public bool isDialogueActive = false;

    public float typingSpeed = 0.2f;

    private void SetChildren(bool activeStat)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(activeStat);
        }
    }

    public void StartDialogue(List<DialogueLine> dialogue)
    {
        isDialogueActive = true;

        SetChildren(true);              // Makes the UI visible when dialogue starts

        Debug.Log(lines.Count);
        foreach (DialogueLine line in dialogue)
        {
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
    }

    private void Start()
    {
        SetChildren(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextDialogueLine();
        }
    }
}
