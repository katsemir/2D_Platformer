using UnityEngine;

public class MushroomEnemy : EnemyBase
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float aggroRange = 12f;
    public float meleeRange = 6f;
    public float rangedRange = 12f;
    public float stopDistance = 1f;

    [Header("Combat")]
    public int contactDamage = 1;
    public float attackCooldown = 2f;
    public Transform attackPoint;
    public float meleeAttackRadius = 1f;
    public LayerMask playerLayer;
    public float contactDamageCooldown = 1f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 8f;
    public int projectileDamage = 1;

    [Header("Difficulty")]
    public DynamicDifficultyManager difficultyManager;

    private float lastAttackTime = -10f;
    private float lastContactDamageTime = -10f;
    private bool facingRight = true;
    private bool isBusy = false;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector3 attackPointStartLocalPosition;
    private Vector3 projectileSpawnPointStartLocalPosition;

    private float baseMoveSpeed;
    private float baseAggroRange;
    private float baseAttackCooldown;
    private int baseContactDamage;
    private int baseProjectileDamage;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GetDifficultyManager();

        if (attackPoint != null)
        {
            attackPointStartLocalPosition = attackPoint.localPosition;
        }

        if (projectileSpawnPoint != null)
        {
            projectileSpawnPointStartLocalPosition = projectileSpawnPoint.localPosition;
        }

        baseMoveSpeed = moveSpeed;
        baseAggroRange = aggroRange;
        baseAttackCooldown = attackCooldown;
        baseContactDamage = contactDamage;
        baseProjectileDamage = projectileDamage;
    }

    private DynamicDifficultyManager GetDifficultyManager()
    {
        if (difficultyManager == null)
        {
            difficultyManager = DynamicDifficultyManager.Instance;
        }

        return difficultyManager;
    }

    void Update()
    {
        ApplyDifficultyStats();

        if (isDead || isBusy)
        {
            UpdateAnimator();
            return;
        }

        if (player == null)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            UpdateAnimator();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > aggroRange)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            UpdateAnimator();
            return;
        }

        FacePlayer();

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (distanceToPlayer <= meleeRange)
            {
                StartMeleeAttack();
                UpdateAnimator();
                return;
            }

            DynamicDifficultyManager dda = GetDifficultyManager();
            bool rangedAttackEnabled = dda == null || dda.IsEnemyRangedAttackEnabled;

            if (rangedAttackEnabled && distanceToPlayer <= rangedRange)
            {
                StartRangedAttack();
                UpdateAnimator();
                return;
            }
        }

        if (rb != null)
        {
            if (distanceToPlayer > stopDistance)
            {
                float dir = Mathf.Sign(player.position.x - transform.position.x);
                rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }

        UpdateAnimator();
    }

    void ApplyDifficultyStats()
    {
        DynamicDifficultyManager dda = GetDifficultyManager();

        if (dda == null)
        {
            moveSpeed = baseMoveSpeed;
            aggroRange = baseAggroRange;
            attackCooldown = baseAttackCooldown;
            contactDamage = baseContactDamage;
            projectileDamage = baseProjectileDamage;
            return;
        }

        moveSpeed = baseMoveSpeed * dda.EnemyMoveSpeedMultiplier;
        aggroRange = baseAggroRange * dda.EnemyAggroRangeMultiplier;
        attackCooldown = baseAttackCooldown * dda.EnemyMeleeCooldownMultiplier;
        contactDamage = baseContactDamage + dda.EnemyMeleeDamageBonus;
        projectileDamage = baseProjectileDamage + dda.EnemyProjectileDamageBonus;
    }

    void UpdateAnimator()
    {
        if (animator == null || rb == null)
            return;

        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsDead", isDead);
    }

    void FacePlayer()
    {
        if (player == null || spriteRenderer == null)
            return;

        if (player.position.x > transform.position.x)
        {
            facingRight = true;
            spriteRenderer.flipX = false;
            UpdateAttackPointDirection();
            UpdateProjectileSpawnPointDirection();
        }
        else
        {
            facingRight = false;
            spriteRenderer.flipX = true;
            UpdateAttackPointDirection();
            UpdateProjectileSpawnPointDirection();
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

    void UpdateProjectileSpawnPointDirection()
    {
        if (projectileSpawnPoint == null)
            return;

        Vector3 localPos = projectileSpawnPointStartLocalPosition;
        localPos.x = facingRight ? Mathf.Abs(localPos.x) : -Mathf.Abs(localPos.x);
        projectileSpawnPoint.localPosition = localPos;
    }

    void StartMeleeAttack()
    {
        isBusy = true;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        lastAttackTime = Time.time;

        int randomAttack = Random.Range(0, 2);

        if (animator != null)
        {
            if (randomAttack == 0)
            {
                animator.SetTrigger("Attack2");
            }
            else
            {
                animator.SetTrigger("Attack3");
            }
        }
    }

    void StartRangedAttack()
    {
        isBusy = true;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("Attack1");
        }
    }

    public void PerformMeleeAttack()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("MushroomEnemy: Attack Point is not assigned.");
            return;
        }

        Collider2D[] hits;

        if (playerLayer.value != 0)
        {
            hits = Physics2D.OverlapCircleAll(attackPoint.position, meleeAttackRadius, playerLayer);
        }
        else
        {
            hits = Physics2D.OverlapCircleAll(attackPoint.position, meleeAttackRadius);
        }

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerHealth playerHealth = hits[i].GetComponent<PlayerHealth>();

            if (playerHealth == null)
            {
                playerHealth = hits[i].GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                break;
            }
        }
    }

    public void SpawnProjectile()
    {
        DynamicDifficultyManager dda = GetDifficultyManager();
        bool rangedAttackEnabled = dda == null || dda.IsEnemyRangedAttackEnabled;

        if (!rangedAttackEnabled)
            return;

        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile Prefab is not assigned on MushroomEnemy.");
            return;
        }

        if (projectileSpawnPoint == null)
        {
            Debug.LogWarning("Projectile Spawn Point is not assigned on MushroomEnemy.");
            return;
        }

        GameObject projectileInstance = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        MushroomProjectile projectileScript = projectileInstance.GetComponent<MushroomProjectile>();

        if (projectileScript != null)
        {
            float direction = facingRight ? 1f : -1f;
            projectileScript.Initialize(direction, projectileSpeed, projectileDamage);
        }
    }

    public void EndAction()
    {
        if (isDead)
            return;

        isBusy = false;
    }

    public override void TakeDamage(int damage)
    {
        if (isDead)
            return;

        base.TakeDamage(damage);
    }

    protected override void OnHit()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        isBusy = true;

        if (animator != null)
        {
            animator.SetTrigger("TakeHit");
        }
    }

    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;
        RegisterEnemyKilledMetric();
        isBusy = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        else
        {
            FinishDeath();
        }
    }

    public void FinishDeath()
    {
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead)
            return;

        DynamicDifficultyManager dda = GetDifficultyManager();

        if (dda != null && !dda.IsEnemyContactDamageEnabled)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (Time.time < lastContactDamageTime + contactDamageCooldown)
            return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth = collision.gameObject.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
            return;

        lastContactDamageTime = Time.time;
        playerHealth.TakeDamage(contactDamage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangedRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, meleeAttackRadius);
        }
    }
}