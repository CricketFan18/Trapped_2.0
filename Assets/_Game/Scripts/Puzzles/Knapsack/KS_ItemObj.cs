using UnityEngine;

public class KS_ItemObj : MonoBehaviour, IInteractable
{
    public KS_Item item;

    public string InteractionPrompt => "Press E to Pick Up " + (item != null ? item.itemName : "Item");

    public bool Interact(Interactor interactor)
    {
        if (item == null) return false;

        // If the UI is already open/spawned, add it directly
        if (KnapsackPuzzleUI.Instance != null && KnapsackPuzzleUI.Instance.inventory != null)
        {
            KnapsackPuzzleUI.Instance.inventory.AddItem(item);
            KnapsackPuzzleUI.Instance.items_collected++;
            KnapsackPuzzleUI.Instance.updateStats();
        }
        else
        {
            // If the UI hasn't been opened yet, save it to the static buffer!
            KnapsackPuzzleUI.PreCollectedItems.Add(item);
            KnapsackPuzzleUI.GlobalItemsCollected++;
        }

        Debug.Log($"[Knapsack] Picked up {item.itemName}");
        Destroy(gameObject);
        return true;
    }
}