using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ============================================================
// Прикрепи на пустой GameObject на сцене выбора персонажа
//
// УПРАВЛЕНИЕ:
//   Игрок 1 — A/D для выбора,  LeftShift  для ГОТОВО
//   Игрок 2 — ←/→ для выбора, RightShift для ГОТОВО
// ============================================================
public class CharacterSelectManager : MonoBehaviour
{
    // ----------------------------------------------------------
    [Header("--- ПЕРСОНАЖИ ---")]
    [Tooltip("Имена персонажей. Element 0 = первый и т.д.")]
    public string[] characterNames;

    [Tooltip("Большие картинки персонажей (в том же порядке что и имена)")]
    public Sprite[] characterSprites;

    [Tooltip("Маленькие картинки для карточек (в том же порядке)")]
    public Sprite[] characterCardSprites;

    [Tooltip("ПРЕФАБЫ персонажей для спавна в игровой сцене (в том же порядке что и имена!)")]
    public GameObject[] characterPrefabs;

    // ----------------------------------------------------------
    [Header("--- КАРТОЧКИ ПЕРСОНАЖЕЙ (общие) ---")]
    [Tooltip("Image на каждой карточке — сюда скрипт ставит картинку персонажа")]
    public Image[] cardImages;          // картинки на карточках

    [Tooltip("Рамка/подсветка карточки для Игрока 1 (GameObject с Image). По одному на каждую карточку")]
    public GameObject[] player1Highlights; // подсветка выбранной карточки игрока 1

    [Tooltip("Рамка/подсветка карточки для Игрока 2")]
    public GameObject[] player2Highlights; // подсветка игрока 2

    // ----------------------------------------------------------
    [Header("--- ИГРОК 1 ---")]
    [Tooltip("Большое превью слева (Image со знаком ?)")]
    public Image player1Preview;

    [Tooltip("Текст имени под превью игрока 1")]
    public Text player1NameText;

    [Tooltip("Кнопка READY игрока 1")]
    public Button player1ReadyButton;

    [Tooltip("Image самой кнопки READY игрока 1 (для смены цвета)")]
    public Image player1ReadyImage;

    // ----------------------------------------------------------
    [Header("--- ИГРОК 2 ---")]
    [Tooltip("Большое превью справа")]
    public Image player2Preview;

    [Tooltip("Текст имени под превью игрока 2")]
    public Text player2NameText;

    [Tooltip("Кнопка READY игрока 2")]
    public Button player2ReadyButton;

    [Tooltip("Image самой кнопки READY игрока 2")]
    public Image player2ReadyImage;

    // ----------------------------------------------------------
    [Header("--- НАСТРОЙКИ ---")]
    [Tooltip("Название следующей сцены (точно как в Build Settings)")]
    public string nextSceneName = "GameScene";

    [Tooltip("Картинка-заглушка (знак вопроса) когда персонаж не выбран")]
    public Sprite emptySprite;

    [Tooltip("Цвет подсветки карточки Игрока 1")]
    public Color highlight1Color = new Color(0.3f, 0.7f, 1f, 1f);   // голубой

    [Tooltip("Цвет подсветки карточки Игрока 2")]
    public Color highlight2Color = new Color(1f, 0.4f, 0.3f, 1f);   // красный

    // ----------------------------------------------------------
    // Цвета кнопки READY
    private readonly Color colorLocked = new Color(0.8f, 0.2f, 0.2f); // красный — заблокирована
    private readonly Color colorUnlocked = new Color(0.9f, 0.9f, 0.9f); // белый   — можно нажать
    private readonly Color colorReady = new Color(0.2f, 0.8f, 0.3f); // зелёный — нажата

    // ----------------------------------------------------------
    // Внутреннее состояние
    private int p1Cursor = 0;     // на какой карточке стоит курсор игрока 1
    private int p2Cursor = 0;     // курсор игрока 2
    private bool p1Locked = false; // игрок 1 нажал ГОТОВО
    private bool p2Locked = false;

    // задержка между нажатиями клавиш (чтобы не прыгало слишком быстро)
    private float p1InputTimer = 0f;
    private float p2InputTimer = 0f;
    private const float INPUT_DELAY = 0.2f;

