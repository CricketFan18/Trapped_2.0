using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KS_Inventory : MonoBehaviour
{
    public GameObject BagObj;
    public GameObject InventoryObj;

    public Image draggedSlotImg;
    public KS_Slot draggedSlot = null;
    private bool isDragging = false;

    private List<KS_Slot> bagSlots = new List<KS_Slot>();
    private List<KS_Slot> inventorySlots = new List<KS_Slot>();
    private List<KS_Slot> allSlots = new List<KS_Slot>();

    private void Awake()
    {
        if (draggedSlotImg != null) draggedSlotImg.enabled = false;

        if (InventoryObj) inventorySlots.AddRange(InventoryObj.GetComponentsInChildren<KS_Slot>());
        if (BagObj) bagSlots.AddRange(BagObj.GetComponentsInChildren<KS_Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(bagSlots);
    }

    private void Update()
    {
        startDrag();
        //updateDragIcon();
        endDrag();
    }

    public void AddItem(KS_Item item)
    {
        foreach (KS_Slot slot in inventorySlots)
        {
            if (!slot.hasItem())
            {
                slot.set_item(item);
                break;
            }
        }
    }

    private void updateDragIcon()
    {
        if (isDragging && draggedSlotImg != null)
        {
            draggedSlotImg.transform.position = Input.mousePosition;
        }
    }

    private void startDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            KS_Slot hovered_slot = GetHoveredSlot();
            if (hovered_slot != null && hovered_slot.hasItem())
            {
                draggedSlot = hovered_slot;
                isDragging = true;
                
                /* commented this block because it causes bug in inventory
                 * 
                 * 
                if (draggedSlotImg != null)
                {
                    draggedSlotImg.enabled = true;
                    draggedSlotImg.sprite = draggedSlot.GetItem().icon;
                    draggedSlotImg.color = new Color(1, 0.3f, 0.3f, 0.7f);
                }*/
            }
        }
    }

    private void endDrag()
    {
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            KS_Slot hovered_slot = GetHoveredSlot();
            if (hovered_slot != null)
            {
                handleDropSlot(draggedSlot, hovered_slot);
            }
            if (draggedSlotImg != null) draggedSlotImg.enabled = false;
            draggedSlot = null;
            isDragging = false;
        }
    }

    private KS_Slot GetHoveredSlot()
    {
        foreach (KS_Slot slot in allSlots)
        {
            if (slot.isHovered) return slot;
        }
        return null;
    }

    private void handleDropSlot(KS_Slot from, KS_Slot to)
    {
        Debug.Log("handle drop");
        if (from == to) return;

        if (to.hasItem())
        {
            KS_Item temp_item1 = to.GetItem();
            KS_Item temp_item2 = from.GetItem();

            to.clearSlot();
            from.clearSlot();

            to.set_item(temp_item2);
            from.set_item(temp_item1);
        }
        else
        {
            to.set_item(from.GetItem());
            from.clearSlot();
        }
    }

}