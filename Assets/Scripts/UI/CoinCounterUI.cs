using UnityEngine;
using TMPro;

public class CoinCounterUI : MonoBehaviour
{
    public TMP_Text coinText;

    void Update()
    {
        if (coinText == null)
            return;

        coinText.text = "Coins: " + PlayerProgress.Instance.TotalCoins;
    }
}