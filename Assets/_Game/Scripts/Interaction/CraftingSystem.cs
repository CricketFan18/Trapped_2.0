using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The parent GameObject containing your Crafting slots and buttons")]
    public GameObject craftingUIPanel;

    [Tooltip("Drag your InventoryScreenUI object here (must have a RectTransform)")]
    public RectTransform inventoryRectTransform;

    [Header("Position Settings")]
    [Tooltip("How far the inventory shifts on the X axis when crafting opens")]
    public float shiftedXPosition = -100f;

    private float originalXPosition;
    private bool isCraftingActive = false;

    void Start()
    {
        // Remember the center position of the inventory so we can return it later
        if (inventoryRectTransform != null)
        {
            originalXPosition = inventoryRectTransform.anchoredPosition.x;
        }

        craftingUIPanel.SetActive(false);
    }

    void Update()
    {
        // If the player presses 'I' to close the inventory while crafting is open,
        // we need to detect that the inventory closed and hide the crafting UI too.
        if (isCraftingActive && !InventorySystem.Instance.isOpen)
        {
            CloseCraftingStation();
        }
    }

    public void OpenCraftingStation()
    {
        if (isCraftingActive) return; // Already open

        isCraftingActive = true;
        craftingUIPanel.SetActive(true);

        // 1. Force open the Inventory if the player hasn't already opened it
        if (!InventorySystem.Instance.isOpen)
        {
            InventorySystem.Instance.isOpen = true;
            InventorySystem.Instance.inventoryScreenUI.SetActive(true);

            // Unlock the cursor so they can drag items
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 2. Shift the Inventory to the left
        if (inventoryRectTransform != null)
        {
            Vector2 pos = inventoryRectTransform.anchoredPosition;
            pos.x = originalXPosition + shiftedXPosition;
            inventoryRectTransform.anchoredPosition = pos;
        }
    }

    public void CloseCraftingStation()
    {
        if (!isCraftingActive) return;

        isCraftingActive = false;
        craftingUIPanel.SetActive(false);

        // Reset the Inventory position back to the center
        if (inventoryRectTransform != null)
        {
            Vector2 pos = inventoryRectTransform.anchoredPosition;
            pos.x = originalXPosition;
            inventoryRectTransform.anchoredPosition = pos;
        }
    }
}