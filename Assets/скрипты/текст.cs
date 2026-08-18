using UnityEngine;
using TMPro;
using System.Collections;
using System.Text; // Добавлено для оптимизации работы с текстом

public class UndertaleDialogue : MonoBehaviour
{
    [Header("=== Настройки диалога (Undertale-style) ===")]
    [Tooltip("Текст, который будет показываться по буквам")]
    [TextArea(3, 10)]
    public string[] dialogues = new string[]
    {
        "Приветствую, человек!",
        "Я застрял в этом коде...",
        "Нажми пробел, чтобы продолжить.",
        "Или нажми его быстро, чтобы пропустить анимацию!"
    };

    [Header("Компоненты Unity")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject arrowObject;

    [Header("Скорость печати")]
    [SerializeField][Range(0.01f, 0.1f)] private float typingSpeed = 0.035f;

    private int currentIndex = 0;
    private string fullText = "";
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (dialogueText == null)
        {
            Debug.LogError("Ошибка: Назначь TMP_Text (TextMeshPro) в инспекторе на объекте " + gameObject.name);
            return;
        }

        if (arrowObject != null)
            arrowObject.SetActive(false);

        // Запуск первого диалога
        if (dialogues.Length > 0)
        {
            StartNextDialogue();
        }
    }

    private void Update()
    {
        // Проверка нажатия Space (Пробел)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                FinishCurrentDialogue();
            }
            else if (currentIndex < dialogues.Length)
            {
                StartNextDialogue();
            }
        }
    }

    private void StartNextDialogue()
    {
        if (currentIndex >= dialogues.Length) return;

        fullText = dialogues[currentIndex];

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeTextCoroutine());
        currentIndex++;
    }

    private void FinishCurrentDialogue()
    {
        StopCoroutine(typingCoroutine);
        dialogueText.text = fullText;
        isTyping = false;
        if (arrowObject != null) arrowObject.SetActive(true);
    }

    private IEnumerator TypeTextCoroutine()
    {
        isTyping = true;
        if (arrowObject != null) arrowObject.SetActive(false);

        dialogueText.text = "";
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < fullText.Length; i++)
        {
            sb.Append(fullText[i]);
            dialogueText.text = sb.ToString();

            // Если символ - пауза (запятая или точка), можно добавить задержку для эффекта
            // yield return new WaitForSeconds(fullText[i] == ',' ? typingSpeed * 3 : typingSpeed);

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (arrowObject != null) arrowObject.SetActive(true);
    }

    // --- Публичные методы управления ---

    public void StartFromBeginning()
    {
        currentIndex = 0;
        StartNextDialogue();
    }

    public void SkipToDialogue(int index)
    {
        if (index >= 0 && index < dialogues.Length)
        {
            currentIndex = index;
            StartNextDialogue();
        }
    }
}