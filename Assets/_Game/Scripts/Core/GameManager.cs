using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float MaxTime = 7200f; // 2 Hours
    public float CurrentTime;
    public bool IsGamePaused = false;
    public bool HasEscaped = false;

    private void Awake()
    {
        // Ensure only one Manager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CurrentTime = MaxTime;
        // Lock cursor for FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
    }

    public void GameOver()
    {
        IsGamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("GAME OVER: TIME EXPIRED");
        // UIManager.Instance.ShowGameOver(); // We will add this later
        // 1. Add Score Locally
        //GameManager.Instance.AddScore(ScoreValue, PuzzleID);

        //// 2. SEND TO GOOGLE
        //GoogleSheetManager.Instance.LogPuzzleSolve(PuzzleID); // <--- ADD THIS

        //OnPuzzleSolved.Invoke();
    }

    public void TimePenalty(float time)
    {
        CurrentTime-=time;
    }

    public void WinGame()
    {
        HasEscaped = true;
        IsGamePaused = true;
        Debug.Log("YOU ESCAPED!");
    }
}