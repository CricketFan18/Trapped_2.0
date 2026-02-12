using UnityEngine;

public interface IInteractable
{
    // The text displayed on UI (e.g., "Press E to Cut Red Wire")
    public string InteractionPrompt { get; }

    // What happens when the player presses E
    // We pass the Interactor in case the puzzle needs to know who triggered it (Inventory check)
    public bool Interact(Interactor interactor);
}