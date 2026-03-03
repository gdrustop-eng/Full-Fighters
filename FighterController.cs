using UnityEngine;
using UnityEngine.InputSystem;

public class FighterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float attackDamage = 15f;
    public float attackRange = 1.5f;
    public float maxHealth = 100f;

    public int playerNumber = 1;
    public Transform opponent;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private BoxCollider2D col;

    private bool isGrounded;
    private bool isAttacking;
    private bool isCrouching;
    private bool isDead;
    private float attackTimer;
    private float currentHealth;

    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;

    private Key keyLeft, keyRight, keyJump, keyCrouch, keyAttack;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();

        currentHealth = maxHealth;

        standingColliderSize = col.size;
        standingColliderOffset = col.offset;

        if (playerNumber == 1)
        {
            keyLeft = Key.A;
            keyRight = Key.D;
            keyJump = Key.W;
            keyCrouch = Key.S;
            keyAttack = Key.LeftShift;
        }
        else
        {
            keyLeft = Key.LeftArrow;
            keyRight = Key.RightArrow;
            keyJump = Key.UpArrow;
            keyCrouch = Key.DownArrow;
            keyAttack = Key.RightShift;
        }
    }

    void Update()
    {
        if (isDead) return;

        CheckGround();
        HandleCrouch();
        HandleMovement();
        HandleJump();
        HandleAttack();
        FaceOpponent();
        UpdateAnimations();

        attackTimer -= Time.deltaTime;
    }

    bool Press(Key key) => Keyboard.current[key].isPressed;
    bool PressDown(Key key) => Keyboard.current[key].wasPressedThisFrame;

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    void HandleCrouch()
    {
        bool wantsCrouch = Press(keyCrouch) && isGrounded && !isAttacking;

        if (wantsCrouch && !isCrouching)
        {
            isCrouching = true;
            col.size = new Vector2(standingColliderSize.x, standingColliderSize.y * 0.5f);
            col.offset = new Vector2(standingColliderOffset.x, standingColliderOffset.y - standingColliderSize.y * 0.25f);
        }

        if (!wantsCrouch && isCrouching)
        {
            isCrouching = false;
            col.size = standingColliderSize;
            col.offset = standingColliderOffset;
        }
    }

    void HandleMovement()
    {
        if (isAttacking) return;

        float direction = 0f;
        if (Press(keyLeft)) direction = -1f;
        if (Press(keyRight)) direction = 1f;

        if (direction != 0f && isCrouching)
        {
            isCrouching = false;
            col.size = standingColliderSize;
            col.offset = standingColliderOffset;
        }

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void HandleJump()
    {
        if (isCrouching) return;

        if (PressDown(keyJump) && isGrounded && !isAttacking)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void HandleAttack()
    {
        if (PressDown(keyAttack) && attackTimer <= 0f)
        {
            isAttacking = true;
            attackTimer = 0.3f;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            anim.SetTrigger("attack");

            Invoke(nameof(DealDamage), 0.15f);
            Invoke(nameof(EndAttack), 0.4f);
        }
    }

    void DealDamage()
    {
        if (opponent == null) return;

        float distance = Vector2.Distance(transform.position, opponent.position);
        if (distance <= attackRange)
        {
            FighterController opponentScript = opponent.GetComponent<FighterController>();
            if (opponentScript != null)
                opponentScript.TakeDamage(attackDamage);
        }
    }

    void EndAttack() => isAttacking = false;

    void UpdateAnimations()
    {
        if (isAttacking) return;

        float velocityX = Mathf.Abs(rb.linearVelocity.x);

        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isCrouching", isCrouching);
        anim.SetBool("isWalking", velocityX > 0.1f);
        anim.SetFloat("velocityY", rb.linearVelocity.y);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        sr.color = Color.red;
        Invoke(nameof(ResetColor), 0.15f);

        anim.SetTrigger("hurt");

        if (currentHealth <= 0f) Die();
    }

    void ResetColor() => sr.color = Color.white;

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        anim.SetTrigger("die");

        GameManager.Instance?.PlayerDied(playerNumber);
    }

    void FaceOpponent()
    {
        if (opponent == null) return;
        bool opponentIsOnRight = opponent.position.x > transform.position.x;
        sr.flipX = !opponentIsOnRight;
    }

    public void ResetFighter(Vector3 startPosition)
    {
        currentHealth = maxHealth;
        isDead = false;
        isAttacking = false;
        isCrouching = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        col.size = standingColliderSize;
        col.offset = standingColliderOffset;
        transform.position = startPosition;
        transform.localScale = Vector3.one;
        sr.color = Color.white;

        anim.Rebind();
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}