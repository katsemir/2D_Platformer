using UnityEngine;

public class LevelCompleteUI : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject player;

    private bool levelFinished = false;

    public void ShowWin()
    {
        if (levelFinished)
            return;

        levelFinished = true;

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        Time.timeScale = 0f;
    }

    public void ResetState()
    {
        levelFinished = false;

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }
}