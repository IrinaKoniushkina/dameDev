using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerController))]
public class PlayerTaskSystem : MonoBehaviour
{
    [Header("Task Settings")]
    public GameObject tasksPanel;
    public Button tasksButton;
    public GameObject newTaskNotification;
    public Text[] taskTexts;
    public Color completedColor = new Color(0.39f, 0.39f, 0.39f);
    public Color activeColor = Color.white;
    public Color pendingColor = new Color(0.39f, 0.39f, 0.39f, 0.5f);

    [Header("Interactable Objects")]
    public Transform electricBox;
    public Transform electricScheme;
    public GameObject schemeObject;
    public float activationDistance = 3f;
    public float interactionTime = 2f;

    [Header("Puzzle Settings")]
    public GameObject gameElectricPanel;
    public ButtonPuzzleManager puzzleManager;
    public float panelHideDelay = 3f;

    [Header("Completion Effects")]
    public Light completionLight;
    public GameObject doorToHide;
    public ParticleSystem completionEffect;

     [Header("Dialogue Settings")]
    public GameObject alienNPC;
    public Transform doorLookTarget;
    public float rotationSpeed = 1f;
    public DialogueSystem dialogueSystem;
    
    [System.Serializable]
    public class NPCDialogue
    {
        public string text;
        public float displayTime = 3f;
    }
    
    public List<NPCDialogue> completionDialogue = new List<NPCDialogue>()
    {
        new NPCDialogue { text = "Поздравляю, ты наладил электропитание!", displayTime = 3f },
        new NPCDialogue { text = "Теперь у нас есть доступ в другие отсеки корабля, но тебе нужно поторопиться...", displayTime = 2.5f },
        new NPCDialogue { text = "Системы жизнеобеспечения повреждены, кислорода в этом секторе становится всё меньше.", displayTime = 3.5f },
        new NPCDialogue { text = "Используй терминал в следующем отсеке, чтобы стабилизировать ситуацию.", displayTime = 4f }
    };

    [Header("References")]
    public PlayerController playerController;
    public Animator playerAnimator;

    private int currentTaskIndex = 0;
    private bool isInteracting;
    private bool hasScheme;
    private bool isPuzzleActive;
    private AudioSource audioSource;
    private float panelHideTimer;
    private bool isCompletingSequence;
    private bool isRotatingToDoor;
    private Quaternion targetRotation;
    private bool hasStartedDialogue;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (tasksButton != null)
        {
            tasksButton.onClick.AddListener(ToggleTasksPanel);
        }

        if (gameElectricPanel != null)
        {
            gameElectricPanel.SetActive(false);
        }

        if (completionLight != null) completionLight.enabled = false;
        if (doorToHide != null) doorToHide.SetActive(true);
        if (alienNPC != null) alienNPC.SetActive(false);

