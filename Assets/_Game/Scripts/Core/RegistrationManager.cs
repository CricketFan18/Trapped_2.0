using UnityEngine;
using TMPro; // Standard Unity UI Text
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RegistrationManager : MonoBehaviour
{
    [Header("Event Configuration")]
    [Tooltip("The exact name of the first game scene")]

    public string GameSceneName = "Zone1_GroundFloor";
    public string EventAccessCode = "START2025";

    [Header("UI Panels")]
    public GameObject RegistrationPanel;
    public GameObject WaitingPanel;

    [Header("Mandatory Inputs")]
    public TMP_InputField TeamNameInput;
    public TMP_InputField P1NameInput;
    public TMP_InputField P1RollInput;

    [Header("Optional Inputs")]
    public TMP_InputField P2NameInput;
    public TMP_InputField P2RollInput;
    public TMP_InputField P3NameInput;
    public TMP_InputField P3RollInput;

    [Header("Feedback UI")]
    public Button SubmitButton;
    public TextMeshProUGUI StatusText; // Shows errors or "Success!"
    public TMP_InputField AccessCodeInput; // Inside Waiting Panel

    private void Start()
    {
        RegistrationPanel.SetActive(true);
        WaitingPanel.SetActive(false);
        StatusText.text = "";
        AccessCodeInput.text = "";
    }

    public void OnSubmitClicked()
    {
        if (string.IsNullOrWhiteSpace(TeamNameInput.text) ||
            string.IsNullOrWhiteSpace(P1NameInput.text) ||
            string.IsNullOrWhiteSpace(P1RollInput.text))
        {
            StatusText.color = Color.red;
            StatusText.text = "Error: Team Name and Player 1 are required!";
            return;
        }

        // Lock UI (Prevent double submission)
        SubmitButton.interactable = false;
        StatusText.color = Color.yellow;
        StatusText.text = "Registering Team...";

        // Prepare Data (Handle Optionals)
        // If P2 name is empty, send empty string ""
        string p2Name = string.IsNullOrWhiteSpace(P2NameInput.text) ? "" : P2NameInput.text;
        string p2Roll = string.IsNullOrWhiteSpace(P2RollInput.text) ? "" : P2RollInput.text;

        string p3Name = string.IsNullOrWhiteSpace(P3NameInput.text) ? "" : P3NameInput.text;
        string p3Roll = string.IsNullOrWhiteSpace(P3RollInput.text) ? "" : P3RollInput.text;

        // Send to Google Sheet Manager
        GoogleSheetManager.Instance.RegisterTeam(
            TeamNameInput.text,
            P1NameInput.text, P1RollInput.text,
            p2Name, p2Roll,
            p3Name, p3Roll
        );

        // Move to Waiting Room (Give a small delay for visual feedback)
        Invoke(nameof(ShowWaitingRoom), 1.5f);
    }

    private void ShowWaitingRoom()
    {
        RegistrationPanel.SetActive(false);
        WaitingPanel.SetActive(true);
    }

    // START GAME (ACCESS CODE) ---
    public void OnStartGameClicked()
    {
        if (AccessCodeInput.text.Trim() == EventAccessCode)
        {
            // Code Correct -> Load Level
            SceneManager.LoadScene(GameSceneName);
        }
        else
        {
            // Code Wrong -> Show Error
            AccessCodeInput.text = ""; // Clear field
            AccessCodeInput.placeholder.GetComponent<TextMeshProUGUI>().text = "WRONG CODE";
            AccessCodeInput.placeholder.GetComponent<TextMeshProUGUI>().color = Color.red;
        }
    }
}