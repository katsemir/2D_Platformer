using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    [Header("UI")]
    public GameObject victoryPanel;

    [Header("Scene")]
    public string mainMenuSceneName = "MainMenu";

    private bool isShown = false;

    private void Awake()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    public void ShowVictoryScreen()
    {
        if (isShown)
            return;

        isShown = true;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        MainMenuUI.ResetFullGameState();

        SceneManager.LoadScene(mainMenuSceneName);
    }
}