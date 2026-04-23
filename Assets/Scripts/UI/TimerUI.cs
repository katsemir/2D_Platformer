using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public GameMetrics metrics;
    public TMP_Text timerText;

    private void Awake()
    {
        ResolveReferences();
        UpdateTimerText(0f);
    }

    private void OnEnable()
    {
        ResolveReferences();
        UpdateTimerText(0f);
    }

    private void Update()
    {
        ResolveReferences();

        if (metrics == null || timerText == null)
            return;

        float time = metrics.CurrentLevelTime;
        UpdateTimerText(time);
    }

    private void ResolveReferences()
    {
        if (metrics == null)
        {
            metrics = GameMetrics.Instance;
        }

        if (timerText == null)
        {
            timerText = GetComponent<TMP_Text>();
        }
    }

    private void UpdateTimerText(float time)
    {
        if (timerText == null)
            return;

        if (time < 0f)
        {
            time = 0f;
        }

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timerText.text = "Time: " + minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}