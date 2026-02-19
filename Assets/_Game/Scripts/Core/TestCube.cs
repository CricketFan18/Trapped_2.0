using UnityEngine;

public class TestCube : MonoBehaviour, IInteractable
{
    // Implementation of the Interface Property
    public string InteractionPrompt => "Press E to change color";

    // Implementation of the Interface Method
    public bool Interact(Interactor interactor)
    {
        // Simple visual feedback
        GetComponent<Renderer>().material.color = Random.ColorHSV();
        Debug.Log("Interaction Successful!");
        return true;
    }
}