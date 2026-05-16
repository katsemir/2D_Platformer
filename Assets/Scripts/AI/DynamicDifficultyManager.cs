using UnityEngine;

public class DynamicDifficultyManager : MonoBehaviour
{
    public enum DifficultyTier
    {
        Easy,
        Normal,
        Hard
    }

    public static DynamicDifficultyManager Instance
    {
        get
        {
            if (instance == null)
            {
                DynamicDifficultyManager existing = FindFirstObjectByType<DynamicDifficultyManager>();

                if (existing != null)
                {
                    instance = existing;
                }
                else
                {
                    GameObject ddaObject = new GameObject("DynamicDifficultyManager");
                    instance = ddaObject.AddComponent<DynamicDifficultyManager>();
                }
            }

            return instance;
        }
    }

    private static DynamicDifficultyManager instance;

    [Header("References")]
    public GameMetrics metrics;

    [Header("Difficulty State")]
    [SerializeField] private DifficultyTier currentDifficulty = DifficultyTier.Normal;

    [Header("Auto Evaluation")]
    public bool autoEvaluate = true;
    public float evaluationInterval = 2f;

    [Header("Normalization Limits")]
    public float maxDeathsForNormalization = 5f;
    public float maxHealthLostForNormalization = 15f;
    public float maxCoinsForNormalization = 20f;
    public float maxEnemiesKilledForNormalization = 5f;
    public float bestLevelTime = 120f;
    public float worstLevelTime = 180f;

    [Header("Model Weights")]
    [Range(0f, 1f)] public float deathsWeight = 0.30f;
    [Range(0f, 1f)] public float healthLostWeight = 0.25f;
    [Range(0f, 1f)] public float coinsWeight = 0.15f;
    [Range(0f, 1f)] public float enemiesKilledWeight = 0.15f;
    [Range(0f, 1f)] public float levelCompletedWeight = 0.05f;
    [Range(0f, 1f)] public float levelTimeWeight = 0.10f;

    [Header("Score Smoothing")]
    [Range(0f, 1f)] public float rawScoreInfluence = 0.7f;

    [Header("Difficulty Thresholds")]
    [Range(0f, 1f)] public float easyThreshold = 0.33f;
    [Range(0f, 1f)] public float hardThreshold = 0.66f;

    [Header("Current Debug Values")]
    [SerializeField] private float normalizedDeaths = 0f;
    [SerializeField] private float normalizedHealthLost = 0f;
    [SerializeField] private float normalizedCoins = 0f;
    [SerializeField] private float normalizedEnemiesKilled = 0f;
    [SerializeField] private float normalizedLevelTime = 0f;
    [SerializeField] private float normalizedLevelCompleted = 0f;
    [SerializeField] private float rawScore = 0.5f;
    [SerializeField] private float smoothedScore = 0.5f;

    [Header("Platform Multipliers")]
    public float easyPlatformSpeedMultiplier = 0.5f;
    public float normalPlatformSpeedMultiplier = 1f;
    public float hardPlatformSpeedMultiplier = 1.5f;

    [Header("Enemy Movement")]
    public float easyEnemyMoveSpeedMultiplier = 0.5f;
    public float normalEnemyMoveSpeedMultiplier = 1f;
    public float hardEnemyMoveSpeedMultiplier = 1.5f;

    [Header("Enemy Aggro Range")]
    public float easyEnemyAggroRangeMultiplier = 0.5f;
    public float normalEnemyAggroRangeMultiplier = 1f;
    public float hardEnemyAggroRangeMultiplier = 1.5f;

    [Header("Enemy Melee Cooldown")]
    public float easyEnemyMeleeCooldownMultiplier = 1.25f;
    public float normalEnemyMeleeCooldownMultiplier = 1f;
    public float hardEnemyMeleeCooldownMultiplier = 0.75f;

    [Header("Player Easy Assist")]
    public float easyInvulnerabilityDuration = 1f;
    public bool easyDeathTrapDealsDamageInsteadOfInstantDeath = true;

