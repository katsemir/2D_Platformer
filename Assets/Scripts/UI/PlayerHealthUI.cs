using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image[] heartImages;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    private int lastHealth = -1;
    private int lastMaxHealth = -1;

    void Update()
    {
        if (playerHealth == null)
            return;

        if (playerHealth.CurrentHealth == lastHealth && playerHealth.MaxHealth == lastMaxHealth)
            return;

        lastHealth = playerHealth.CurrentHealth;
        lastMaxHealth = playerHealth.MaxHealth;

        UpdateHearts();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
                continue;

            if (i < playerHealth.CurrentHealth)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].enabled = true;
            }
            else if (i < playerHealth.MaxHealth)
            {
                heartImages[i].sprite = emptyHeartSprite;
                heartImages[i].enabled = true;
            }
            else
            {
                heartImages[i].enabled = false;
            }
        }
    }
}