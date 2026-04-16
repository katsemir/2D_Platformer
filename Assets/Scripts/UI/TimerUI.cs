using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public GameMetrics metrics;
    public TMP_Text timerText;

    void Awake()
    {
        if (metrics == null)
        {
            metrics = FindFirstObjectByType<GameMetrics>();
        }
    }

    void Update()
    {
        if (metrics == null || timerText == null)
            return;

        float time = metrics.TimeAlive;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timerText.text = "Time: " + minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}