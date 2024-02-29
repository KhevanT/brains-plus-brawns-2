using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public bool isTriggerToGate = false;
    public string name;
    [TextArea(3, 10)]
    public string line;
}

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] public List<DialogueLine> dialogue;

    public void TriggerDialogue()
    {
        GameObject.FindGameObjectWithTag("Dialogue Manager").GetComponent<DialogueManager>().StartDialogue(dialogue);
    }
}
