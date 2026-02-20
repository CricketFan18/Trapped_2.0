using UnityEngine;
using TMPro;

public class ClueUIManager : MonoBehaviour
{
    public static ClueUIManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject cluePanel;
    public TextMeshProUGUI clueTextDisplay;

    private bool _isOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (cluePanel != null) cluePanel.SetActive(false);
    }

    private void Update()
    {
        if (_isOpen)
        {
            // Close if they press Escape or E again
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            {
                CloseClue();
            }
        }
    }

    public void ShowClue(string textToDisplay)
    {
        if (_isOpen) return;

        if (clueTextDisplay != null) clueTextDisplay.text = textToDisplay;
        if (cluePanel != null) cluePanel.SetActive(true);

        _isOpen = true;

        // Use your GameManager to freeze the player & free the mouse WITHOUT pausing the timer
        if (GameManager.Instance != null) GameManager.Instance.SetPuzzleMode(true);
        if (UIManager.Instance != null) UIManager.Instance.SetInteractionPrompt(""); // Hide the crosshair text
    }

    public void CloseClue()
    {
        if (cluePanel != null) cluePanel.SetActive(false);
        _isOpen = false;

        // Return player to normal FPS state
        if (GameManager.Instance != null) GameManager.Instance.SetPuzzleMode(false);
    }
}