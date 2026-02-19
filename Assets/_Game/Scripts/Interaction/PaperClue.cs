using UnityEngine;

public class PaperClue : MonoBehaviour, IInteractable
{
    [Header("Clue Settings")]
    public string PromptText = "Read Note";

    [Tooltip("Type the clue here. It will automatically format in the UI.")]
    [TextArea(5, 15)]
    public string ClueMessage = "This is a secret clue...";

    // Implementation of the Interface Property
    public string InteractionPrompt => $"Press E to {PromptText}";

    // Implementation of the Interface Method
    public bool Interact(Interactor interactor)
    {
        // Tell the UI Manager to display this specific string
        if (ClueUIManager.Instance != null)
        {
            ClueUIManager.Instance.ShowClue(ClueMessage);
            return true;
        }
        else
        {
            Debug.LogError("[PaperClue] ClueUIManager is missing from the scene!");
            return false;
        }
    }
}