using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DraggableItem : MonoBehaviour
{
    private Canvas canvas;
    private GameObject selectedItem;
    private Transform originalParent;
    private Vector3 originalPosition;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

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
                        originCSlot.currentItemName = GetItemName(existingItem.gameObject);
                    }
                    else
                    {
                        existingItem.localScale = Vector3.one;
                        if (CraftingSystem.Instance != null && CraftingSystem.Instance.temporaryItemStorage.ContainsKey(existingItem.gameObject))
                            CraftingSystem.Instance.temporaryItemStorage.Remove(existingItem.gameObject);
                    }
                }
                else
                {
                    if (originCSlot != null) originCSlot.currentItemName = "";
                }

                // 2. Move selected item
                selectedItem.transform.SetParent(targetSlot);
                selectedItem.transform.localPosition = Vector3.zero;

                // 3. Scale & Register Block
                if (targetCSlot != null)
                {
                    selectedItem.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
                    targetCSlot.currentItemName = GetItemName(selectedItem);

                    if (originCSlot == null && CraftingSystem.Instance != null)
                        CraftingSystem.Instance.temporaryItemStorage[selectedItem] = originalParent;
                }
                else
                {
                    selectedItem.transform.localScale = Vector3.one;
                    if (CraftingSystem.Instance != null && CraftingSystem.Instance.temporaryItemStorage.ContainsKey(selectedItem))
                        CraftingSystem.Instance.temporaryItemStorage.Remove(selectedItem);
                }

                placed = true;
                break;
            }
        }

        // Return to original spot if dropped in empty space
        if (!placed)
        {
            selectedItem.transform.SetParent(originalParent);
            selectedItem.transform.position = originalPosition;

            if (originalParent.GetComponent<CraftingSlot>() != null)
                selectedItem.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
            else
                selectedItem.transform.localScale = Vector3.one;
        }
        selectedItem = null;
    }

    private string GetItemName(GameObject itemObj)
    {
        ItemData data = itemObj.GetComponent<ItemData>();
        if (data != null) return data.ItemName;
        return "";
    }
}