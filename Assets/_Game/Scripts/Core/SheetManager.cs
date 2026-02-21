using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class SheetManager : MonoBehaviour
{
    public static SheetManager Instance;

    [Header("Configuration")]
    public string WebAppURL = "PASTE_YOUR_NEW_DEPLOYMENT_URL_HERE";

    [Header("Session Data")]
    public string TeamName;

    private string logFilePath;
    private readonly byte[] encryptionKey = Encoding.UTF8.GetBytes("Tr4pp3d_2.0_K3y!"); // 16 bytes for AES-128
    private readonly byte[] encryptionIV = Encoding.UTF8.GetBytes("Tr4pp3d_2.0_IV!!"); // 16 bytes for AES-128

    private void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
            logFilePath = Path.Combine(Application.persistentDataPath, "session_log.dat");
            LogToEncryptedFile("--- NEW SESSION STARTED ---");
        }
        else { Destroy(gameObject); }
    }

    // --- 1. REGISTRATION (Call this from Main Menu) ---
    public void RegisterTeam(string tName, string p1, string r1, string p2, string r2, string p3, string r3)
    {
        TeamName = tName;
        LogToEncryptedFile($"Registered Team: {tName} | P1: {p1} ({r1}) | P2: {p2} ({r2}) | P3: {p3} ({r3})");
        StartCoroutine(PostRegistration(tName, p1, r1, p2, r2, p3, r3));
    }

    // --- 2. PUZZLE SOLVED (Call this from PuzzleController) ---
    public void LogPuzzleSolve(string puzzleID, int scoreAwarded)
    {
        LogToEncryptedFile($"Puzzle Solved: {puzzleID} | Score Awarded: {scoreAwarded}");
        StartCoroutine(PostPuzzle(puzzleID, scoreAwarded));
    }

    // --- 3. GAME OVER (Call this from GameManager) ---
    public void SendFinalTime(float totalTimeInSeconds)
    {
        // Use FloorToInt to prevent automatic rounding errors
        int minutes = Mathf.FloorToInt(totalTimeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalTimeInSeconds % 60f);
        
        string timeStr = string.Format("{0:00}:{1:00}", minutes, seconds);
        LogToEncryptedFile($"Game Over | Final Time: {timeStr}");
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

    // ---------------- ENCRYPTED LOGGING ----------------

    private void LogToEncryptedFile(string message)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}\n";

            string existingContent = DecryptFile();
            string newContent = existingContent + logEntry;

            EncryptToFile(newContent);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write encrypted log: " + e.Message);
        }
    }

    private void EncryptToFile(string content)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = encryptionKey;
            aesAlg.IV = encryptionIV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (FileStream fileStream = new FileStream(logFilePath, FileMode.Create))
            using (CryptoStream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(content);
            }
        }
    }

    private string DecryptFile()
    {
        if (!File.Exists(logFilePath)) return "";

        try
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = encryptionKey;
                aesAlg.IV = encryptionIV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (FileStream fileStream = new FileStream(logFilePath, FileMode.Open))
                using (CryptoStream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
        catch
        {
            return ""; // Return empty if decryption fails (e.g., corrupted or tampered file)
        }
    }
}