using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMetrics : MonoBehaviour
{
    public static GameMetrics Instance { get; private set; }

    private const string DeathsKey = "Metrics_Deaths";
    private const string HealthLostKey = "Metrics_HealthLost";
    private const string TotalCoinsCollectedKey = "Metrics_TotalCoinsCollected";
    private const string EnemiesKilledKey = "Metrics_EnemiesKilled";
    private const string LastCompletedLevelTimeKey = "Metrics_LastCompletedLevelTime";
    private const string LastLevelCompletedKey = "Metrics_LastLevelCompleted";

    [Header("Global Metrics")]
    [SerializeField] private int deaths = 0;
    [SerializeField] private int totalHealthLost = 0;
    [SerializeField] private int totalCoinsCollected = 0;
    [SerializeField] private int enemiesKilled = 0;

    [Header("Runtime Metrics")]
    [SerializeField] private float timeAlive = 0f;
    [SerializeField] private float currentLevelTime = 0f;

    [Header("Last Completed Level Result")]
    [SerializeField] private float lastCompletedLevelTime = 0f;
    [SerializeField] private bool lastLevelCompleted = false;

    [Header("Difficulty Debug")]
    [SerializeField] private DynamicDifficultyManager difficultyManager;

    public int Deaths
    {
        get { return deaths; }
    }

    public int TotalHealthLost
    {
        get { return totalHealthLost; }
    }

    public int TotalCoinsCollected
    {
        get { return totalCoinsCollected; }
    }

    public int EnemiesKilled
    {
        get { return enemiesKilled; }
    }

    public int CurrentPlayerCoins
    {
        get { return PlayerProgress.Instance.TotalCoins; }
    }

    public float TimeAlive
    {
        get { return timeAlive; }
    }

    public float CurrentLevelTime
    {
        get { return currentLevelTime; }
    }

    public float LastCompletedLevelTime
    {
        get { return lastCompletedLevelTime; }
    }

    public bool LastLevelCompleted
    {
        get { return lastLevelCompleted; }
    }

    public DynamicDifficultyManager.DifficultyTier CurrentDifficulty
    {
        get
        {
            if (difficultyManager == null)
            {
                difficultyManager = FindFirstObjectByType<DynamicDifficultyManager>();
            }

            if (difficultyManager == null)
                return DynamicDifficultyManager.DifficultyTier.Normal;

            return difficultyManager.CurrentDifficulty;
        }
    }

    public string CurrentDifficultyString
    {
        get { return CurrentDifficulty.ToString(); }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadMetrics();
    }

    private void Start()
    {
        if (difficultyManager == null)
        {
            difficultyManager = FindFirstObjectByType<DynamicDifficultyManager>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;
        currentLevelTime += Time.deltaTime;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentLevelTime = 0f;

        if (difficultyManager == null)
        {
            difficultyManager = FindFirstObjectByType<DynamicDifficultyManager>();
        }
    }

    private void LoadMetrics()
    {
        deaths = PlayerPrefs.GetInt(DeathsKey, 0);
        totalHealthLost = PlayerPrefs.GetInt(HealthLostKey, 0);
        totalCoinsCollected = PlayerPrefs.GetInt(TotalCoinsCollectedKey, 0);
        enemiesKilled = PlayerPrefs.GetInt(EnemiesKilledKey, 0);
        lastCompletedLevelTime = PlayerPrefs.GetFloat(LastCompletedLevelTimeKey, 0f);
        lastLevelCompleted = PlayerPrefs.GetInt(LastLevelCompletedKey, 0) == 1;
    }

    private void SaveMetrics()
    {
        PlayerPrefs.SetInt(DeathsKey, deaths);
        PlayerPrefs.SetInt(HealthLostKey, totalHealthLost);
        PlayerPrefs.SetInt(TotalCoinsCollectedKey, totalCoinsCollected);
        PlayerPrefs.SetInt(EnemiesKilledKey, enemiesKilled);
        PlayerPrefs.SetFloat(LastCompletedLevelTimeKey, lastCompletedLevelTime);
        PlayerPrefs.SetInt(LastLevelCompletedKey, lastLevelCompleted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void RegisterDeath()
    {
        deaths++;
        SaveMetrics();
        Debug.Log("GameMetrics -> Deaths: " + deaths);
    }

    public void RegisterCoin()
    {
        totalCoinsCollected++;
        SaveMetrics();
        Debug.Log("GameMetrics -> Total Coins Collected: " + totalCoinsCollected);
    }

    public void RegisterEnemyKilled()
    {
        enemiesKilled++;
        SaveMetrics();
        Debug.Log("GameMetrics -> Enemies Killed: " + enemiesKilled);
    }

    public void RegisterHealthLost(int amount)
    {
        if (amount <= 0)
            return;

        totalHealthLost += amount;
        SaveMetrics();
        Debug.Log("GameMetrics -> Total Health Lost: " + totalHealthLost);
    }

    public void RegisterLevelCompleted()
    {
        lastLevelCompleted = true;
        lastCompletedLevelTime = currentLevelTime;
        SaveMetrics();

        Debug.Log(
            "GameMetrics -> Last Level Completed. Time: " +
            lastCompletedLevelTime.ToString("0.00") + " sec"
        );
    }

    public void RegisterLevelFailed()
    {
        lastLevelCompleted = false;
        SaveMetrics();
        Debug.Log("GameMetrics -> Last Level Result marked as failed.");
    }

    public void ResetAllMetrics()
    {
        deaths = 0;
        totalHealthLost = 0;
        totalCoinsCollected = 0;
        enemiesKilled = 0;
        timeAlive = 0f;
        currentLevelTime = 0f;
        lastCompletedLevelTime = 0f;
        lastLevelCompleted = false;

        PlayerPrefs.DeleteKey(DeathsKey);
        PlayerPrefs.DeleteKey(HealthLostKey);
        PlayerPrefs.DeleteKey(TotalCoinsCollectedKey);
        PlayerPrefs.DeleteKey(EnemiesKilledKey);
        PlayerPrefs.DeleteKey(LastCompletedLevelTimeKey);
        PlayerPrefs.DeleteKey(LastLevelCompletedKey);
        PlayerPrefs.Save();

        Debug.Log("GameMetrics -> All metrics reset.");
    }

    public void ResetRuntimeMetrics()
    {
        timeAlive = 0f;
        currentLevelTime = 0f;
        lastCompletedLevelTime = 0f;
        lastLevelCompleted = false;
    }
}