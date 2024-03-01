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
    public Kingdom locationKingdom;
    public bool isGateKeeper = false;
    public bool isPasswordCorrect = false;
    [SerializeField] public List<DialogueLine> dialogue;

    public void TriggerDialogue()
    {
        StartCoroutine(GameObject.FindGameObjectWithTag("Dialogue Manager").GetComponent<DialogueManager>().StartDialogue(this, dialogue));
    }

    public GameObject GetPasswordObject()
    {
        if(isGateKeeper)
            return this.transform.GetChild(2).gameObject;
        else
            return null;
    }

    void Start()
    {
        if(isGateKeeper)
            GetPasswordObject().SetActive(false);
    }
}
