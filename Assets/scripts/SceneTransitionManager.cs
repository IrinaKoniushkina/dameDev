using UnityEngine;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("NPC Settings")]
    public Transform npcTransform; // Ссылка на NPC
    public float npcFacePlayerSpeed = 50f;

    [Header("Transition Settings")]
    public Image fadeImage;
    public float fadeDuration = 2f;
    
    [Header("Player Rotation Settings")]
    public GameObject player;
    public float rotationAngle = 45f; // Угол поворота в градусах
    public float rotationSpeed = 90f;
    public float pauseBetweenRotations = 0.6f;
    
    [Header("Dialogue Settings")]
    public DialogueSystem dialogueSystem;
    public PlayerController playerController;

    private enum TransitionState { FadingIn, RotatingRight, Pausing, RotatingLeft, Returning, StartingDialogue, Done }
    private TransitionState currentState;
    private float timer;
    private Quaternion targetRightRotation;
    private Quaternion targetLeftRotation;
    private Quaternion initialRotation;
    private bool dialogueStarted;

    void Start()
    {
        InitializeReferences();
        
        initialRotation = player.transform.rotation;
        currentState = TransitionState.FadingIn;
        fadeImage.color = Color.black;
        fadeImage.gameObject.SetActive(true);
        playerController.DisableControls();
    }

    void InitializeReferences()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (playerController == null && player != null) 
            playerController = player.GetComponent<PlayerController>();
        if (dialogueSystem == null)
            dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    void Update()
    {
        switch (currentState)
        {
            case TransitionState.FadingIn:
                HandleFadingIn();
                break;
                
            case TransitionState.RotatingRight:
                HandleRotation(ref targetRightRotation, TransitionState.Pausing);
                break;
                
            case TransitionState.Pausing:
                HandlePause();
                break;
                
            case TransitionState.RotatingLeft:
                HandleRotation(ref targetLeftRotation, TransitionState.Returning);
                break;
                
            case TransitionState.Returning:
                HandleRotation(ref initialRotation, TransitionState.StartingDialogue);
                break;
                
            case TransitionState.StartingDialogue:
                HandleStartingDialogue();
                break;
        }
    }

     void HandleStartingDialogue()
    {
        if (!dialogueStarted)
        {
            if (dialogueSystem != null)
            {
                // Передаем ссылку на NPC в диалоговую систему
                dialogueSystem.npcTransform = npcTransform;
                dialogueSystem.SetupShipCrashDialogue();
                dialogueSystem.StartDialogue(dialogueSystem.dialogueLines);
                dialogueStarted = true;
            }
        }
        else if (!dialogueSystem.IsDialogueActive())
        {
            // После диалога поворачиваем NPC к игроку
            FaceNPCToPlayer();
            CompleteTransition();
        }
    }

    private void FaceNPCToPlayer()
    {
        if (npcTransform != null && player != null)
        {
            Vector3 directionToPlayer = player.transform.position - npcTransform.position;
            directionToPlayer.y = 0;
            
            if (directionToPlayer != Vector3.zero)
            {
                npcTransform.rotation = Quaternion.LookRotation(directionToPlayer);
            }
        }
    }

    void HandleFadingIn()
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
        fadeImage.color = new Color(0, 0, 0, alpha);

        if (timer >= fadeDuration)
        {
            fadeImage.gameObject.SetActive(false);
            timer = 0f;
            currentState = TransitionState.RotatingRight;
            targetRightRotation = initialRotation * Quaternion.Euler(0, rotationAngle, 0);
            targetLeftRotation = initialRotation * Quaternion.Euler(0, -rotationAngle, 0);
        }
    }

    void HandleRotation(ref Quaternion target, TransitionState nextState)
    {
        player.transform.rotation = Quaternion.RotateTowards(
            player.transform.rotation, 
            target, 
            rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(player.transform.rotation, target) < 0.5f)
        {
            timer = 0f;
            currentState = nextState;
        }
    }

    void HandlePause()
    {
        timer += Time.deltaTime;
        if (timer >= pauseBetweenRotations)
        {
            timer = 0f;
            currentState = TransitionState.RotatingLeft;
        }
    }

    void CompleteTransition()
    {
        playerController.EnableControls();
        currentState = TransitionState.Done;
        this.enabled = false;
    }
}