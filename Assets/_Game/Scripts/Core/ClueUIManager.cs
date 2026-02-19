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

        // 1. Free the mouse cursor so they can read/click, but DO NOT pause the game!
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseClue()
    {
        if (cluePanel != null) cluePanel.SetActive(false);
        _isOpen = false;

        // 2. Lock the mouse back to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}