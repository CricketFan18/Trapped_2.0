using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    public string ItemName;
    public Sprite itemIcon;
    public string InteractionPrompt => "Press E to pick up";
    public bool Interact(Interactor interactor)
    {
        Debug.Log("PickUp called on " + gameObject.name);
        InventorySystem.Instance.AddToInventory(ItemName, itemIcon);
        Destroy(gameObject);
        return true;
    }
}
