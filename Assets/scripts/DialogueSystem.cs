using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class DialogueSystem : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(3, 5)] public string text;
        public bool isPlayerLine;
    }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Text speakerNameText;
    public Text dialogueText;
    public GameObject continuePrompt;

    [Header("Settings")]
    public List<DialogueLine> dialogueLines;
    public float textSpeed = 0.05f;
    public Color playerNameColor = new Color(0.2f, 0.6f, 1f);
    public Color npcNameColor = Color.white;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

        [Header("NPC Settings")]
    public Transform npcTransform; // Ссылка на трансформ NPC
    public float npcRotationSpeed = 120f; // Скорость поворота NPC

    private Transform playerTransform;
    private bool shouldNPCLookAtPlayer = false;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        dialoguePanel.SetActive(false);
        continuePrompt.SetActive(false);
    }

     void Update()
    {
        if (shouldNPCLookAtPlayer && npcTransform != null)
        {
            RotateNPCTowardsPlayer();
        }
        if (dialoguePanel.activeSelf && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return)))
        {
            NextLine();
        }
    }

    public void StartDialogue(List<DialogueLine> lines)
    {
        dialogueLines = lines;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        if (currentLineIndex >= dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = dialogueLines[currentLineIndex];

        // Настройка UI в зависимости от говорящего
        if (currentLine.isPlayerLine)
        {
            speakerNameText.text = "Игрок";
            speakerNameText.color = playerNameColor;
        }
        else
        {
            speakerNameText.text = currentLine.speakerName;
            speakerNameText.color = npcNameColor;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(currentLine.text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        continuePrompt.SetActive(false);
        dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        continuePrompt.SetActive(true);
    }

    public void NextLine()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = dialogueLines[currentLineIndex].text;
            isTyping = false;
            continuePrompt.SetActive(true);
            return;
        }

        currentLineIndex++;
        DisplayCurrentLine();
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        shouldNPCLookAtPlayer = true;
    }

    private void RotateNPCTowardsPlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - npcTransform.position;
        directionToPlayer.y = 0; // Игнорируем разницу по высоте

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            npcTransform.rotation = Quaternion.RotateTowards(
                npcTransform.rotation,
                targetRotation,
                npcRotationSpeed * Time.deltaTime);

            // Если NPC уже смотрит на игрока, отключаем дальнейший поворот
            if (Quaternion.Angle(npcTransform.rotation, targetRotation) < 1f)
            {
                shouldNPCLookAtPlayer = false;
            }
        }
    }

    public void SetupShipCrashDialogue()
    {
        dialogueLines = new List<DialogueLine>()
        {
            new DialogueLine() {
                isPlayerLine = true,
                text = "Где... я? Голова раскалывается... Последнее, что помню — метеоритный дождь... Системы корабля повреждены... Надо... Надо проверить кислород..."
            },
            new DialogueLine() {
                speakerName = "Х-317",
                text = "Человек! Ты жив! Твой корабль упал на нашу планету. Я — Х-317. Можешь называть меня \"Икс\". Ты в опасности — кислород на исходе. Дай я помогу.",
                isPlayerLine = false
            },
            new DialogueLine() {
                isPlayerLine = true,
                text = "Икс…? Ладно…Что…Что мне делать? Мою память будто отшибло"
            },
            new DialogueLine() {
                speakerName = "Х-317",
                text = "Сначала тебе нужно починить электричество, оно было повреждено.",
                isPlayerLine = false
            }
        };
    }
    public bool IsDialogueActive()
    {
        return dialoguePanel.activeSelf;
    }

}