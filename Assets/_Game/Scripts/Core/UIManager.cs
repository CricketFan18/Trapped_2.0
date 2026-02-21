using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD References")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _interactionPromptText;
    [SerializeField] private GameObject _interactionPanel;
    public GameObject _controlScreen;

    [Header("Puzzle UI System")]
    [Tooltip("An Empty UI Panel stretched to fill the screen")]
    public Transform PuzzleParent;
    private GameObject _activePuzzleUI;

    private void Awake() { if (Instance == null) Instance = this; }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePaused)
        {
            float time = GameManager.Instance.CurrentTime;
            float hours = Mathf.FloorToInt(time / 3600 );
            float minutes = Mathf.FloorToInt(time % 3600)/60;
            float seconds = Mathf.FloorToInt(time % 60);
            _timerText.text = string.Format("Time Left - {0:00}:{1:00}:{2:00}", hours, minutes, seconds);

            if (_scoreText != null)
            {
                _scoreText.text = "Score: " + GameManager.Instance.CurrentScore.ToString();
            }
        }
    }

    // --- PUZZLE HANDLING ---
    public void OpenPuzzle(GameObject puzzleInstance)
    {
        _activePuzzleUI = puzzleInstance;
        _activePuzzleUI.SetActive(true); // Show UI

        GameManager.Instance.SetPuzzleMode(true); // Freeze player
        SetInteractionPrompt(""); // Hide crosshair text
    }

    public void CloseActivePuzzle()
    {
        if (_activePuzzleUI != null)
        {
            _activePuzzleUI.SetActive(false); // Hide UI
            _activePuzzleUI = null;
        }

        GameManager.Instance.SetPuzzleMode(false); // Unfreeze player
    }

    public void SetInteractionPrompt(string promptText)
    {
        if (string.IsNullOrEmpty(promptText))
        {
            _interactionPanel.SetActive(false);
            _interactionPromptText.text = "";
        }
        else
        {
            _interactionPanel.SetActive(true);
            _interactionPromptText.text = promptText;
        }
    }
}