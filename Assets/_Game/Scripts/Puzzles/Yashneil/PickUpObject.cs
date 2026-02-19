using UnityEngine;

public class PickUpObject : MonoBehaviour, IInteractable
{
    public string ItemName;
    public Sprite itemIcon;

    public string InteractionPrompt => $"Press E to pick up {ItemName}";

    public bool Interact(Interactor interactor)
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.CollectItem(ItemName, itemIcon);
        }

        Destroy(gameObject);
        return true;
    }
}