using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Список всех панелей")]
    public GameObject[] panels;

    // Открыть панель по индексу и ГАРАНТИРОВАННО закрыть все остальные
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
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

