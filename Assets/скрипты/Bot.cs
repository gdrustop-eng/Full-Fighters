using System.Collections;
using UnityEngine;

/// <summary>
/// 2D бот с системой состояний:
///   IDLE     → стоит на месте
///   PATROL   → ходит между точками патруля
///   CHASE    → видит игрока — бежит к нему
///   ATTACK   → игрок в зоне атаки — бьёт его
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bot : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  ХАРАКТЕРИСТИКИ
    // ─────────────────────────────────────────
    [Header("Здоровье")]
    public int maxHealth = 40;

    [Header("Передвижение")]
    public float moveSpeed = 4f;

    [Header("Оружие")]
    public int attackDamage = 10;
    public float attackCooldown = 2f;   // секунды между атаками

    [Header("Радиусы")]
    public float visionRadius = 6f;     // радиус обнаружения игрока
    public float attackRadius = 1.2f;   // радиус атаки (должен быть < visionRadius)

    [Header("Патруль")]
    public Transform[] patrolPoints;    // точки патруля (перетащить в инспекторе)
    public float patrolWaitTime = 1.5f; // пауза в каждой точке

    [Header("Слой игрока")]
    public LayerMask playerLayer;       // назначь слой "Player" в инспекторе

    // ─────────────────────────────────────────
    //  ПРИВАТНЫЕ ПОЛЯ
    // ─────────────────────────────────────────
    private int _currentHealth;
    private bool _canAttack = true;
    private bool _isAlive = true;

    // ИИ — состояния
    private enum State { Idle, Patrol, Chase, Attack }
    private State _state = State.Idle;

    // Ссылка на игрока
    private Transform _player;
    private Player _playerScript;

    // Патруль
    private int _patrolIndex = 0;
    private bool _waitingAtPoint = false;

    // Компоненты Unity
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    // Цвета состояний (визуальный фидбэк)
    private readonly Color _colorIdle = new Color(0.8f, 0.3f, 0.3f);   // красный
    private readonly Color _colorChase = new Color(1f, 0.6f, 0f);     // оранжевый
    private readonly Color _colorAttack = new Color(1f, 0.1f, 0.1f);   // ярко-красный
    private readonly Color _colorPatrol = new Color(0.6f, 0.2f, 0.8f);   // фиолетовый
    private readonly Color _colorHurt = Color.white;

    // ─────────────────────────────────────────
    //  ИНИЦИАЛИЗАЦИЯ
    // ─────────────────────────────────────────
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _currentHealth = maxHealth;

        _rb.freezeRotation = true;
        _rb.gravityScale = 3f;

        // Ищем игрока по тегу
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            _playerScript = playerObj.GetComponent<Player>();
        }

        // Выбираем начальное состояние
        _state = (patrolPoints != null && patrolPoints.Length > 0)
            ? State.Patrol
            : State.Idle;

        SetColor(_colorIdle);
        Debug.Log($"[Bot] Создан. HP: {_currentHealth} | Состояние: {_state}");
    }

    // ─────────────────────────────────────────
    //  ОСНОВНОЙ ЦИКЛ ИИ
    // ─────────────────────────────────────────
    void Update()
    {
        if (!_isAlive) return;

        float distToPlayer = _player != null
            ? Vector2.Distance(transform.position, _player.position)
            : float.MaxValue;

        // ── Переключение состояний ──────────────
        if (distToPlayer <= attackRadius)
        {
            _state = State.Attack;
        }
        else if (distToPlayer <= visionRadius)
        {
            _state = State.Chase;
        }
        else
        {
            // Игрок вышел из зоны видимости — возвращаемся к патрулю / ожиданию
            _state = (patrolPoints != null && patrolPoints.Length > 0)
                ? State.Patrol
                : State.Idle;
        }

        // ── Выполнение состояния ────────────────
        switch (_state)
        {
            case State.Idle: DoIdle(); break;
            case State.Patrol: DoPatrol(); break;
            case State.Chase: DoChase(); break;
            case State.Attack: DoAttack(); break;
        }
    }

    // ─────────────────────────────────────────
    //  IDLE — стоит на месте
    // ─────────────────────────────────────────
    void DoIdle()
    {
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        SetColor(_colorIdle);
    }

    // ─────────────────────────────────────────
    //  PATROL — ходит между точками
    // ─────────────────────────────────────────
    void DoPatrol()
    {
        if (_waitingAtPoint) return;
        SetColor(_colorPatrol);

        Transform target = patrolPoints[_patrolIndex];
        float dist = Mathf.Abs(transform.position.x - target.position.x);

        if (dist < 0.15f)
        {
            // Достигли точки — ждём и идём к следующей
            StartCoroutine(WaitAtPatrolPoint());
        }
        else
        {
            // Двигаемся к точке патруля
            float dir = (target.position.x - transform.position.x) > 0 ? 1f : -1f;
            _rb.velocity = new Vector2(dir * (moveSpeed * 0.6f), _rb.velocity.y);
            FlipSprite(dir);
        }
    }

    IEnumerator WaitAtPatrolPoint()
    {
        _waitingAtPoint = true;
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        yield return new WaitForSeconds(patrolWaitTime);
        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
        _waitingAtPoint = false;
    }

    // ─────────────────────────────────────────
    //  CHASE — бежит за игроком
    // ─────────────────────────────────────────
    void DoChase()
    {
        if (_player == null) return;
        SetColor(_colorChase);

        float dir = (_player.position.x - transform.position.x) > 0 ? 1f : -1f;
        _rb.velocity = new Vector2(dir * moveSpeed, _rb.velocity.y);
        FlipSprite(dir);

        Debug.Log($"[Bot] Преследует игрока! Дистанция: {Vector2.Distance(transform.position, _player.position):F1}");
    }

    // ─────────────────────────────────────────
    //  ATTACK — бьёт игрока в радиусе атаки
    // ─────────────────────────────────────────
    void DoAttack()
    {
        // Стоим на месте во время атаки
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        SetColor(_colorAttack);

        if (_canAttack && _playerScript != null)
        {
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        _canAttack = false;

        Debug.Log($"[Bot] ⚔ Атакует игрока! Урон: {attackDamage}");
        _playerScript.TakeDamage(attackDamage);

        // Кратковременная вспышка
        yield return StartCoroutine(FlashColor(_colorHurt, 0.1f));

        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }

    // ─────────────────────────────────────────
    //  ПОЛУЧЕНИЕ УРОНА (вызывается из Player.cs)
    // ─────────────────────────────────────────
    public void TakeDamage(int damage)
    {
        if (!_isAlive) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        Debug.Log($"[Bot] Получил {damage} урона. HP: {_currentHealth}/{maxHealth}");

        StartCoroutine(FlashColor(_colorHurt, 0.12f));

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    // ─────────────────────────────────────────
    //  СМЕРТЬ
    // ─────────────────────────────────────────
    void Die()
    {
        _isAlive = false;
        _rb.velocity = Vector2.zero;
        Debug.Log("[Bot] ☠ Уничтожен!");

        // Можно заменить на анимацию смерти
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Быстро мигаем и исчезаем
        for (int i = 0; i < 5; i++)
        {
            if (_sr) _sr.enabled = false;
            yield return new WaitForSeconds(0.08f);
            if (_sr) _sr.enabled = true;
            yield return new WaitForSeconds(0.08f);
        }
        gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────
    //  ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // ─────────────────────────────────────────
    void FlipSprite(float direction)
    {
        if (_sr != null)
            _sr.flipX = direction < 0;
    }

    void SetColor(Color c)
    {
        if (_sr != null) _sr.color = c;
    }

    IEnumerator FlashColor(Color flash, float duration)
    {
        Color prev = _sr != null ? _sr.color : Color.white;
        SetColor(flash);
        yield return new WaitForSeconds(duration);
        SetColor(prev);
    }

    // ─────────────────────────────────────────
    //  GIZMOS — визуализация радиусов в редакторе
    // ─────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Радиус видения — жёлтый
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        // Радиус атаки — красный
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}