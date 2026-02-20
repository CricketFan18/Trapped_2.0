using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    // Pure data storage. No UI references needed!
    public List<string> CollectedItemsNames = new List<string>();
    public List<Sprite> CollectedItemSprites = new List<Sprite>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void CollectItem(string itemName, Sprite icon)
    {
        CollectedItemsNames.Add(itemName);
        CollectedItemSprites.Add(icon);
        Debug.Log($"[Inventory] Added {itemName} to pockets. Total items: {CollectedItemsNames.Count}");
    }
}