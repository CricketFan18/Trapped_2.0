using UnityEngine;

public interface IInteractable
{
    // The text that appears on screen (e.g., "Press E to Assemble Bomb")
    string InteractionPrompt { get; }

    // What happens when the player presses E
    void OnInteract();
}