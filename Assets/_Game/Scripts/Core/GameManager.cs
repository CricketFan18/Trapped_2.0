using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float MaxTime = 7200f; // 2 Hours
    public float CurrentTime;
    public bool IsGamePaused = false;
    public bool HasEscaped = false;

#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    private static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern bool IsDebuggerPresent();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

    private const int GWL_STYLE = -16;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int SW_RESTORE = 9;
#endif

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
        SetPuzzleMode(false); // Start in standard FPS mode

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        IntPtr hWnd = FindWindow(null, Application.productName);
        if (hWnd != IntPtr.Zero)
        {
            int style = GetWindowLong(hWnd, GWL_STYLE);
            SetWindowLong(hWnd, GWL_STYLE, style & ~WS_MINIMIZEBOX);
        }
#endif
    }

    private void Update()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        bool isRemoteDebuggerPresent = false;
        try
        {
            CheckRemoteDebuggerPresent(System.Diagnostics.Process.GetCurrentProcess().Handle, ref isRemoteDebuggerPresent);
        }
        catch { }
        
        if (System.Diagnostics.Debugger.IsAttached || IsDebuggerPresent() || isRemoteDebuggerPresent)
        {
            Application.Quit();
        }
#else
        if (System.Diagnostics.Debugger.IsAttached)
        {
            Application.Quit();
        }
#endif

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

        // LINKED: Send max time to Google Sheets to indicate they ran out of time
        if (SheetManager.Instance != null)
        {
            SheetManager.Instance.SendFinalTime(MaxTime);
        }

    }

    public void TimePenalty(float time)
    {
        CurrentTime-=time;
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

    }

    private void OnApplicationFocus(bool hasFocus)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (!hasFocus)
        {
            IntPtr hWnd = FindWindow(null, Application.productName);
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
        }
#endif
    }

    private void OnApplicationPause(bool pauseStatus)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (pauseStatus)
        {
            IntPtr hWnd = FindWindow(null, Application.productName);
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
        }
#endif
    }
}