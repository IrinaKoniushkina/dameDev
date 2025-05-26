using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Elements")]
    public Image[] indicators;
    public Button[] buttons;
    public Sprite greenLight;
    public Sprite redLight;
    public bool[] correctCombination = { true, false, false };
    public Text statusText;

    public UnityEvent OnPuzzleCompleted = new UnityEvent();

    private bool[] currentState;

    public void InitializePuzzle()
    {
        // Проверки
        if (indicators == null || buttons == null || correctCombination == null)
        {
            Debug.LogError("Не назначены необходимые элементы!");
            return;
        }

        if (indicators.Length != correctCombination.Length || buttons.Length != correctCombination.Length)
        {
            Debug.LogError("Количество кнопок/индикаторов не совпадает с комбинацией!");
            return;
        }

        // Инициализация
        currentState = new bool[correctCombination.Length];
        for (int i = 0; i < currentState.Length; i++)
        {
            currentState[i] = false;
        }

        // Подписка на кнопки
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => ToggleButton(index));
        }

        UpdateIndicators();
        
        if (statusText != null)
        {
            statusText.text = "Почините проводку!";
        }
    }

    public void ToggleButton(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= currentState.Length) return;

        currentState[buttonIndex] = !currentState[buttonIndex];
        UpdateIndicators();
        CheckSolution();
    }

    private void UpdateIndicators()
    {
        if (indicators == null || greenLight == null || redLight == null) return;

        for (int i = 0; i < indicators.Length; i++)
        {
            if (indicators[i] != null)
            {
                indicators[i].sprite = currentState[i] ? greenLight : redLight;
            }
        }
    }

    private void CheckSolution()
    {
        for (int i = 0; i < correctCombination.Length; i++)
        {
            if (currentState[i] != correctCombination[i])
            {
                return;
            }
        }

        // Пазл решен
        if (statusText != null)
        {
            statusText.text = "Электричество починено!";
        }

        OnPuzzleCompleted.Invoke();
    }
}