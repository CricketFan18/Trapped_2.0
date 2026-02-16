using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KS_ItemObj : MonoBehaviour, IInteractable
{
    public KS_Item item;
    private knapsackManager InventoryParent;
    private bool over = false;

    private void Start()
    {
        InventoryParent = FindObjectOfType<knapsackManager>();
    }
    // Implementation of the Interface Property
    public string InteractionPrompt => "Press E to Pick " + item.name;

    // Implementation of the Interface Method
    public bool Interact(Interactor interactor)
    {
        // Simple visual feedback
        InventoryParent.GetComponentInChildren<KS_Inventory>().AddItem(item);
        InventoryParent.GetComponent<knapsackManager>().items_collected += 1;
        Debug.Log("Interaction Successful!");
        over = true;
        return true;
    }

    private void FixedUpdate()
    {
        if (over)
        {
            over = false;
            gameObject.SetActive(false);
            
        }
    }
}

