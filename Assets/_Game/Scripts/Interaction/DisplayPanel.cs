using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayPanel : MonoBehaviour,IInteractable
{
    private knapsackManager KS_Manager;

    private void Awake()
    {
        KS_Manager = FindObjectOfType<knapsackManager>();
    }
    // Implementation of the Interface Property
    public string InteractionPrompt => "Press E to Open Display Panel";

    // Implementation of the Interface Method
    public bool Interact(Interactor interactor)
    {
        // Simple visual feedback
        KS_Manager.toggleInventory();
        return true;
    }
}
