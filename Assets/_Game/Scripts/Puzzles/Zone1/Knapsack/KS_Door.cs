using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KS_Door : MonoBehaviour,IInteractable
{
    private knapsackManager KS_manager;
    private void Awake()
    {
        KS_manager = FindObjectOfType<knapsackManager>();
    }
    // Implementation of the Interface Property
    public string InteractionPrompt => "Press E to  exit";

    // Implementation of the Interface Method
    public bool Interact(Interactor interactor)
    {
        if (KS_manager.GetComponent<knapsackManager>().stealthLevel >= 5 || KS_manager.items_collected == 10)
        {
            Debug.Log("Door opened! You escaped the room.");
            
        }
        else
        {
            KS_manager.DisplayDoorMsg();
            Debug.Log("The door is locked. You need to collect more items or increase your stealth level.");
        }
        // Simple visual feedback
        return true;
    }
}