        UpdateTaskUI();
    }

    void Update()
    {
        if (isCompletingSequence)
        {
            HandleCompletionSequence();
            return;
        }

        // Обработка таймера скрытия панели
        if (panelHideTimer > 0)
        {
            panelHideTimer -= Time.deltaTime;
            if (panelHideTimer <= 0)
            {
                HideElectricPanel();
            }
        }

        if (isInteracting || isPuzzleActive) return;

        CheckTaskCompletion();
    }

    private void HandleCompletionSequence()
    {
        if (isRotatingToDoor)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                isRotatingToDoor = false;
                StartDialogue();
            }
        }
    }

    private void StartDialogue()
    {
        if (alienNPC != null)
        {
            alienNPC.SetActive(true);
            Animator npcAnimator = alienNPC.GetComponent<Animator>();
            if (npcAnimator != null)
            {
                npcAnimator.SetTrigger("Appear");
            }
        }

        if (dialogueSystem != null && completionDialogue.Count > 0 && !hasStartedDialogue)
        {
            dialogueSystem.StartDialogue(ConvertDialogueToSystemFormat());
            hasStartedDialogue = true;
        }
    }

    private List<DialogueSystem.DialogueLine> ConvertDialogueToSystemFormat()
    {
        List<DialogueSystem.DialogueLine> converted = new List<DialogueSystem.DialogueLine>();
        foreach (NPCDialogue dialogue in completionDialogue)
        {
            DialogueSystem.DialogueLine line = new DialogueSystem.DialogueLine();
            // Предполагаем, что DialogueLine содержит только текст
            line.text = dialogue.text;
            converted.Add(line);
        }
        return converted;
    }

    private void CheckTaskCompletion()
    {
        switch (currentTaskIndex)
        {
            case 0:
                if (CheckProximity(electricBox.position) && !hasScheme)
                {
                    CompleteCurrentTask();
                }
                break;

            case 1:
                if (CheckProximity(electricScheme.position))
                {
                    StartInteraction("pickup");
                }
                break;

            case 2:
                if (CheckProximity(electricBox.position) && hasScheme)
                {
                    StartElectricPuzzle();
                }
                break;
        }
    }

    private void StartElectricPuzzle()
    {
        if (puzzleManager == null || playerController == null || gameElectricPanel == null)
        {
            Debug.LogError("Missing required puzzle components!");
            return;
        }

        isPuzzleActive = true;
        playerController.DisableControls();
        
        puzzleManager.InitializePuzzle();
        gameElectricPanel.SetActive(true);
        
        puzzleManager.OnPuzzleCompleted.AddListener(HandlePuzzleComplete);
    }

    private void HandlePuzzleComplete()
    {
        puzzleManager.OnPuzzleCompleted.RemoveListener(HandlePuzzleComplete);
        
        playerController.EnableControls();
        isPuzzleActive = false;
        
        CompleteCurrentTask();
        
        panelHideTimer = panelHideDelay;
    }

    private void HideElectricPanel()
    {
        if (gameElectricPanel != null)
        {
            gameElectricPanel.SetActive(false);
        }
        
        ActivateCompletionEffects();
        StartCompletionSequence();
    }

    private void ActivateCompletionEffects()
    {
        if (completionLight != null) completionLight.enabled = true;
        if (doorToHide != null) doorToHide.SetActive(false);
        if (completionEffect != null) completionEffect.Play();
    }

    private void StartCompletionSequence()
    {
        isCompletingSequence = true;
        playerController.enabled = false;
        
        Vector3 directionToDoor = doorLookTarget.position - transform.position;
        directionToDoor.y = 0;
        targetRotation = Quaternion.LookRotation(directionToDoor);
        isRotatingToDoor = true;
        hasStartedDialogue = false;
    }

    private bool CheckProximity(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) < activationDistance;
    }

    private void StartInteraction(string interactionType)
    {
        isInteracting = true;
        playerController.DisableControls();
        playerAnimator.SetBool(interactionType == "repair" ? "IsRepairing" : "IsPickingUp", true);

        Invoke("CompleteInteraction", interactionTime);
    }

    private void CompleteInteraction()
    {
        playerAnimator.SetBool("IsRepairing", false);
        playerAnimator.SetBool("IsPickingUp", false);
        playerController.EnableControls();
        isInteracting = false;

        if (currentTaskIndex == 1)
        {
            hasScheme = true;
            if (schemeObject != null)
            {
                Destroy(schemeObject);
            }
        }

        CompleteCurrentTask();
    }

    private void CompleteCurrentTask()
    {
        if (currentTaskIndex < taskTexts.Length)
        {
            taskTexts[currentTaskIndex].color = completedColor;
            currentTaskIndex++;
            UpdateTaskUI();
            ShowNewTaskNotification();
        }
    }

    private void UpdateTaskUI()
    {
        for (int i = 0; i < taskTexts.Length; i++)
        {
            if (taskTexts[i] == null) continue;

            if (i < currentTaskIndex)
            {
                taskTexts[i].color = completedColor;
            }
            else if (i == currentTaskIndex)
            {
                taskTexts[i].color = activeColor;
            }
            else
            {
                taskTexts[i].color = pendingColor;
            }
        }
    }

    private void ShowNewTaskNotification()
    {
        if (newTaskNotification != null && currentTaskIndex < taskTexts.Length)
        {
            newTaskNotification.SetActive(true);
        }
    }

    public void ToggleTasksPanel()
    {
        if (tasksPanel != null)
        {
            bool isActive = !tasksPanel.activeSelf;
            tasksPanel.SetActive(isActive);

            if (isActive && newTaskNotification != null)
            {
                newTaskNotification.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        if (puzzleManager != null)
        {
            puzzleManager.OnPuzzleCompleted.RemoveListener(HandlePuzzleComplete);
        }
    }
}