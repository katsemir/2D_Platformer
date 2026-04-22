using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public PlayerController playerController;
    public GameMetrics metrics;
    public DynamicDifficultyManager difficultyManager;

    [Header("Invulnerability")]
    public float baseInvulnerabilityDuration = 0f;

    private int currentHealth;
    private bool isDead = false;
    private float invulnerabilityTimer = 0f;

    public int CurrentHealth
    {
        get { return currentHealth; }
    }

    public int MaxHealth
    {
        get { return maxHealth; }
    }

    public bool IsInvulnerable
    {
        get { return invulnerabilityTimer > 0f; }
    }

    void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (metrics == null)
        {
            metrics = GameMetrics.Instance;
        }

        if (difficultyManager == null)
        {
            difficultyManager = FindFirstObjectByType<DynamicDifficultyManager>();
        }

        if (metrics == null)
        {
            Debug.LogWarning("PlayerHealth: GameMetrics not found in scene.");
        }

        PlayerProgress.Instance.InitializeHealthIfNeeded(maxHealth);
        currentHealth = Mathf.Clamp(PlayerProgress.Instance.CurrentHealth, 0, maxHealth);
    }

    void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    private void EnsureMetricsReference()
    {
        if (metrics == null)
        {
            metrics = GameMetrics.Instance;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (damage <= 0)
            return;

        if (IsInvulnerable)
            return;

        EnsureMetricsReference();

        int previousHealth = currentHealth;

        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        int actualDamageTaken = previousHealth - currentHealth;

        if (actualDamageTaken > 0 && metrics != null)
        {
            metrics.RegisterHealthLost(actualDamageTaken);
        }

        PlayerProgress.Instance.SetCurrentHealth(currentHealth);
        StartInvulnerability();

        if (currentHealth <= 0)
        {
            isDead = true;

            if (metrics != null)
            {
                metrics.RegisterLevelFailed();
            }

            if (playerController != null)
            {
                playerController.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void ApplyDeathTrapEffect()
    {
        if (isDead)
            return;

        EnsureMetricsReference();

        bool shouldDamageInsteadOfInstantDeath =
            difficultyManager != null &&
            difficultyManager.DeathTrapDealsDamageInsteadOfInstantDeath;

        if (shouldDamageInsteadOfInstantDeath)
        {
            TakeDamage(1);
            return;
        }

        currentHealth = 0;
        PlayerProgress.Instance.SetCurrentHealth(0);
        isDead = true;

        if (metrics != null)
        {
            metrics.RegisterLevelFailed();
        }

        if (playerController != null)
        {
            playerController.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        invulnerabilityTimer = 0f;
        PlayerProgress.Instance.FullHeal(maxHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void StartInvulnerability()
    {
        float duration = baseInvulnerabilityDuration;

        if (difficultyManager != null)
        {
            duration += difficultyManager.PlayerAdditionalInvulnerabilityDuration;
        }

        if (duration > 0f)
        {
            invulnerabilityTimer = duration;
        }
    }
}