    // ----------------------------------------------------------

    void Start()
    {
        // Инициализация карточек
        for (int i = 0; i < cardImages.Length; i++)
        {
            if (i < characterCardSprites.Length && characterCardSprites[i] != null)
                cardImages[i].sprite = characterCardSprites[i];
        }

        // Спрятать все подсветки
        SetAllHighlights(player1Highlights, false);
        SetAllHighlights(player2Highlights, false);

        // Курсоры на разные карточки чтобы не совпадали сразу
        p1Cursor = 0;
        p2Cursor = (characterNames.Length > 1) ? 1 : 0;

        // Показать начальное положение курсоров
        UpdateCursorVisual(player1Highlights, p1Cursor, highlight1Color);
        UpdateCursorVisual(player2Highlights, p2Cursor, highlight2Color);

        // Обновить превью
        UpdatePreview(player1Preview, player1NameText, p1Cursor);
        UpdatePreview(player2Preview, player2NameText, p2Cursor);

        // Кнопки READY — заблокировать (красные)
        SetReadyButton(player1ReadyButton, player1ReadyImage, false, false);
        SetReadyButton(player2ReadyButton, player2ReadyImage, false, false);

        // Подвесить клик на кнопки READY (на случай если мышью тыкнут)
        player1ReadyButton.onClick.AddListener(OnPlayer1Ready);
        player2ReadyButton.onClick.AddListener(OnPlayer2Ready);
    }

    // ----------------------------------------------------------

    void Update()
    {
        p1InputTimer -= Time.deltaTime;
        p2InputTimer -= Time.deltaTime;

        HandlePlayer1Input();
        HandlePlayer2Input();
    }

