using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishZone : MonoBehaviour
{
    [Header("Scene Transition")]
    public string nextSceneName = "";
    public bool useNextBuildIndexIfSceneNameEmpty = true;
    public float loadDelay = 0.5f;

    [Header("Player Control")]
    public bool lockPlayerBeforeLoad = true;

    [Header("References")]
    public GameMetrics metrics;
    public DynamicDifficultyManager difficultyManager;

    private bool levelCompleted = false;

    private void Awake()
    {
        if (metrics == null)
        {
            metrics = FindFirstObjectByType<GameMetrics>();
        }

        if (difficultyManager == null)
        {
            difficultyManager = FindFirstObjectByType<DynamicDifficultyManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (levelCompleted)
            return;

        if (!collision.CompareTag("Player"))
            return;

        levelCompleted = true;

        if (metrics != null)
        {
            metrics.RegisterLevelCompleted();
        }

        if (difficultyManager != null)
        {
            difficultyManager.EvaluateDifficulty();
        }

        PlayerController playerController = collision.GetComponent<PlayerController>();

        if (playerController == null)
        {
            playerController = collision.GetComponentInParent<PlayerController>();
        }

        if (lockPlayerBeforeLoad && playerController != null)
        {
            playerController.SetInputLocked(true);
        }

        Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

        if (playerRb == null)
        {
            playerRb = collision.GetComponentInParent<Rigidbody2D>();
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        Time.timeScale = 1f;

        if (loadDelay <= 0f)
        {
            LoadNextScene();
        }
        else
        {
            Invoke(nameof(LoadNextScene), loadDelay);
        }

        Debug.Log("Finish Zone reached. Loading next scene...");
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        if (useNextBuildIndexIfSceneNameEmpty)
        {
            int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
            int nextBuildIndex = currentBuildIndex + 1;

            if (nextBuildIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextBuildIndex);
                return;
            }

            Debug.LogWarning("FinishZone: Next build index scene was not found.");
            return;
        }

        Debug.LogWarning("FinishZone: nextSceneName is empty and automatic next build index loading is disabled.");
    }
}