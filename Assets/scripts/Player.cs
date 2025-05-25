using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour 
{
    // Компоненты
    private Animator anim;
    private CharacterController controller;
    
    // Настройки движения
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;
    public float gravity = 180f;

    // Параметры анимаций
    [Header("Animation Parameters")]
    public string walkParam = "IsWalking";
    public string runParam = "IsRunning";
    public string repairParam = "IsRepairing";
    public string pickupParam = "IsPickingUp";

    // Система задач
    [Header("Task System")]
    public GameObject tasksPanel;
    public Button tasksButton;
    public GameObject newTaskNotification;
    public Text[] taskTexts;
    public Transform electricBox;
    public Transform electricScheme;
    public float activationDistance = 3f;
    public float interactionTime = 2f;

    // Приватные переменные
    private Vector3 moveDirection = Vector3.zero;
    private float currentSpeed;
    private bool isRunning;
    private int currentTask = 0;
    private Color completedColor = new Color(0.39f, 0.39f, 0.39f);
    private Color activeColor = Color.black;
    private float interactionTimer;
    private bool isInteracting;

    void Start() 
    {
        // Инициализация компонентов
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        if (controller == null)
            Debug.LogError("CharacterController not found!");
        if (anim == null)
            Debug.LogError("Animator not found!");

        currentSpeed = walkSpeed;

        // Инициализация системы задач
        if (tasksButton != null)
        {
            tasksButton.onClick.AddListener(ToggleTasksPanel);
        }
        UpdateTaskVisuals();
    }

    void Update()
    {
        if (controller == null || anim == null) return;

        if (!isInteracting)
        {
            HandleMovement();
        }
        else
        {
            // Блокировка движения во время взаимодействия
            moveDirection = Vector3.zero;
            interactionTimer -= Time.deltaTime;
            if (interactionTimer <= 0)
            {
                isInteracting = false;
                anim.SetBool(repairParam, false);
                anim.SetBool(pickupParam, false);
            }
        }

        HandleTasks();
    }

    private void HandleMovement()
    {
        // Получаем ввод с клавиатуры
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Проверяем бег (Shift + движение)
        isRunning = Input.GetKey(KeyCode.LeftShift) && vertical > 0;
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Поворот персонажа
        if (horizontal != 0)
        {
            transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);
        }

        // Движение вперед
        if (vertical > 0)
        {
            moveDirection = transform.forward * vertical * currentSpeed;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        controller.Move(moveDirection * Time.deltaTime);

        // Управление анимациями движения
        bool isMoving = vertical > 0;
        anim.SetBool(walkParam, isMoving);
        anim.SetBool(runParam, isRunning && isMoving);
    }

    private void HandleTasks()
    {
        if (isInteracting) return;

        // Проверка расстояния до электрощитка (для первой задачи)
        if (currentTask == 0 && Vector3.Distance(transform.position, electricBox.position) < activationDistance)
        {
            StartInteraction(true);
        }
        // Проверка расстояния до схемы электрощитка (для второй задачи)
        else if (currentTask == 1 && Vector3.Distance(transform.position, electricScheme.position) < activationDistance)
        {
            StartInteraction(false);
        }
    }

    private void StartInteraction(bool isRepair)
    {
        isInteracting = true;
        interactionTimer = interactionTime;

        if (isRepair)
        {
            anim.SetBool(repairParam, true);
        }
        else
        {
            anim.SetBool(pickupParam, true);
        }

        // Завершаем задачу после анимации
        Invoke("CompleteCurrentTask", interactionTime);
    }

    private void CompleteCurrentTask()
    {
        // Помечаем текущую задачу как выполненную
        if (currentTask < taskTexts.Length)
        {
            taskTexts[currentTask].color = completedColor;
        }
        
        // Переходим к следующей задаче
        currentTask++;
        
        // Обновляем визуал
        UpdateTaskVisuals();
        
        // Показываем уведомление
        if (newTaskNotification != null)
        {
            newTaskNotification.SetActive(true);
        }
    }

    private void UpdateTaskVisuals()
    {
        for (int i = 0; i < taskTexts.Length; i++)
        {
            if (taskTexts[i] == null) continue;

            if (i < currentTask)
            {
                // Выполненные задачи
                taskTexts[i].color = completedColor;
            }
            else if (i == currentTask)
            {
                // Активная задача
                taskTexts[i].color = activeColor;
            }
            else
            {
                // Будущие задачи
                taskTexts[i].color = new Color(0.39f, 0.39f, 0.39f, 0.5f);
            }
        }
    }

    public void ToggleTasksPanel()
    {
        if (tasksPanel != null)
        {
            tasksPanel.SetActive(!tasksPanel.activeSelf);
            if (tasksPanel.activeSelf && newTaskNotification != null)
            {
                newTaskNotification.SetActive(false);
            }
        }
    }
}