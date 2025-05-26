using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    // Этот метод будет вызван при нажатии на кнопку
    public void OnPlayButtonClicked()
    {
        // Загружаем следующую сцену по индексу
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
    }
}