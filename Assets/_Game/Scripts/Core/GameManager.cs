using UnityEngine;
using TMPro; // Standard Unity UI
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float TimeRemaining = 7200f; // 2 Hours in seconds
    public bool IsPaused = false;

    [Header("UI References")]
    public TextMeshProUGUI TimerText; // Drag your UI Text here
    public GameObject GameOverPanel;
    public GameObject WinPanel;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        if (!IsPaused && TimeRemaining > 0)
        {
            TimeRemaining -= Time.deltaTime;
            UpdateTimerUI();
        }
        else if (TimeRemaining <= 0 && !IsPaused)
        {
            TriggerGameOver();
        }
    }

    void UpdateTimerUI()
    {
        if (TimerText != null)
        {
            float minutes = Mathf.FloorToInt(TimeRemaining / 60);
            float seconds = Mathf.FloorToInt(TimeRemaining % 60);
            TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void TriggerGameOver()
    {
        IsPaused = true;
        if (GameOverPanel) GameOverPanel.SetActive(true);
        Debug.Log("GAME OVER: TIME EXPIRED");
    }

    public void TriggerWin()
    {
        IsPaused = true;
        if (WinPanel) WinPanel.SetActive(true);
        Debug.Log("MISSION ACCOMPLISHED");
    }
}