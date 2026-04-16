using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Base Enemy Stats")]
    public int maxHealth = 1;

    protected int currentHealth;
    protected bool isDead = false;
    protected GameMetrics metrics;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        metrics = FindFirstObjectByType<GameMetrics>();

        if (metrics == null)
        {
            Debug.LogWarning("EnemyBase: GameMetrics not found in scene.");
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            OnHit();
        }
    }

    protected virtual void OnHit()
    {
    }

    protected void RegisterEnemyKilledMetric()
    {
        if (metrics != null)
        {
            metrics.RegisterEnemyKilled();
        }
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        RegisterEnemyKilledMetric();
        Destroy(gameObject);
    }

    public bool IsDead()
    {
        return isDead;
    }
}