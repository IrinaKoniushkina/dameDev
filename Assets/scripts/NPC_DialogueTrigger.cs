using UnityEngine;

public class NPC_DialogueTrigger : MonoBehaviour
{
    public DialogueSystem dialogueSystem;

    void Start()
    {
        dialogueSystem.SetupShipCrashDialogue();
        dialogueSystem.StartDialogue(dialogueSystem.dialogueLines);
    }
}
