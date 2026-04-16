using UnityEngine;

public class Coin : MonoBehaviour
{
    public GameMetrics metrics;

    void Awake()
    {
        if (metrics == null)
        {
            metrics = FindFirstObjectByType<GameMetrics>();
        }

        if (metrics == null)
        {
            Debug.LogWarning("Coin: GameMetrics not found in scene.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        PlayerProgress.Instance.AddCoins(1);

        if (metrics != null)
        {
            metrics.RegisterCoin();
        }

        Destroy(gameObject);
    }
}