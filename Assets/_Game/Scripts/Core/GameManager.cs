using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float MaxTime = 7200f; // 2 Hours
    public float CurrentTime;
    public int CurrentScore = 0;

    [Header("State")]
    public bool IsGamePaused = false;
    public bool HasEscaped = false;
    public bool IsInPuzzleMode = false;

    // Track solved puzzles to prevent duplicate scoring
    private HashSet<string> _solvedPuzzles = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        CurrentTime = MaxTime;
        SetPuzzleMode(false); // Start in standard FPS mode
    }

    private void Update()
    {
        if (!IsGamePaused && !HasEscaped)
        {
            CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 0)
            {
                CurrentTime = 0;
                GameOver();
            }
        }

        // --- THE ESCAPE KEY LISTENER ---
        if (IsInPuzzleMode && Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.CloseActivePuzzle();
        }
    }

    // --- PUZZLE STATE & SCORING ---
    public bool IsPuzzleSolved(string puzzleID)
    {
        return _solvedPuzzles.Contains(puzzleID);
    }

    public void AddScore(int amount, string puzzleID)
    {
        if (_solvedPuzzles.Contains(puzzleID)) return;

        _solvedPuzzles.Add(puzzleID);
        CurrentScore += amount;
        Debug.Log($"Solved {puzzleID}! Score: {CurrentScore}");

        // LINKED: Send the live score update to Google Sheets!
        if (SheetManager.Instance != null)
        {
            SheetManager.Instance.LogPuzzleSolve(puzzleID, amount);
        }
    }

    // --- MODE SWITCHER (FPS <-> UI) ---
    public void SetPuzzleMode(bool isUIActive)
    {
        IsInPuzzleMode = isUIActive;

        // Toggle Cursor
        Cursor.lockState = isUIActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isUIActive;

        // Toggle Player Movement & Interaction
        FirstPersonController fps = FindObjectOfType<FirstPersonController>();
        if (fps != null) fps.enabled = !isUIActive;

        Interactor interactor = FindObjectOfType<Interactor>();
        if (interactor != null) interactor.enabled = !isUIActive;
    }

    public void GameOver()
    {
        IsGamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("GAME OVER: TIME EXPIRED");

        // LINKED: Send max time to Google Sheets to indicate they ran out of time
        if (SheetManager.Instance != null)
        {
            SheetManager.Instance.SendFinalTime(MaxTime);
        }
        
        UIManager.Instance.ShowEndScreen();
        // UIManager.Instance.ShowEndScreen(CurrentScore, false); // Add later
    }

    public void TimePenalty(float time)
    {
        CurrentTime -= time;
        Debug.Log($"Time Penalty: {time} seconds lost!");
    }

    public void WinGame()
    {
        HasEscaped = true;
        IsGamePaused = true; // Stops the timer

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Calculate how long it took them to escape
        float timeTaken = MaxTime - CurrentTime;
        Debug.Log($"YOU ESCAPED! Final Score: {CurrentScore} | Time Taken: {timeTaken}s");

        // LINKED: Upload their final winning time to Google Sheets!
        if (SheetManager.Instance != null)
        {
            SheetManager.Instance.SendFinalTime(timeTaken);
        }

        UIManager.Instance.ShowEndScreen(); // Add later
    }
}