    // ----------------------------------------------------------
    // Управление Игрока 1 — A / D / Space
    // ----------------------------------------------------------
    void HandlePlayer1Input()
    {
        if (p1Locked) return;

        // Влево — A
        if (Input.GetKey(KeyCode.A) && p1InputTimer <= 0f)
        {
            p1InputTimer = INPUT_DELAY;
            MovePlayer1Cursor(-1);
        }
        // Вправо — D
        else if (Input.GetKey(KeyCode.D) && p1InputTimer <= 0f)
        {
            p1InputTimer = INPUT_DELAY;
            MovePlayer1Cursor(1);
        }

        // ГОТОВО — левый Shift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            OnPlayer1Ready();
        }
    }

    // ----------------------------------------------------------
    // Управление Игрока 2 — ← / → / Enter
    // ----------------------------------------------------------
    void HandlePlayer2Input()
    {
        if (p2Locked) return;

        // Влево — стрелка влево
        if (Input.GetKey(KeyCode.LeftArrow) && p2InputTimer <= 0f)
        {
            p2InputTimer = INPUT_DELAY;
            MovePlayer2Cursor(-1);
        }
        // Вправо — стрелка вправо
        else if (Input.GetKey(KeyCode.RightArrow) && p2InputTimer <= 0f)
        {
            p2InputTimer = INPUT_DELAY;
            MovePlayer2Cursor(1);
        }

        // ГОТОВО — правый Shift
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            OnPlayer2Ready();
        }
    }

    // ----------------------------------------------------------
    // Движение курсора
    // ----------------------------------------------------------
    void MovePlayer1Cursor(int dir)
    {
        int total = characterNames.Length;
        p1Cursor = (p1Cursor + dir + total) % total;

        UpdateCursorVisual(player1Highlights, p1Cursor, highlight1Color);
        UpdatePreview(player1Preview, player1NameText, p1Cursor);

        // Если навёл на того же что игрок 2 — READY красная (нельзя нажать)
        bool sameAsP2 = (p1Cursor == p2Cursor);
        SetReadyButton(player1ReadyButton, player1ReadyImage, !sameAsP2, false);
    }

    void MovePlayer2Cursor(int dir)
    {
        int total = characterNames.Length;
        p2Cursor = (p2Cursor + dir + total) % total;

        UpdateCursorVisual(player2Highlights, p2Cursor, highlight2Color);
        UpdatePreview(player2Preview, player2NameText, p2Cursor);

        // Если навёл на того же что игрок 1 — READY красная (нельзя нажать)
        bool sameAsP1 = (p2Cursor == p1Cursor);
        SetReadyButton(player2ReadyButton, player2ReadyImage, !sameAsP1, false);
    }

    // ----------------------------------------------------------
    // ГОТОВО
    // ----------------------------------------------------------
    void OnPlayer1Ready()
    {
        if (p1Locked) return;

        // Нельзя взять персонажа которого навёл или заблокировал игрок 2
        if (p1Cursor == p2Cursor)
        {
            Debug.Log("Этот персонаж уже выбран Игроком 2!");
            return;
        }

        p1Locked = true;
        SetReadyButton(player1ReadyButton, player1ReadyImage, false, true); // зелёная
        Debug.Log("Игрок 1 готов: " + characterNames[p1Cursor]);
        CheckBothReady();
    }

    void OnPlayer2Ready()
    {
        if (p2Locked) return;

        // Нельзя взять персонажа которого навёл или заблокировал игрок 1
        if (p2Cursor == p1Cursor)
        {
            Debug.Log("Этот персонаж уже выбран Игроком 1!");
            return;
        }

        p2Locked = true;
        SetReadyButton(player2ReadyButton, player2ReadyImage, false, true);
        Debug.Log("Игрок 2 готов: " + characterNames[p2Cursor]);
        CheckBothReady();
    }

    // ----------------------------------------------------------
    void CheckBothReady()
    {
        if (!p1Locked || !p2Locked) return;

        // Индекс и имя (как раньше)
        GameData1.Player1CharacterIndex1 = p1Cursor;
        GameData1.Player1CharacterName1 = characterNames[p1Cursor];
        GameData1.Player2CharacterIndex1 = p2Cursor;
        GameData1.Player2CharacterName1 = characterNames[p2Cursor];

        // НОВОЕ: сохраняем сам префаб персонажа, чтобы игровая сцена знала КОГО спавнить
        GameData1.Player1CharacterPrefab1 = (p1Cursor < characterPrefabs.Length) ? characterPrefabs[p1Cursor] : null;
        GameData1.Player2CharacterPrefab1 = (p2Cursor < characterPrefabs.Length) ? characterPrefabs[p2Cursor] : null;

        if (GameData1.Player1CharacterPrefab1 == null)
            Debug.LogWarning("У персонажа '" + characterNames[p1Cursor] + "' не назначен префаб в characterPrefabs!");
        if (GameData1.Player2CharacterPrefab1 == null)
            Debug.LogWarning("У персонажа '" + characterNames[p2Cursor] + "' не назначен префаб в characterPrefabs!");

        Debug.Log("Оба готовы! Переход: " + nextSceneName);
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nextSceneName);
    }

    // ----------------------------------------------------------
    // Вспомогательные методы
    // ----------------------------------------------------------

    void UpdatePreview(Image img, Text txt, int index)
    {
        if (img != null)
            img.sprite = (index < characterSprites.Length && characterSprites[index] != null)
                         ? characterSprites[index] : emptySprite;

        if (txt != null && index < characterNames.Length)
            txt.text = "Name: " + characterNames[index];
    }

    // Показать подсветку только на нужной карточке
    void UpdateCursorVisual(GameObject[] highlights, int index, Color col)
    {
        for (int i = 0; i < highlights.Length; i++)
        {
            if (highlights[i] == null) continue;
            bool active = (i == index);
            highlights[i].SetActive(active);
            if (active)
            {
                Image img = highlights[i].GetComponent<Image>();
                if (img) img.color = col;
            }
        }
    }

    void SetAllHighlights(GameObject[] highlights, bool active)
    {
        if (highlights == null) return;
        foreach (var h in highlights)
            if (h != null) h.SetActive(active);
    }

    // interactable=false + ready=false → красная (заблокирована)
    // interactable=true  + ready=false → белая  (можно нажать)
    // interactable=false + ready=true  → зелёная (нажата)
    void SetReadyButton(Button btn, Image img, bool interactable, bool ready)
    {
        if (btn != null) btn.interactable = interactable;
        if (img == null) return;

        if (ready) img.color = colorReady;
        else if (interactable) img.color = colorUnlocked;
        else img.color = colorLocked;
    }
}