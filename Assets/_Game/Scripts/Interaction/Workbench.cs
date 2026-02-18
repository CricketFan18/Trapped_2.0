using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workbench : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Press E to use Workbench";

    [Tooltip("Drag your CraftingUIManager here")]
    public CraftingSystem uiManager;

    public bool Interact(Interactor interactor)
    {
        if (uiManager != null)
        {
            uiManager.OpenCraftingStation();
            return true;
        }
        return false;
    }
}