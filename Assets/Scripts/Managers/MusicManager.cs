using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio")]
    public AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        ApplySavedVolume();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RestartTrackFromBeginning();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RestartTrackFromBeginning();
    }

    private void RestartTrackFromBeginning()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void ApplySavedVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (audioSource != null)
        {
            audioSource.volume = savedVolume;
        }
    }

    public float GetCurrentVolume()
    {
        if (audioSource != null)
        {
            return audioSource.volume;
        }

        return PlayerPrefs.GetFloat("MusicVolume", 1f);
    }
}