using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public string mainMenuSceneName = "MainMenu";

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.RestoreLevelEntrySnapshotForCurrentScene();
        }

        if (GameMetrics.Instance != null)
        {
            GameMetrics.Instance.RestoreLevelEntrySnapshotForCurrentScene();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.ResetAllProgress();
        }

        if (GameMetrics.Instance != null)
        {
            GameMetrics.Instance.ResetAllMetrics();
        }

        if (DynamicDifficultyManager.Instance != null)
        {
            DynamicDifficultyManager.Instance.ResetModelState();
        }

        PlayerPrefs.DeleteKey("HasCheckpoint");
        PlayerPrefs.DeleteKey("CheckpointX");
        PlayerPrefs.DeleteKey("CheckpointY");
        PlayerPrefs.DeleteKey("CheckpointScene");
        PlayerPrefs.Save();

        SceneManager.LoadScene(mainMenuSceneName);
    }
}