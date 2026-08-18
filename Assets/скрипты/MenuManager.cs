using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Список всех панелей")]
    public GameObject[] panels;

<<<<<<< Updated upstream
    // Открыть панель по индексу и ГАРАНТИРОВАННО закрыть все остальные
=======
    // Переменные для запоминания выбора
    private string selectedMode;

    // 1. Стандартный метод (оставь для простых переключений)
>>>>>>> Stashed changes
    public void OpenPanel(int panelIndex)
    {
        for (int i = 0; i < panels.Length; i++)
        {
<<<<<<< Updated upstream
            if (i == panelIndex)
            {
                panels[i].SetActive(true); // Включаем нужную
            }
            else
            {
                // Принудительно выключаем все остальные
                // Это сразу остановит их AudioSource
                panels[i].SetActive(false);
            }
        }
    }

    public void CloseAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
=======
            panels[i].SetActive(i == panelIndex);
        }
    }

    // 2. СПЕЦИАЛЬНЫЙ МЕТОД ДЛЯ РАЗВИЛКИ (Выбор режима)
    public void ChooseMainMode(string mode)
    {
        selectedMode = mode; // Запоминаем "1vs1", "2vs2" или "Campaign"

        if (mode == "Campaign")
        {
            // Если кампания, открываем сразу панель выбора героя
            // Допустим, она под индексом 5
            OpenPanel(5);
        }
        else if (mode == "1vs1")
        {
            // Если 1 на 1, открываем панель выбора (Бот/Друг/Рандом) для 1на1
            // Допустим, это индекс 1
            OpenPanel(1);
        }
        else if (mode == "2vs2")
        {
            // Если 2 на 2, открываем панель выбора для 2на2
            // Допустим, это индекс 2
            OpenPanel(2);
        }
    }

    // 3. МЕТОД ДЛЯ ВЫБОРА ПРОТИВНИКА
    public void ChooseOpponent(string opponentType)
    {
        // Здесь можно просто переключить на выбор персонажа (индекс 3)
        // Но скрипт уже "помнит", какой режим был выбран в selectedMode
        OpenPanel(3);
>>>>>>> Stashed changes
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
<<<<<<< Updated upstream
}

=======
}
>>>>>>> Stashed changes
