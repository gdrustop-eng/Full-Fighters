using UnityEngine;
using TMPro;
using System.Collections;
// Код для появления диалогов впринципе
public class UndertaleDialogue : MonoBehaviour
{
    [Header("=== Настройки диалога (Undertale-style) ===")]
    [Tooltip("Текст, который будет показываться по буквам")]
    [TextArea(3, 10)]
    public string[] dialogues = new string[]
    {
        "Тест Поля",
        "Тест Поля",
        "Тест Поля",
        "Тест Поля",
        "Тест Поля"
    };

    [Header("Компоненты Unity")]
    [SerializeField] private TMP_Text dialogueText;      // TextMeshProUGUI (рекомендуется)
    [SerializeField] private GameObject arrowObject;      // стрелка ▼ (любой UI-объект)

    [Header("Скорость печати")]
    [SerializeField][Range(0.01f, 0.1f)] private float typingSpeed = 0.035f; // как в Undertale

    private int currentIndex = 0;      // какой сейчас диалог
    private string fullText = "";      // полный текст текущего диалога
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (dialogueText == null)
        {
            Debug.LogError("Назначь TMP_Text в инспекторе!");
            return;
        }

        if (arrowObject != null)
            arrowObject.SetActive(false);

        // Запускаем первый диалог автоматически
        if (dialogues.Length > 0)
            StartNextDialogue();
    }

    /// <summary>
    /// Показать следующий диалог (или первый)
    /// </summary>
    private void StartNextDialogue()
    {
        if (currentIndex >= dialogues.Length)
        {
            dialogueText.text = "Конец демонстрации.\nМожно перезапустить сцену :)";
            if (arrowObject != null) arrowObject.SetActive(false);
            return;
        }

        fullText = dialogues[currentIndex];
        dialogueText.text = "";
        if (arrowObject != null) arrowObject.SetActive(false);

        // Если уже печатается — останавливаем
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeTextCoroutine());
        currentIndex++;
    }

    /// <summary>
    /// Основная корутина печати по буквам
    /// </summary>
    private IEnumerator TypeTextCoroutine()
    {
        isTyping = true;
        string displayed = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            displayed += fullText[i];
            dialogueText.text = displayed;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Текст полностью напечатан
        isTyping = false;
        if (arrowObject != null) arrowObject.SetActive(true);
    }

    private void Update()
    {
        // Проверка нажатия Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // === СКИП ДО КОНЦА ===
                StopCoroutine(typingCoroutine);
                dialogueText.text = fullText;
                isTyping = false;
                if (arrowObject != null) arrowObject.SetActive(true);
            }
            else
            {
                // === СЛЕДУЮЩИЙ ДИАЛОГ ===
                StartNextDialogue();
            }
        }
    }

    // ==================================================================
    // Дополнительные публичные методы (на случай, если нужно вызывать из других скриптов)
    // ==================================================================
    public void StartFromBeginning() => currentIndex = 0; StartNextDialogue();
    public void SkipToDialogue(int index)
    {
        if (index >= 0 && index < dialogues.Length)
        {
            currentIndex = index;
            StartNextDialogue();
        }
    }
}