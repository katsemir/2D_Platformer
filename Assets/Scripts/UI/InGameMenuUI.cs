using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuPanel;
    public Slider volumeSlider;
    public Button settingsButton;

    [Header("Scene")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.Escape;

    [Header("Optional References")]
    public ShopUI shopUI;

    private bool isOpen = false;

    public bool IsOpen
    {
        get { return isOpen; }
    }

    private void Awake()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        SetupVolumeSlider();
        SetupSettingsButton();
    }

    private void Start()
    {
        ApplySavedVolumeToUI();
        UpdateSettingsButtonState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }

        UpdateSettingsButtonState();
    }

    private void SetupSettingsButton()
    {
        if (settingsButton == null)
            return;

        settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }

    private void OnSettingsButtonClicked()
    {
        ToggleMenu();
    }

    private void SetupVolumeSlider()
    {
        if (volumeSlider == null)
            return;

        volumeSlider.onValueChanged.RemoveListener(SetVolume);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void ApplySavedVolumeToUI()
    {
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(savedVolume);
        }

        ApplyVolume(savedVolume);
    }

    public void ToggleMenu()
    {
        if (isOpen)
        {
            CloseMenu();
        }
        else
        {
            if (CanOpenMenu() == false)
                return;

            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (CanOpenMenu() == false)
            return;

        isOpen = true;

        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }

        ApplySavedVolumeToUI();
        Time.timeScale = 0f;
        UpdateSettingsButtonState();
    }

    public void CloseMenu()
    {
        isOpen = false;

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        UpdateSettingsButtonState();
    }

    public void ResumeGame()
    {
        CloseMenu();
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        isOpen = false;

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SetVolume(float volume)
    {
        ApplyVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float volume)
    {
        AudioListener.volume = volume;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(volume);
        }
    }

    private bool CanOpenMenu()
    {
        if (shopUI != null && shopUI.IsOpen)
            return false;

        if (Time.timeScale == 0f && isOpen == false)
            return false;

        return true;
    }

    private void UpdateSettingsButtonState()
    {
        if (settingsButton == null)
            return;

        settingsButton.gameObject.SetActive(isOpen == false);
    }
}