using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DraggableItem : MonoBehaviour
{
    [Header("Assign your global Canvas here!")]
    public Canvas canvas;

    private GameObject selectedItem;
    private Transform originalParent;
    private Vector3 originalPosition;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TrySelectItem();

        if (Input.GetMouseButton(0) && selectedItem != null)
            selectedItem.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonUp(0) && selectedItem != null) TryPlaceItem();
    }

    void TrySelectItem()
    {
        PointerEventData data = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("Item"))
            {
                Image img = result.gameObject.GetComponent<Image>();
                if (img != null)
                {
                    selectedItem = result.gameObject;
                    originalParent = selectedItem.transform.parent;
                    originalPosition = selectedItem.transform.position;

                    if (canvas != null) selectedItem.transform.SetParent(canvas.transform);
                    break;
                }
            }
        }
    }

    void TryPlaceItem()
    {
        PointerEventData data = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        bool placed = false;

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("Slot"))
            {
                Transform targetSlot = result.gameObject.transform;
                CraftingSlot targetCSlot = targetSlot.GetComponent<CraftingSlot>();
                CraftingSlot originCSlot = originalParent.GetComponent<CraftingSlot>();

                // 1. Swap handling if slot is full
                if (targetSlot.childCount > 0)
                {
                    Transform existingItem = targetSlot.GetChild(0);
                    existingItem.SetParent(originalParent);
                    existingItem.localPosition = Vector3.zero;

                    if (originCSlot != null)
                    {
                        existingItem.localScale = new Vector3(0.95f, 0.95f, 1f);
                        originCSlot.currentItemName = GetItemName(existingItem.GetComponent<Image>());
                    }
                    else
                    {
                        existingItem.localScale = Vector3.one;
                        // Item went back to inventory, erase from block
                        if (CraftingSystem.Instance != null && CraftingSystem.Instance.temporaryItemStorage.ContainsKey(existingItem.gameObject))
                            CraftingSystem.Instance.temporaryItemStorage.Remove(existingItem.gameObject);
                    }
                }
                else
                {
                    if (originCSlot != null) originCSlot.currentItemName = "";
                }

                // 2. Physically move selected item
                selectedItem.transform.SetParent(targetSlot);
                selectedItem.transform.localPosition = Vector3.zero;

                // 3. Handle Scaling & Add to Temporary Block
                if (targetCSlot != null)
                {
                    selectedItem.transform.localScale = new Vector3(0.95f, 0.95f, 1f); // 5% shrink
                    targetCSlot.currentItemName = GetItemName(selectedItem.GetComponent<Image>());

                    // SAVE TO BLOCK: Only if it came from the inventory
                    if (originCSlot == null && CraftingSystem.Instance != null)
                    {
                        CraftingSystem.Instance.temporaryItemStorage[selectedItem] = originalParent;
                    }
                }
                else
                {
                    selectedItem.transform.localScale = Vector3.one; // Restores to 100%
                    // It returned to inventory, remove from block
                    if (CraftingSystem.Instance != null && CraftingSystem.Instance.temporaryItemStorage.ContainsKey(selectedItem))
                    {
                        CraftingSystem.Instance.temporaryItemStorage.Remove(selectedItem);
                    }
                }

                placed = true;
                if (CraftingSystem.Instance != null) CraftingSystem.Instance.OnItemPlaced();
                break;
            }
        }

        // FIX FOR DROPPING OUTSIDE SLOTS: Preserves the correct scale depending on where it bounces back to
        if (!placed)
        {
            selectedItem.transform.SetParent(originalParent);
            selectedItem.transform.position = originalPosition;

            if (originalParent.GetComponent<CraftingSlot>() != null)
                selectedItem.transform.localScale = new Vector3(0.95f, 0.95f, 1f); // Bounced back to Crafting (stays 95%)
            else
                selectedItem.transform.localScale = Vector3.one; // Bounced back to Inventory (stays 100%)
        }
        selectedItem = null;
    }

    private string GetItemName(Image img)
    {
        if (img == null || InventorySystem.Instance == null) return "";
        int index = InventorySystem.Instance.itemIcons.IndexOf(img);
        if (index >= 0 && index < InventorySystem.Instance.itemList.Count)
            return InventorySystem.Instance.itemList[index];
        return "";
    }
}