    private float evaluationTimer = 0f;

    public DifficultyTier CurrentDifficulty
    {
        get { return currentDifficulty; }
    }

    public float RawScore
    {
        get { return rawScore; }
    }

    public float SmoothedScore
    {
        get { return smoothedScore; }
    }

    public float NormalizedDeaths
    {
        get { return normalizedDeaths; }
    }

    public float NormalizedHealthLost
    {
        get { return normalizedHealthLost; }
    }

    public float NormalizedCoins
    {
        get { return normalizedCoins; }
    }

    public float NormalizedEnemiesKilled
    {
        get { return normalizedEnemiesKilled; }
    }

    public float NormalizedLevelTime
    {
        get { return normalizedLevelTime; }
    }

    public float NormalizedLevelCompleted
    {
        get { return normalizedLevelCompleted; }
    }

    public float PlatformSpeedMultiplier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyTier.Easy:
                    return easyPlatformSpeedMultiplier;
                case DifficultyTier.Hard:
                    return hardPlatformSpeedMultiplier;
                default:
                    return normalPlatformSpeedMultiplier;
            }
        }
    }

    public float EnemyMoveSpeedMultiplier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyTier.Easy:
                    return easyEnemyMoveSpeedMultiplier;
                case DifficultyTier.Hard:
                    return hardEnemyMoveSpeedMultiplier;
                default:
                    return normalEnemyMoveSpeedMultiplier;
            }
        }
    }

    public float EnemyAggroRangeMultiplier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyTier.Easy:
                    return easyEnemyAggroRangeMultiplier;
                case DifficultyTier.Hard:
                    return hardEnemyAggroRangeMultiplier;
                default:
                    return normalEnemyAggroRangeMultiplier;
            }
        }
    }

    public float EnemyMeleeCooldownMultiplier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyTier.Easy:
                    return easyEnemyMeleeCooldownMultiplier;
                case DifficultyTier.Hard:
                    return hardEnemyMeleeCooldownMultiplier;
                default:
                    return normalEnemyMeleeCooldownMultiplier;
            }
        }
    }

    public bool IsEnemyRangedAttackEnabled
    {
        get { return currentDifficulty != DifficultyTier.Easy; }
    }

    public bool IsEnemyContactDamageEnabled
    {
        get { return currentDifficulty != DifficultyTier.Easy; }
    }

    public int EnemyMeleeDamageBonus
    {
        get { return currentDifficulty == DifficultyTier.Hard ? 1 : 0; }
    }

    public int EnemyProjectileDamageBonus
    {
        get { return currentDifficulty == DifficultyTier.Hard ? 1 : 0; }
    }

    public float PlayerAdditionalInvulnerabilityDuration
    {
        get { return currentDifficulty == DifficultyTier.Easy ? easyInvulnerabilityDuration : 0f; }
    }

    public bool DeathTrapDealsDamageInsteadOfInstantDeath
    {
        get
        {
            return currentDifficulty == DifficultyTier.Easy &&
                   easyDeathTrapDealsDamageInsteadOfInstantDeath;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (metrics == null)
        {
            metrics = GameMetrics.Instance;
        }

        EvaluateDifficulty();
    }

    private void Update()
    {
        if (!autoEvaluate)
            return;

        evaluationTimer += Time.deltaTime;

        if (evaluationTimer >= evaluationInterval)
        {
            evaluationTimer = 0f;
            EvaluateDifficulty();
        }
    }

    public void EvaluateDifficulty()
    {
        if (metrics == null)
        {
            metrics = GameMetrics.Instance;
        }

        if (metrics == null)
            return;

        normalizedDeaths = NormalizeDirect(metrics.Deaths, maxDeathsForNormalization);
        normalizedHealthLost = NormalizeDirect(metrics.TotalHealthLost, maxHealthLostForNormalization);
        normalizedCoins = NormalizeDirect(metrics.TotalCoinsCollected, maxCoinsForNormalization);
        normalizedEnemiesKilled = NormalizeDirect(metrics.EnemiesKilled, maxEnemiesKilledForNormalization);
        normalizedLevelTime = NormalizeInverse(metrics.LastCompletedLevelTime, bestLevelTime, worstLevelTime);
        normalizedLevelCompleted = metrics.LastLevelCompleted ? 1f : 0f;

        if (!HasAnyPlayerPerformanceData())
        {
            rawScore = 0.5f;
            smoothedScore = 0.5f;
            currentDifficulty = DifficultyTier.Normal;

            Debug.Log(
                "DDA -> Difficulty: " + currentDifficulty +
                " | RawScore: " + rawScore.ToString("0.000") +
                " | SmoothedScore: " + smoothedScore.ToString("0.000") +
                " | No player data"
            );

            return;
        }

        float totalWeight =
            deathsWeight +
            healthLostWeight +
            coinsWeight +
            enemiesKilledWeight +
            levelCompletedWeight +
            levelTimeWeight;

        if (totalWeight <= 0f)
        {
            totalWeight = 1f;
        }

        rawScore =
            coinsWeight * normalizedCoins +
            enemiesKilledWeight * normalizedEnemiesKilled +
            levelTimeWeight * normalizedLevelTime +
            levelCompletedWeight * normalizedLevelCompleted +
            deathsWeight * (1f - normalizedDeaths) +
            healthLostWeight * (1f - normalizedHealthLost);

        rawScore /= totalWeight;
        rawScore = Mathf.Clamp01(rawScore);

        smoothedScore = Mathf.Clamp01(
            rawScoreInfluence * rawScore +
            (1f - rawScoreInfluence) * smoothedScore
        );

        if (smoothedScore < easyThreshold)
        {
            currentDifficulty = DifficultyTier.Easy;
        }
        else if (smoothedScore >= hardThreshold)
        {
            currentDifficulty = DifficultyTier.Hard;
        }
        else
        {
            currentDifficulty = DifficultyTier.Normal;
        }

        Debug.Log(
            "DDA -> Difficulty: " + currentDifficulty +
            " | RawScore: " + rawScore.ToString("0.000") +
            " | SmoothedScore: " + smoothedScore.ToString("0.000")
        );
    }

    public void ForceDifficulty(DifficultyTier difficulty)
    {
        currentDifficulty = difficulty;
        Debug.Log("DDA -> Forced Difficulty: " + currentDifficulty);
    }

    public void ResetModelState()
    {
        normalizedDeaths = 0f;
        normalizedHealthLost = 0f;
        normalizedCoins = 0f;
        normalizedEnemiesKilled = 0f;
        normalizedLevelTime = 0f;
        normalizedLevelCompleted = 0f;
        rawScore = 0.5f;
        smoothedScore = 0.5f;
        currentDifficulty = DifficultyTier.Normal;
        evaluationTimer = 0f;
    }

    private bool HasAnyPlayerPerformanceData()
    {
        if (metrics == null)
            return false;

        return metrics.Deaths > 0 ||
               metrics.TotalHealthLost > 0 ||
               metrics.TotalCoinsCollected > 0 ||
               metrics.EnemiesKilled > 0 ||
               metrics.LastCompletedLevelTime > 0f ||
               metrics.LastLevelCompleted;
    }

    private float NormalizeDirect(float value, float maxValue)
    {
        if (maxValue <= 0f)
            return 0f;

        return Mathf.Clamp01(value / maxValue);
    }

    private float NormalizeInverse(float value, float minGoodValue, float maxBadValue)
    {
        if (value <= 0f)
            return 0f;

        if (maxBadValue <= minGoodValue)
            return 0f;

        if (value <= minGoodValue)
            return 1f;

        if (value >= maxBadValue)
            return 0f;

        return 1f - ((value - minGoodValue) / (maxBadValue - minGoodValue));
    }
}