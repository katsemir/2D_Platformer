using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject shopPanel;

    [Header("Buttons")]
    public Button fullHealButton;
    public Button damageUpgradeButton;
    public Button speedUpgradeButton;
    public Button doubleJumpButton;
    public Button dashButton;
    public Button closeButton;

    [Header("Texts")]
    public TMP_Text coinsText;
    public TMP_Text healthText;
    public TMP_Text damageText;
    public TMP_Text speedText;
    public TMP_Text doubleJumpText;
    public TMP_Text dashText;
    public TMP_Text messageText;

    [Header("Prices")]
    public int fullHealPrice = 5;
    public int damageUpgradePrice = 10;
    public int speedUpgradePrice = 10;
    public int doubleJumpPrice = 20;
    public int dashPrice = 20;

    private PlayerController currentPlayer;
    private PlayerHealth currentPlayerHealth;
    private bool isOpen = false;

    public bool IsOpen
    {
        get { return isOpen; }
    }

    private void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        if (fullHealButton != null)
        {
            fullHealButton.onClick.RemoveAllListeners();
            fullHealButton.onClick.AddListener(BuyFullHeal);
        }

        if (damageUpgradeButton != null)
        {
            damageUpgradeButton.onClick.RemoveAllListeners();
            damageUpgradeButton.onClick.AddListener(BuyDamageUpgrade);
        }

        if (speedUpgradeButton != null)
        {
            speedUpgradeButton.onClick.RemoveAllListeners();
            speedUpgradeButton.onClick.AddListener(BuySpeedUpgrade);
        }

        if (doubleJumpButton != null)
        {
            doubleJumpButton.onClick.RemoveAllListeners();
            doubleJumpButton.onClick.AddListener(BuyDoubleJump);
        }

        if (dashButton != null)
        {
            dashButton.onClick.RemoveAllListeners();
            dashButton.onClick.AddListener(BuyDash);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseShop);
        }

        ClearMessage();
        RefreshUI();
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    public void OpenShop(PlayerController player)
    {
        currentPlayer = player;

        if (currentPlayer != null)
        {
            currentPlayerHealth = currentPlayer.GetComponent<PlayerHealth>();

            if (currentPlayerHealth == null)
            {
                currentPlayerHealth = currentPlayer.GetComponentInChildren<PlayerHealth>();
            }

            currentPlayer.SetInputLocked(true);
        }
        else
        {
            currentPlayerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        isOpen = true;

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        ClearMessage();
        RefreshUI();
    }

    public void CloseShop()
    {
        isOpen = false;

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        if (currentPlayer != null)
        {
            currentPlayer.SetInputLocked(false);
        }

        currentPlayer = null;
        currentPlayerHealth = null;
        ClearMessage();
    }

    public void BuyFullHeal()
    {
        if (currentPlayerHealth == null)
        {
            ShowMessage("PlayerHealth not found");
            return;
        }

        if (currentPlayerHealth.CurrentHealth >= currentPlayerHealth.MaxHealth)
        {
            ShowMessage("HP already full");
            RefreshUI();
            return;
        }

        if (!PlayerProgress.Instance.CanAfford(fullHealPrice))
        {
            ShowMessage("Not enough coins");
            RefreshUI();
            return;
        }

        bool spent = PlayerProgress.Instance.SpendCoins(fullHealPrice);

        if (!spent)
        {
            ShowMessage("Not enough coins");
            RefreshUI();
            return;
        }

        currentPlayerHealth.RestoreFullHealth();
        ShowMessage("Full heal purchased");
        RefreshUI();
    }

    public void BuyDamageUpgrade()
    {
        if (PlayerProgress.Instance.DamageLevel >= 2)
        {
            ShowMessage("Damage is max");
            RefreshUI();
            return;
        }

        if (!PlayerProgress.Instance.CanAfford(damageUpgradePrice))
        {
            ShowMessage("Not enough coins");
            RefreshUI();
            return;
        }

        bool spent = PlayerProgress.Instance.SpendCoins(damageUpgradePrice);

        if (!spent)
        {
            ShowMessage("Not enough coins");
            RefreshUI();
            return;
        }

        bool upgraded = PlayerProgress.Instance.UpgradeDamage();

        if (!upgraded)
        {
            PlayerProgress.Instance.AddCoins(damageUpgradePrice);
            ShowMessage("Damage is max");
            RefreshUI();
            return;
        }

        if (currentPlayer != null)
        {
            currentPlayer.ApplyProgressStats();
        }

        ShowMessage("Damage upgraded");
        RefreshUI();
    }

    public void BuySpeedUpgrade()
    {
        if (PlayerProgress.Instance.SpeedLevel >= 2)
        {
            ShowMessage("Speed is max");
            RefreshUI();
            return;
        }

        if (!PlayerProgress.Instance.CanAfford(speedUpgradePrice))
        {
            ShowMessage("Not enough coins");
            RefreshUI();
            return;
        }

        bool spent = PlayerProgress.Instance.SpendCoins(speedUpgradePrice);

        if (!spent)
        {
            ShowMessage("Not enough coins");
            RefreshUI();
            return;
        }

        bool upgraded = PlayerProgress.Instance.UpgradeSpeed();

        if (!upgraded)
        {
            PlayerProgress.Instance.AddCoins(speedUpgradePrice);
            ShowMessage("Speed is max");
            RefreshUI();
            return;
        }

        if (currentPlayer != null)
        {
            currentPlayer.ApplyProgressStats();
        }

        ShowMessage("Speed upgraded");
        RefreshUI();
    }

    public void BuyDoubleJump()
    {
        if (PlayerProgress.Instance.DoubleJumpUnlocked)
        {
            ShowMessage("Double Jump already bought");
            RefreshUI();
            return;
        }

        if (!PlayerProgress.Instance.CanAfford(doubleJumpPrice))
        {
            ShowMessage("Not enough coins");
            RefreshUI();
            return;
        }

        bool spent = PlayerProgress.Instance.SpendCoins(doubleJumpPrice);

        if (!spent)
        {
            ShowMessage("Not enough coins");
            RefreshUI();
            return;
        }

        bool unlocked = PlayerProgress.Instance.UnlockDoubleJump();

        if (!unlocked)
        {
            PlayerProgress.Instance.AddCoins(doubleJumpPrice);
            ShowMessage("Double Jump already bought");
            RefreshUI();
            return;
        }

        ShowMessage("Double Jump unlocked");
        RefreshUI();
    }

    public void BuyDash()
    {
        if (PlayerProgress.Instance.DashUnlocked)
        {
            ShowMessage("Dash already bought");
            RefreshUI();
            return;
        }

        if (!PlayerProgress.Instance.CanAfford(dashPrice))
        {
            ShowMessage("Not enough coins");
            RefreshUI();
            return;
        }

        bool spent = PlayerProgress.Instance.SpendCoins(dashPrice);

        if (!spent)
        {
            ShowMessage("Not enough coins");
            RefreshUI();
            return;
        }

        bool unlocked = PlayerProgress.Instance.UnlockDash();

        if (!unlocked)
        {
            PlayerProgress.Instance.AddCoins(dashPrice);
            ShowMessage("Dash already bought");
            RefreshUI();
            return;
        }

        ShowMessage("Dash unlocked");
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (coinsText != null)
        {
            coinsText.text = "Coins: " + PlayerProgress.Instance.TotalCoins;
        }

        int maxHealth = 3;
        int currentHealth = PlayerProgress.Instance.CurrentHealth;

        if (currentPlayerHealth != null)
        {
            maxHealth = currentPlayerHealth.MaxHealth;
            currentHealth = currentPlayerHealth.CurrentHealth;
        }

        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + " / " + maxHealth;
        }

        if (damageText != null)
        {
            int damageValue = 1 + PlayerProgress.Instance.DamageLevel;

            if (PlayerProgress.Instance.DamageLevel >= 2)
            {
                damageText.text = "Damage: " + damageValue + " (MAX)";
            }
            else
            {
                damageText.text = "Damage: " + damageValue + " -> " + (damageValue + 1) + " | Cost: " + damageUpgradePrice;
            }
        }

        if (speedText != null)
        {
            int speedLevel = PlayerProgress.Instance.SpeedLevel;

            if (speedLevel >= 2)
            {
                speedText.text = "Speed Level: " + speedLevel + " (MAX)";
            }
            else
            {
                speedText.text = "Speed Level: " + speedLevel + " -> " + (speedLevel + 1) + " | Cost: " + speedUpgradePrice;
            }
        }

        if (doubleJumpText != null)
        {
            doubleJumpText.text = PlayerProgress.Instance.DoubleJumpUnlocked
                ? "Double Jump: BOUGHT"
                : "Double Jump: Cost " + doubleJumpPrice;
        }

        if (dashText != null)
        {
            dashText.text = PlayerProgress.Instance.DashUnlocked
                ? "Dash: BOUGHT"
                : "Dash: Cost " + dashPrice;
        }

        if (fullHealButton != null)
        {
            bool canHeal = currentHealth < maxHealth && PlayerProgress.Instance.CanAfford(fullHealPrice);
            fullHealButton.interactable = canHeal;
        }

        if (damageUpgradeButton != null)
        {
            bool canBuyDamage = PlayerProgress.Instance.DamageLevel < 2 && PlayerProgress.Instance.CanAfford(damageUpgradePrice);
            damageUpgradeButton.interactable = canBuyDamage;
        }

        if (speedUpgradeButton != null)
        {
            bool canBuySpeed = PlayerProgress.Instance.SpeedLevel < 2 && PlayerProgress.Instance.CanAfford(speedUpgradePrice);
            speedUpgradeButton.interactable = canBuySpeed;
        }

        if (doubleJumpButton != null)
        {
            bool canBuyDoubleJump = !PlayerProgress.Instance.DoubleJumpUnlocked && PlayerProgress.Instance.CanAfford(doubleJumpPrice);
            doubleJumpButton.interactable = canBuyDoubleJump;
        }

        if (dashButton != null)
        {
            bool canBuyDash = !PlayerProgress.Instance.DashUnlocked && PlayerProgress.Instance.CanAfford(dashPrice);
            dashButton.interactable = canBuyDash;
        }
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    private void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}