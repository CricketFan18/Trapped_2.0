using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoogleSheetManager : MonoBehaviour
{
    public static GoogleSheetManager Instance;

    [Header("Configuration")]
    public string WebAppURL = "PASTE_YOUR_NEW_DEPLOYMENT_URL_HERE";

    [Header("Session Data")]
    public string TeamName;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // --- 1. REGISTRATION (Call this from Main Menu) ---
    public void RegisterTeam(string tName, string p1, string r1, string p2, string r2, string p3, string r3)
    {
        TeamName = tName;
        StartCoroutine(PostRegistration(tName, p1, r1, p2, r2, p3, r3));
    }

    // --- 2. PUZZLE SOLVED (Call this from PuzzleController) ---
    public void LogPuzzleSolve(string puzzleID, int scoreAwarded)
    {
        StartCoroutine(PostPuzzle(puzzleID, scoreAwarded));
    }

    // --- 3. GAME OVER (Call this from GameManager) ---
    public void SendFinalTime(float totalTimeInSeconds)
    {
        string timeStr = string.Format("{0:00}:{1:00}", totalTimeInSeconds / 60, totalTimeInSeconds % 60);
        StartCoroutine(PostGameOver(timeStr));
    }

    // ---------------- NETWORK COROUTINES ----------------

    IEnumerator PostRegistration(string tName, string p1, string r1, string p2, string r2, string p3, string r3)
    {
        WWWForm form = new WWWForm();
        form.AddField("action", "REGISTER");
        form.AddField("teamName", tName);

        // Player Data
        form.AddField("p1Name", p1); form.AddField("p1Roll", r1);
        form.AddField("p2Name", p2); form.AddField("p2Roll", r2);
        form.AddField("p3Name", p3); form.AddField("p3Roll", r3);

        yield return SendRequest(form);
    }

    IEnumerator PostPuzzle(string puzzleID, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("action", "PUZZLE");
        form.AddField("teamName", TeamName);
        form.AddField("puzzleID", puzzleID);
        form.AddField("scoreToAdd", score); // We send score to add, not total

        yield return SendRequest(form);
    }

    IEnumerator PostGameOver(string timeStr)
    {
        WWWForm form = new WWWForm();
        form.AddField("action", "GAMEOVER");
        form.AddField("teamName", TeamName);
        form.AddField("totalTime", timeStr);

        yield return SendRequest(form);
    }

    IEnumerator SendRequest(WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(WebAppURL, form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) Debug.LogError("Google Sheet Error: " + www.error);
            else Debug.Log("Upload Success: " + www.downloadHandler.text);
        }
    }
}