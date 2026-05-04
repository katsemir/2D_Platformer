using UnityEngine;

public class BossEnemy : EnemyBase
{
    [Header("Target")]
    public Transform player;

    [Header("Combat")]
    public float aggroRange = 15f;
    public float castCooldown = 3f;

    [Header("Spell")]
    public GameObject spellPrefab;
    public BossSpellPoint[] spellPoints;
    public int spellDamage = 1;
    public float spellLifeTime = 2f;

    [Header("Cast Timing")]
    public float spellSpawnDelay = 0.5f;
    public float castDuration = 1.2f;

    [Header("Adaptive Boss Stats")]
    public int easyHealth = 6;
    public int normalHealth = 9;
    public int hardHealth = 12;

    public int easySpellDamage = 1;
    public int normalSpellDamage = 1;
    public int hardSpellDamage = 2;

    [Header("Contact Damage")]
    public int contactDamage = 1;
    public float contactDamageCooldown = 1f;

    [Header("Physics")]
    public float gravityScale = 3f;

    [Header("Difficulty")]
    public DynamicDifficultyManager difficultyManager;

    [Header("Facing")]
    public bool faceRightByDefault = true;

    [Header("Victory Screen")]
    public VictoryScreen victoryScreen;

    private float lastCastTime = -10f;
    private float lastContactDamageTime = -10f;

    private bool isBusy = false;
    private bool spellSpawnedDuringCurrentCast = false;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private BossSpellPoint currentSpellPoint;

    protected override void Start()
    {
        if (difficultyManager == null)
        {
            difficultyManager = FindFirstObjectByType<DynamicDifficultyManager>();
        }

        if (difficultyManager == null)
        {
            difficultyManager = DynamicDifficultyManager.Instance;
        }

        ApplyDifficultyStats();

        base.Start();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (victoryScreen == null)
        {
            victoryScreen = FindFirstObjectByType<VictoryScreen>();
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = gravityScale;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Update()
    {
        ApplyDifficultyStats();

        if (isDead || isBusy)
            return;

        if (player == null)
            return;

        FacePlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > aggroRange)
            return;

        if (Time.time < lastCastTime + castCooldown)
            return;

        BossSpellPoint pointWithPlayer = GetPointWherePlayerStands();

        if (pointWithPlayer != null)
        {
            StartCast(pointWithPlayer);
        }
    }

    private void ApplyDifficultyStats()
    {
        if (difficultyManager == null)
        {
            maxHealth = normalHealth;
            spellDamage = normalSpellDamage;
            return;
        }

        switch (difficultyManager.CurrentDifficulty)
        {
            case DynamicDifficultyManager.DifficultyTier.Easy:
                maxHealth = easyHealth;
                spellDamage = easySpellDamage;
                break;

            case DynamicDifficultyManager.DifficultyTier.Hard:
                maxHealth = hardHealth;
                spellDamage = hardSpellDamage;
                break;

            default:
                maxHealth = normalHealth;
                spellDamage = normalSpellDamage;
                break;
        }

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    private void FacePlayer()
    {
        if (player == null || spriteRenderer == null)
            return;

        bool playerIsRight = player.position.x > transform.position.x;

        if (faceRightByDefault)
        {
            spriteRenderer.flipX = !playerIsRight;
        }
        else
        {
            spriteRenderer.flipX = playerIsRight;
        }
    }

    private BossSpellPoint GetPointWherePlayerStands()
    {
        if (spellPoints == null || spellPoints.Length == 0)
            return null;

        for (int i = 0; i < spellPoints.Length; i++)
        {
            if (spellPoints[i] != null && spellPoints[i].playerInside)
            {
                return spellPoints[i];
            }
        }

        return null;
    }

    private void StartCast(BossSpellPoint targetPoint)
    {
        if (targetPoint == null)
            return;

        if (spellPrefab == null)
        {
            Debug.LogWarning("BossEnemy: Spell Prefab is not assigned.");
            return;
        }

        isBusy = true;
        spellSpawnedDuringCurrentCast = false;
        currentSpellPoint = targetPoint;
        lastCastTime = Time.time;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        if (animator != null)
        {
            animator.SetTrigger("Cast");
        }

        Invoke(nameof(SpawnSpellFromTimer), spellSpawnDelay);
        Invoke(nameof(EndCast), castDuration);
    }

    private void SpawnSpellFromTimer()
    {
        if (spellSpawnedDuringCurrentCast)
            return;

        SpawnSpell();
    }

    public void SpawnSpell()
    {
        if (isDead || spellSpawnedDuringCurrentCast || currentSpellPoint == null)
            return;

        if (spellPrefab == null)
            return;

        spellSpawnedDuringCurrentCast = true;

        Vector3 spawnPosition = currentSpellPoint.transform.position;
        spawnPosition.z = 0f;

        GameObject spellObject = Instantiate(spellPrefab, spawnPosition, Quaternion.identity);
        spellObject.SetActive(true);

        Vector3 spellScale = spellObject.transform.localScale;

        if (Mathf.Abs(spellScale.x) < 0.01f || Mathf.Abs(spellScale.y) < 0.01f)
        {
            spellObject.transform.localScale = Vector3.one;
        }

        BossSpell bossSpell = spellObject.GetComponent<BossSpell>();

        if (bossSpell != null)
        {
            bossSpell.Initialize(spellDamage, spellLifeTime);
        }
        else
        {
            Debug.LogWarning("BossEnemy: BossSpell component is missing on Spell Prefab.");
        }
    }

    public void EndCast()
    {
        if (isDead)
            return;

        isBusy = false;
        currentSpellPoint = null;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead)
            return;

        if (difficultyManager != null && !difficultyManager.IsEnemyContactDamageEnabled)
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

    public override void TakeDamage(int damage)
    {
        if (isDead)
            return;

        base.TakeDamage(damage);
    }

    protected override void OnHit()
    {
        isBusy = true;

        CancelInvoke(nameof(SpawnSpellFromTimer));
        CancelInvoke(nameof(EndCast));

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        Invoke(nameof(EndHurt), 0.4f);
    }

    public void EndHurt()
    {
        if (isDead)
            return;

        isBusy = false;
        currentSpellPoint = null;
    }

    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isBusy = true;

        CancelInvoke();
        RegisterEnemyKilledMetric();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        Collider2D bossCollider = GetComponent<Collider2D>();

        if (bossCollider != null)
        {
            bossCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        else
        {
            FinishDeath();
        }
    }

    public void FinishDeath()
    {
        if (victoryScreen != null)
        {
            victoryScreen.ShowVictoryScreen();
        }
        else
        {
            Debug.LogWarning("BossEnemy: VictoryScreen не знайдено в сцені.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        if (spellPoints == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < spellPoints.Length; i++)
        {
            if (spellPoints[i] != null)
            {
                Gizmos.DrawWireSphere(spellPoints[i].transform.position, 0.3f);
            }
        }
    }
}