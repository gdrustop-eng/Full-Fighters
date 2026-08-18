using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Скрипт игрока для 2D игры.
/// Управление: A/D — движение, W/Пробел — прыжок,
///              L — атака, C — способность (ускоренная атака).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  ХАРАКТЕРИСТИКИ
    // ─────────────────────────────────────────
    [Header("Здоровье")]
    public int maxHealth = 100;
    public float regenAmount = 1f;     // HP за тик
    public float regenInterval = 2f;     // тик каждые 2 секунды

    [Header("Передвижение")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;

    [Header("Атака")]
    public int attackDamage = 10;
    public float attackCooldown = 1f;     // секунды между атаками (обычная)

    [Header("Способность (C)")]
    public float abilityCooldown = 0.5f;  // скорость удара в 2 раза быстрее

    [Header("Наземная проверка")]
    public Transform groundCheck;          // пустой дочерний объект у ног
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    // ─────────────────────────────────────────
    //  UI (перетащить в инспекторе, необязательно)
    // ─────────────────────────────────────────
    [Header("UI (опционально)")]
    public Slider healthBar;
    public Text statusText;

    // ─────────────────────────────────────────
    //  ПРИВАТНЫЕ ПОЛЯ
    // ─────────────────────────────────────────
    private int _currentHealth;
    private bool _isGrounded;
    private bool _canAttack = true;
    private bool _abilityActive = false;  // режим ускоренной атаки

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    // Цвета для визуальной обратной связи
    private Color _colorNormal = new Color(0.2f, 0.6f, 1f);   // синий куб
    private Color _colorAbility = new Color(1f, 0.4f, 0.1f);  // оранжевый при способности
    private Color _colorHurt = new Color(1f, 0.1f, 0.1f);  // красный при уроне

    // ─────────────────────────────────────────
    //  ИНИЦИАЛИЗАЦИЯ
    // ─────────────────────────────────────────
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _currentHealth = maxHealth;

        // Блокируем вращение, чтобы куб не кувыркался
        _rb.freezeRotation = true;

        UpdateUI();

        // Запускаем регенерацию HP
        StartCoroutine(RegenHealth());

        Debug.Log("[Player] Игрок создан. HP: " + _currentHealth);
    }

    // ─────────────────────────────────────────
    //  ОБНОВЛЕНИЕ (каждый кадр)
    // ─────────────────────────────────────────
    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleAttack();
        HandleAbility();
    }

    void FixedUpdate()
    {
        // Проверка земли через Physics2D
        if (groundCheck != null)
        {
            _isGrounded = Physics2D.OverlapCircle(
                groundCheck.position, groundCheckRadius, groundLayer);
        }
    }

    // ─────────────────────────────────────────
    //  ДВИЖЕНИЕ (A / D)
    // ─────────────────────────────────────────
    void HandleMovement()
    {
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;

        _rb.velocity = new Vector2(horizontal * moveSpeed, _rb.velocity.y);

        // Разворачиваем спрайт по направлению движения
        if (horizontal != 0 && _sr != null)
            _sr.flipX = horizontal < 0;
    }

    // ─────────────────────────────────────────
    //  ПРЫЖОК (W / Пробел)
    // ─────────────────────────────────────────
    void HandleJump()
    {
        bool jumpPressed = Input.GetKeyDown(KeyCode.W) ||
                           Input.GetKeyDown(KeyCode.Space);

        if (jumpPressed && _isGrounded)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            Debug.Log("[Player] Прыжок!");
        }
    }

    // ─────────────────────────────────────────
    //  АТАКА (L) — урон 10, перезарядка 1 сек
    // ─────────────────────────────────────────
    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.L) && _canAttack && !_abilityActive)
        {
            StartCoroutine(PerformAttack(attackCooldown, "Атака"));
        }
    }

    // ─────────────────────────────────────────
    //  СПОСОБНОСТЬ (C) — скорость удара 0.5 сек
    // ─────────────────────────────────────────
    void HandleAbility()
    {
        // Зажимаем C — входим в режим ускоренной атаки
        if (Input.GetKeyDown(KeyCode.C))
        {
            _abilityActive = true;
            if (_sr != null) _sr.color = _colorAbility;
            Debug.Log("[Player] Способность активирована! Скорость атаки x2");
            UpdateStatus("⚡ Способность активна");
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            _abilityActive = false;
            if (_sr != null) _sr.color = _colorNormal;
            Debug.Log("[Player] Способность деактивирована");
            UpdateStatus("");
        }

        // Во время способности L тоже работает, но быстрее
        if (_abilityActive && Input.GetKeyDown(KeyCode.L) && _canAttack)
        {
            StartCoroutine(PerformAttack(abilityCooldown, "⚡ Способность-атака"));
        }
    }

    // ─────────────────────────────────────────
    //  ЛОГИКА УДАРА
    // ─────────────────────────────────────────
    IEnumerator PerformAttack(float cooldown, string label)
    {
        _canAttack = false;

        // Зона атаки — прямо перед игроком (50x40 пикселей)
        bool facingRight = _sr != null ? !_sr.flipX : true;
        Vector2 attackOffset = facingRight ? Vector2.right * 0.6f : Vector2.left * 0.6f;
        Vector2 attackSize = new Vector2(0.5f, 0.4f);
        Vector2 attackPos = (Vector2)transform.position + attackOffset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPos, attackSize, 0f);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue; // не бьём себя

            // Пробуем нанести урон боту
            Bot bot = hit.GetComponent<Bot>();
            if (bot != null)
            {
                bot.TakeDamage(attackDamage);
                Debug.Log($"[Player] {label} → попал в [{bot.name}] на {attackDamage} урона!");
            }
        }

        Debug.Log($"[Player] {label} | Перезарядка: {cooldown} сек.");

        // Визуальная вспышка
        yield return StartCoroutine(FlashColor(Color.white, 0.08f));

        yield return new WaitForSeconds(cooldown);
        _canAttack = true;
    }

    // ─────────────────────────────────────────
    //  ПОЛУЧЕНИЕ УРОНА
    // ─────────────────────────────────────────
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        Debug.Log($"[Player] Получил {damage} урона. HP: {_currentHealth}/{maxHealth}");

        UpdateUI();
        StartCoroutine(FlashColor(_colorHurt, 0.15f));

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    // ─────────────────────────────────────────
    //  РЕГЕНЕРАЦИЯ HP (+1 каждые 2 секунды)
    // ─────────────────────────────────────────
    IEnumerator RegenHealth()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenInterval);

            if (_currentHealth > 0 && _currentHealth < maxHealth)
            {
                _currentHealth = Mathf.Min(_currentHealth + (int)regenAmount, maxHealth);
                Debug.Log($"[Player] Регенерация +{regenAmount} HP → {_currentHealth}/{maxHealth}");
                UpdateUI();
            }
        }
    }

    // ─────────────────────────────────────────
    //  СМЕРТЬ
    // ─────────────────────────────────────────
    void Die()
    {
        Debug.Log("[Player] ☠ Игрок погиб!");
        UpdateStatus("☠ Вы погибли");
        // Здесь можно вызвать GameManager.instance.GameOver();
        gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────
    //  ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // ─────────────────────────────────────────

    // Мигание цветом (визуальный фидбэк)
    IEnumerator FlashColor(Color flashColor, float duration)
    {
        if (_sr == null) yield break;
        Color prev = _sr.color;
        _sr.color = flashColor;
        yield return new WaitForSeconds(duration);
        _sr.color = _abilityActive ? _colorAbility : _colorNormal;
    }

    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = _currentHealth;
        }
    }

    void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    // Рисуем зону атаки в редакторе Unity (Gizmos)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.right * 0.6f, new Vector3(0.5f, 0.4f, 0));

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}