using UnityEngine;

public class NPC_DialogueTrigger : MonoBehaviour
{
    // В другом скрипте (например, при триггере или начале сцены)
    public DialogueSystem dialogueSystem;

    void Start()
    {
        dialogueSystem.SetupShipCrashDialogue();
        dialogueSystem.StartDialogue(dialogueSystem.dialogueLines);
    }
}
