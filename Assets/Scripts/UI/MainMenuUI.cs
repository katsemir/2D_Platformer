using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scenes")]
    public string firstLevelScene = "SampleScene";

    [Header("UI")]
    public Button continueButton;
    public Slider volumeSlider;

    private static bool appLaunchResetDone = false;

    private void Awake()
    {
        PerformFirstLaunchResetIfNeeded();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        bool hasCheckpoint = PlayerPrefs.GetInt("HasCheckpoint", 0) == 1;

        if (continueButton != null)
        {
            continueButton.interactable = hasCheckpoint;
        }

        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        SetVolume(savedVolume);
    }

    private void PerformFirstLaunchResetIfNeeded()
    {
        if (appLaunchResetDone)
            return;

        appLaunchResetDone = true;
        ResetFullGameState();

        Debug.Log("MainMenuUI -> Fresh launch reset completed.");
    }

    public static void ResetFullGameState()
    {
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
    }

    public void PlayNewGame()
    {
        ResetFullGameState();

        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelScene);
    }

    public void ContinueGame()
    {
        string savedScene = PlayerPrefs.GetString("CheckpointScene", firstLevelScene);

        Time.timeScale = 1f;
        SceneManager.LoadScene(savedScene);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }
}