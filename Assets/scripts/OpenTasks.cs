using UnityEngine;
using UnityEngine.UI;

public class TaskSystem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject tasksPanel;
    [SerializeField] private Button tasksButton;
    [SerializeField] private GameObject newTaskNotification;
    [SerializeField] private Text[] taskTexts; // Массив из 3 текстовых элементов задач

    [Header("Task Settings")]
    [SerializeField] private Transform electricBox; // Объект электрощитка
    [SerializeField] private Transform electricScheme; // Объект схемы электрощитка
    [SerializeField] private Transform player; // Ссылка на игрока
    [SerializeField] private float activationDistance = 70; // Дистанция активации

    private int currentTask = 0; // Текущая активная задача (0 - первая)
    private Color completedColor = new Color(0.39f, 0.39f, 0.39f); // #636363 в RGB
    private Color activeColor = Color.black;

    private void Start()
    {
        tasksButton.onClick.AddListener(ToggleTasksPanel);
        UpdateTaskVisuals();
        
        // Автоматически находим игрока, если не назначен вручную
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Проверка расстояния до электрощитка (для первой задачи)
        if (currentTask == 0 && Vector3.Distance(player.position, electricBox.position) < activationDistance)
        {
            CompleteCurrentTask();
        }
        // Проверка расстояния до схемы электрощитка (для второй задачи)
        else if (currentTask == 1 && Vector3.Distance(player.position, electricScheme.position) < activationDistance)
        {
            CompleteCurrentTask();
        }
    }

    private void CompleteCurrentTask()
    {
        // Помечаем текущую задачу как выполненную
        taskTexts[currentTask].color = completedColor;
        
        // Переходим к следующей задаче
        currentTask++;
        
        // Обновляем визуал
        UpdateTaskVisuals();
        
        // Показываем уведомление
        newTaskNotification.SetActive(true);
    }

    private void UpdateTaskVisuals()
    {
        // Обновляем цвета всех задач
        for (int i = 0; i < taskTexts.Length; i++)
        {
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
        tasksPanel.SetActive(!tasksPanel.activeSelf);
        if (tasksPanel.activeSelf)
        {
            newTaskNotification.SetActive(false);
        }
    }
}