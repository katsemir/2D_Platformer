using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance
    {
        get
        {
            if (instance == null)
            {
                PlayerProgress existing = FindFirstObjectByType<PlayerProgress>();

                if (existing != null)
                {
                    instance = existing;
                }
                else
                {
                    GameObject progressObject = new GameObject("PlayerProgress");
                    instance = progressObject.AddComponent<PlayerProgress>();
                }
            }

            return instance;
        }
    }

    private static PlayerProgress instance;

    private const string TotalCoinsKey = "Progress_TotalCoins";
    private const string DamageLevelKey = "Progress_DamageLevel";
    private const string SpeedLevelKey = "Progress_SpeedLevel";
    private const string DoubleJumpUnlockedKey = "Progress_DoubleJumpUnlocked";
    private const string DashUnlockedKey = "Progress_DashUnlocked";
    private const string CurrentHealthKey = "Progress_CurrentHealth";
    private const string HealthInitializedKey = "Progress_HealthInitialized";

    [Header("Saved Progress")]
    [SerializeField] private int totalCoins = 0;
    [SerializeField] private int damageLevel = 0;
    [SerializeField] private int speedLevel = 0;
    [SerializeField] private bool doubleJumpUnlocked = false;
    [SerializeField] private bool dashUnlocked = false;
    [SerializeField] private int currentHealth = 3;

    public int TotalCoins
    {
        get { return totalCoins; }
    }

    public int DamageLevel
    {
        get { return damageLevel; }
    }

    public int SpeedLevel
    {
        get { return speedLevel; }
    }

    public bool DoubleJumpUnlocked
    {
        get { return doubleJumpUnlocked; }
    }

    public bool DashUnlocked
    {
        get { return dashUnlocked; }
    }

    public int CurrentHealth
    {
        get { return currentHealth; }
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
        LoadProgress();
    }

    public void LoadProgress()
    {
        totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
        damageLevel = PlayerPrefs.GetInt(DamageLevelKey, 0);
        speedLevel = PlayerPrefs.GetInt(SpeedLevelKey, 0);
        doubleJumpUnlocked = PlayerPrefs.GetInt(DoubleJumpUnlockedKey, 0) == 1;
        dashUnlocked = PlayerPrefs.GetInt(DashUnlockedKey, 0) == 1;
        currentHealth = PlayerPrefs.GetInt(CurrentHealthKey, 3);
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.SetInt(DamageLevelKey, damageLevel);
        PlayerPrefs.SetInt(SpeedLevelKey, speedLevel);
        PlayerPrefs.SetInt(DoubleJumpUnlockedKey, doubleJumpUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(DashUnlockedKey, dashUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(CurrentHealthKey, currentHealth);
        PlayerPrefs.Save();
    }

    public void ResetAllProgress()
    {
        totalCoins = 0;
        damageLevel = 0;
        speedLevel = 0;
        doubleJumpUnlocked = false;
        dashUnlocked = false;
        currentHealth = 3;

        PlayerPrefs.DeleteKey(TotalCoinsKey);
        PlayerPrefs.DeleteKey(DamageLevelKey);
        PlayerPrefs.DeleteKey(SpeedLevelKey);
        PlayerPrefs.DeleteKey(DoubleJumpUnlockedKey);
        PlayerPrefs.DeleteKey(DashUnlockedKey);
        PlayerPrefs.DeleteKey(CurrentHealthKey);
        PlayerPrefs.DeleteKey(HealthInitializedKey);
        PlayerPrefs.Save();
    }

    public void InitializeHealthIfNeeded(int defaultHealth)
    {
        bool isInitialized = PlayerPrefs.GetInt(HealthInitializedKey, 0) == 1;

        if (isInitialized)
        {
            currentHealth = PlayerPrefs.GetInt(CurrentHealthKey, defaultHealth);
            return;
        }

        currentHealth = defaultHealth;
        PlayerPrefs.SetInt(CurrentHealthKey, currentHealth);
        PlayerPrefs.SetInt(HealthInitializedKey, 1);
        PlayerPrefs.Save();
    }

    public void SetCurrentHealth(int health)
    {
        currentHealth = Mathf.Max(0, health);
        PlayerPrefs.SetInt(CurrentHealthKey, currentHealth);
        PlayerPrefs.Save();
    }

    public void FullHeal(int maxHealth)
    {
        currentHealth = maxHealth;
        PlayerPrefs.SetInt(CurrentHealthKey, currentHealth);
        PlayerPrefs.Save();
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        totalCoins += amount;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.Save();
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
            return false;

        if (totalCoins < amount)
            return false;

        totalCoins -= amount;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.Save();
        return true;
    }

    public bool CanAfford(int amount)
    {
        return totalCoins >= amount;
    }

    public bool UpgradeDamage()
    {
        if (damageLevel >= 2)
            return false;

        damageLevel++;
        PlayerPrefs.SetInt(DamageLevelKey, damageLevel);
        PlayerPrefs.Save();
        return true;
    }

    public bool UpgradeSpeed()
    {
        if (speedLevel >= 2)
            return false;

        speedLevel++;
        PlayerPrefs.SetInt(SpeedLevelKey, speedLevel);
        PlayerPrefs.Save();
        return true;
    }

    public bool UnlockDoubleJump()
    {
        if (doubleJumpUnlocked)
            return false;

        doubleJumpUnlocked = true;
        PlayerPrefs.SetInt(DoubleJumpUnlockedKey, 1);
        PlayerPrefs.Save();
        return true;
    }

    public bool UnlockDash()
    {
        if (dashUnlocked)
            return false;

        dashUnlocked = true;
        PlayerPrefs.SetInt(DashUnlockedKey, 1);
        PlayerPrefs.Save();
        return true;
    }
}