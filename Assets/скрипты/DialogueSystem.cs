using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ╔══════════════════════════════════════════════════════════════╗
// ║           СИСТЕМА ДИАЛОГОВ — ОДИН СКРИПТ                   ║
// ║  Поставь этот скрипт на NPC и настрой поля в Inspector      ║
// ╚══════════════════════════════════════════════════════════════╝
//
// УСТАНОВКА (3 шага):
//   1. Поставь скрипт DialogueSystem.cs на GameObject своего NPC
//   2. Создай UI элементы и перетащи их в поля Inspector
//   3. Напиши фразы в массиве "Dialogue Lines" в Inspector
//   4. Убедись что на игроке стоит тег "Player"
//
// ТРЕБОВАНИЯ:
//   - TextMeshPro (Window → Package Manager → TextMeshPro → Install)
//   - Тег "Player" на персонаже игрока

public class DialogueSystem : MonoBehaviour
{
    // ══════════════════════════════════════════════
    //   НАСТРОЙКИ В INSPECTOR
    // ══════════════════════════════════════════════

    [Header("━━━ ДИАЛОГ ━━━")]
    [Tooltip("Имя этого персонажа")]
    public string npcName = "Персонаж";

    [Tooltip("Фразы диалога по порядку")]
    [TextArea(2, 4)]
    public string[] dialogueLines = {
        "Привет, путник! Рад тебя видеть в нашем городе.",
        "Говорят, в лесу к востоку появились странные существа...",
        "Будь осторожен в пути. Удачи тебе!"
    };

    [Space]
    [Header("━━━ UI ЭЛЕМЕНТЫ ━━━")]
    [Tooltip("Главная панель диалога (Panel с фоном)")]
    public GameObject dialoguePanel;

    [Tooltip("Текст имени персонажа (TextMeshPro)")]
    public TextMeshProUGUI nameText;

    [Tooltip("Текст диалога (TextMeshPro)")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("Подсказка 'Нажмите E' — любой GameObject с текстом")]
    public GameObject interactPrompt;

    [Tooltip("Иконка ▼ или ► — мигает когда фраза закончена (необязательно)")]
    public GameObject continueArrow;

    [Space]
    [Header("━━━ СКОРОСТЬ ПЕЧАТИ ━━━")]
    [Tooltip("Секунд на один символ. Меньше = быстрее")]
    [Range(0.01f, 0.15f)]
    public float typingSpeed = 0.04f;

    [Tooltip("Дополнительная пауза после точки, ! и ?")]
    [Range(0f, 0.5f)]
    public float pauseAfterSentence = 0.2f;

    [Space]
    [Header("━━━ ЗОНА ВЗАИМОДЕЙСТВИЯ ━━━")]
    [Tooltip("Радиус в котором появляется подсказка E")]
    public float interactionRadius = 2.5f;

    [Tooltip("Тег игрока (обычно 'Player')")]
    public string playerTag = "Player";

    // ══════════════════════════════════════════════
    //   ПРИВАТНЫЕ ПЕРЕМЕННЫЕ
    // ══════════════════════════════════════════════

    private int currentLine = 0;
    private bool isDialogueOpen = false;
    private bool isTyping = false;
    private bool playerNearby = false;
    private string currentFullText = "";
    private Coroutine typingCoroutine;
    private Transform playerTransform;

    // ══════════════════════════════════════════════
    //   СТАРТ
    // ══════════════════════════════════════════════

    void Start()
    {
        // Скрываем панель диалога и подсказку на старте
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (continueArrow != null) continueArrow.SetActive(false);

        // Ищем игрока
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning($"[DialogueSystem] Игрок с тегом '{playerTag}' не найден! Поставь тег Player на игрока.");
    }

    // ══════════════════════════════════════════════
    //   UPDATE — каждый кадр
    // ══════════════════════════════════════════════

    void Update()
    {
        if (playerTransform == null) return;

        // Проверяем дистанцию до игрока
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= interactionRadius;

        // Если игрок вошёл/вышел из зоны
        if (inRange != playerNearby)
        {
            playerNearby = inRange;

            // Показываем подсказку "Нажмите E" только если диалог не открыт
            if (interactPrompt != null)
                interactPrompt.SetActive(playerNearby && !isDialogueOpen);
        }

        // Нажатие E
        if (Input.GetKeyDown(KeyCode.E) && playerNearby)
        {
            if (!isDialogueOpen)
            {
                OpenDialogue();
            }
            else if (isTyping)
            {
                // Пропустить печать — показать всю фразу сразу
                SkipTyping();
            }
            else
            {
                // Следующая фраза
                NextLine();
            }
        }

        // ESC закрывает диалог
        if (Input.GetKeyDown(KeyCode.Escape) && isDialogueOpen)
        {
            CloseDialogue();
        }
    }

    // ══════════════════════════════════════════════
    //   ДИАЛОГ
    // ══════════════════════════════════════════════

    void OpenDialogue()
    {
        isDialogueOpen = true;
        currentLine = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (nameText != null) nameText.text = npcName;

        ShowLine(currentLine);
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            ShowLine(currentLine);
        }
        else
        {
            // Все фразы закончились
            CloseDialogue();
        }
    }

    void ShowLine(int index)
    {
        if (continueArrow != null) continueArrow.SetActive(false);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(dialogueLines[index]));
    }

    void CloseDialogue()
    {
        isDialogueOpen = false;
        currentLine = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (continueArrow != null) continueArrow.SetActive(false);

        // Показываем подсказку снова если игрок рядом
        if (interactPrompt != null)
            interactPrompt.SetActive(playerNearby);
    }

    void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (dialogueText != null)
            dialogueText.text = currentFullText;

        isTyping = false;

        if (continueArrow != null)
            StartCoroutine(BlinkArrow());
    }

    // ══════════════════════════════════════════════
    //   ЭФФЕКТ ПЕЧАТНОЙ МАШИНКИ
    // ══════════════════════════════════════════════

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        currentFullText = text;

        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char c in text)
        {
            if (dialogueText != null)
                dialogueText.text += c;

            // Пауза после знаков конца предложения
            if (c == '.' || c == '!' || c == '?' || c == '…')
                yield return new WaitForSeconds(pauseAfterSentence);
            else
                yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Показываем стрелку "продолжить"
        if (continueArrow != null)
            StartCoroutine(BlinkArrow());
    }

    // ══════════════════════════════════════════════
    //   МИГАНИЕ СТРЕЛКИ
    // ══════════════════════════════════════════════

    IEnumerator BlinkArrow()
    {
        if (continueArrow == null) yield break;

        continueArrow.SetActive(true);

        CanvasGroup cg = continueArrow.GetComponent<CanvasGroup>();
        if (cg == null) cg = continueArrow.AddComponent<CanvasGroup>();

        float t = 0f;
        // Мигаем пока стрелка активна
        while (continueArrow.activeSelf && !isTyping)
        {
            t += Time.deltaTime * 4f;
            cg.alpha = Mathf.Abs(Mathf.Sin(t));
            yield return null;
        }
    }

    // ══════════════════════════════════════════════
    //   ГИЗМО — радиус в редакторе (зелёный круг)
    // ══════════════════════════════════════════════

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.1f, 0.9f, 0.3f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        Gizmos.color = new Color(0.1f, 0.9f, 0.3f, 0.08f);
        Gizmos.DrawSphere(transform.position, interactionRadius);
    }
}
