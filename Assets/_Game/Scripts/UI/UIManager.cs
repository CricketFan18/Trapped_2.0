using UnityEngine;
using TMPro; // Make sure TextMeshPro is installed

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD References")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _interactionPromptText;
    [SerializeField] private GameObject _interactionPanel; // The background for the prompt

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        // Update Timer Display
        if (GameManager.Instance != null)
        {
            float time = GameManager.Instance.CurrentTime;
            float minutes = Mathf.FloorToInt(time / 60);
            float seconds = Mathf.FloorToInt(time % 60);
            _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
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