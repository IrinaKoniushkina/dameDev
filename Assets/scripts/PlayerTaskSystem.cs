using UnityEngine;
using UnityEngine.UI;

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


    [Header("References")]
    public PlayerController playerController;
    public Animator playerAnimator;

    private int currentTaskIndex = 0;
    private bool isInteracting;
    private bool hasScheme;
    private bool isPuzzleActive;
    private AudioSource audioSource;

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

        UpdateTaskUI();
    }

    void Update()
    {
        if (isInteracting || isPuzzleActive) return;

        CheckTaskCompletion();
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
    if (puzzleManager == null)
    {
        Debug.LogError("PuzzleManager не назначен!");
        return;
    }
        if (playerController == null || gameElectricPanel == null || puzzleManager == null)
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
        CompletePuzzle();
    }

    private void CompletePuzzle()
    {
        puzzleManager.OnPuzzleCompleted.RemoveListener(HandlePuzzleComplete);
        gameElectricPanel.SetActive(false);
        
        playerController.EnableControls();
        isPuzzleActive = false;
        
        CompleteCurrentTask();
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

    private void PlayEffect(ParticleSystem effect, AudioClip sound, Vector3 position)
    {
        if (effect != null)
        {
            Instantiate(effect, position, Quaternion.identity);
        }

        if (sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sound);
        }
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