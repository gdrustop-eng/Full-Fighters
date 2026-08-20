using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Список всех панелей")]
    public GameObject[] panels;

    // Переменные для запоминания выбора
    private string selectedMode;

    // 1. Стандартный метод для переключения панелей
    public void OpenPanel(int panelIndex)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == panelIndex)
            {
                panels[i].SetActive(true); // Включаем нужную
            }
            else
            {
                // Принудительно выключаем все остальные (остановит их AudioSource)
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
    }

    // 2. Выбор режима (Развилка)
    public void ChooseMainMode(string mode)
    {
        selectedMode = mode; // Запоминаем "1vs1", "2vs2" или "Campaign"

        if (mode == "Campaign")
        {
            // Если кампания, открываем сразу панель выбора героя (например, индекс 5)
            OpenPanel(5);
        }
        else if (mode == "1vs1")
        {
            // Если 1 на 1, открываем панель выбора подрежима (индекс 1)
            OpenPanel(1);
        }
        else if (mode == "2vs2")
        {
            // Если 2 на 2, открываем панель выбора подрежима (индекс 2)
            OpenPanel(2);
        }
    }

    // 3. Выбор противника
    public void ChooseOpponent(string opponentType)
    {
        // Переключаем на выбор персонажа (например, индекс 3)
        OpenPanel(3);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
