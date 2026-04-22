using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float baseMoveSpeed = 5f;
    public float moveSpeed = 5f;
    public float jumpForce = 13f;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 1f;
    public int baseAttackDamage = 1;
    public int attackDamage = 1;
    public float attackCooldown = 1f;
    public KeyCode attackKey = KeyCode.J;

    [Header("Abilities")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public float dashForce = 14f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.75f;

    [Header("References")]
    public GameMetrics metrics;
    public GameOverUI gameOverUI;
    public PlayerHealth playerHealth;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded;
    private bool facingRight = true;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isInputLocked = false;
    private bool isDashing = false;

    private bool doubleJumpUnlocked = false;
    private bool dashUnlocked = false;

    private int extraJumpsRemaining = 0;

    private float lastAttackTime = -10f;
    private float lastDashTime = -10f;
    private float dashTimer = 0f;
    private float dashDirection = 1f;
    private float originalGravityScale;

    private Vector3 attackPointStartLocalPosition;
    private int groundContacts = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        EnsureRuntimeReferences();

        if (attackPoint != null)
        {
            attackPointStartLocalPosition = attackPoint.localPosition;
        }

        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
        }

        ApplyProgressStats();
        RefreshAbilityUnlocks();
        RefreshJumpCharges();
    }

    void Update()
    {
        if (isDead)
            return;

        UpdateDashState();

        if (isInputLocked)
        {
            StopHorizontalMovementKeepVertical();
            UpdateAnimator();
            FlipCharacter();
            return;
        }

        if (isDashing)
        {
            HandleDashMovement();
            UpdateAnimator();
            FlipCharacter();
            return;
        }

        TryDash();
        Attack();

        if (!isAttacking)
        {
            Move();
            Jump();
        }
        else
        {
            StopHorizontalMovementKeepVertical();
        }

        UpdateAnimator();
        FlipCharacter();
    }

    private void EnsureRuntimeReferences()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        if (metrics == null)
        {
            metrics = GameMetrics.Instance;
        }

        if (gameOverUI == null)
        {
            gameOverUI = FindFirstObjectByType<GameOverUI>();
        }
    }

    public void ApplyProgressStats()
    {
        moveSpeed = baseMoveSpeed + PlayerProgress.Instance.SpeedLevel;
        attackDamage = baseAttackDamage + PlayerProgress.Instance.DamageLevel;
        RefreshAbilityUnlocks();
        RefreshJumpCharges();
    }

    public void SetInputLocked(bool locked)
    {
        isInputLocked = locked;

        if (locked)
        {
            isAttacking = false;
            CancelDash();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            UpdateAnimator();
        }
    }

    void RefreshAbilityUnlocks()
    {
        doubleJumpUnlocked = PlayerProgress.Instance.DoubleJumpUnlocked;
        dashUnlocked = PlayerProgress.Instance.DashUnlocked;
    }

    void RefreshJumpCharges()
    {
        extraJumpsRemaining = doubleJumpUnlocked ? 1 : 0;
    }

    void Move()
    {
        if (rb == null)
            return;

        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (rb == null)
            return;

        if (!Input.GetButtonDown("Jump"))
            return;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            groundContacts = 0;
            return;
        }

        if (doubleJumpUnlocked && extraJumpsRemaining > 0)
        {
            extraJumpsRemaining--;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void TryDash()
    {
        if (!dashUnlocked)
            return;

        if (rb == null)
            return;

        if (!Input.GetKeyDown(dashKey))
            return;

        if (Time.time < lastDashTime + dashCooldown)
            return;

        StartDash();
    }

    void StartDash()
    {
        if (rb == null)
            return;

        isDashing = true;
        isAttacking = false;
        dashTimer = dashDuration;
        lastDashTime = Time.time;

        float moveInput = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            dashDirection = Mathf.Sign(moveInput);
        }
        else
        {
            dashDirection = facingRight ? 1f : -1f;
        }

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDirection * dashForce, 0f);
    }

    void UpdateDashState()
    {
        if (!isDashing)
            return;

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            EndDash();
        }
    }

    void HandleDashMovement()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(dashDirection * dashForce, 0f);
    }

    void EndDash()
    {
        if (!isDashing)
            return;

        isDashing = false;

        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    void CancelDash()
    {
        if (!isDashing)
            return;

        isDashing = false;

        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
        }
    }

    void StopHorizontalMovementKeepVertical()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void Attack()
    {
        bool attackPressed = Input.GetKeyDown(attackKey) || Input.GetButtonDown("Fire1");

        if (!attackPressed)
            return;

        if (isDashing)
            return;

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;
        isAttacking = true;

        if (animator != null && HasAnimatorParameter("Attack", AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger("Attack");
        }

        DealDamage();
    }

    void DealDamage()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("Attack Point is not assigned on Player.");
            return;
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            EnemyBase enemy = hitColliders[i].GetComponent<EnemyBase>();

            if (enemy == null)
            {
                enemy = hitColliders[i].GetComponentInParent<EnemyBase>();
            }

            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    void UpdateAnimator()
    {
        if (animator == null || rb == null)
            return;

        float speed = Mathf.Abs(rb.linearVelocity.x);

        if (HasAnimatorParameter("Speed", AnimatorControllerParameterType.Float))
        {
            animator.SetFloat("Speed", speed);
        }

        if (HasAnimatorParameter("IsGrounded", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("IsGrounded", isGrounded);
        }

        if (HasAnimatorParameter("IsDashing", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("IsDashing", isDashing);
        }
    }

    void FlipCharacter()
    {
        if (spriteRenderer == null || rb == null)
            return;

        if (rb.linearVelocity.x > 0.1f)
        {
            facingRight = true;
            spriteRenderer.flipX = false;
            UpdateAttackPointDirection();
        }
        else if (rb.linearVelocity.x < -0.1f)
        {
            facingRight = false;
            spriteRenderer.flipX = true;
            UpdateAttackPointDirection();
        }
    }

    void UpdateAttackPointDirection()
    {
        if (attackPoint == null)
            return;

        Vector3 localPos = attackPointStartLocalPosition;
        localPos.x = facingRight ? Mathf.Abs(localPos.x) : -Mathf.Abs(localPos.x);
        attackPoint.localPosition = localPos;
    }

    bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType paramType)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == paramType)
            {
                return true;
            }
        }

        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead)
            return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            if (HasGroundContactFromTop(collision))
            {
                groundContacts++;
                isGrounded = true;
                RefreshJumpCharges();
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead)
            return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            if (HasGroundContactFromTop(collision))
            {
                isGrounded = true;
                RefreshJumpCharges();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isDead)
            return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            if (groundContacts > 0)
            {
                groundContacts--;
            }

            if (groundContacts <= 0)
            {
                groundContacts = 0;
                isGrounded = false;
            }
        }
    }

    private bool HasGroundContactFromTop(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            if (contact.normal.y > 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        if (collision.CompareTag("Death"))
        {
            EnsureRuntimeReferences();

            if (playerHealth != null)
            {
                playerHealth.ApplyDeathTrapEffect();
            }
            else
            {
                Die();
            }
        }
    }

    void Die()
    {
        if (isDead)
            return;

        EnsureRuntimeReferences();

        isDead = true;
        isAttacking = false;
        isInputLocked = true;
        CancelDash();

        if (metrics != null)
        {
            metrics.RegisterDeath();
        }
        else
        {
            Debug.LogWarning("PlayerController: GameMetrics not found, death was not registered.");
        }

        PlayerProgress.Instance.SetCurrentHealth(0);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = originalGravityScale;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }

        if (animator != null && HasAnimatorParameter("Die", AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger("Die");
        }
        else
        {
            OnDeathAnimationFinished();
        }
    }

    public void OnDeathAnimationFinished()
    {
        EnsureRuntimeReferences();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverUI is not assigned on Player.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}