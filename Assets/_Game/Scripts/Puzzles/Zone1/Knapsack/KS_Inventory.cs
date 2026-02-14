using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class KS_Inventory : MonoBehaviour
{
    public KS_Item item1;
    public KS_Item item2;
    public GameObject BagObj;
    public GameObject InventoryObj;

    public Image draggedSlotImg;
    public KS_Slot draggedSlot = null;
    private bool isDragging = false;

    private int items_collected = 0;
    private List<KS_Slot> bagSlots = new List<KS_Slot>();
    private List<KS_Slot> inventorySlots = new List<KS_Slot>();
    private List<KS_Slot> allSlots = new List<KS_Slot>();
    private void Awake()
    {
        draggedSlotImg.enabled = false;
        inventorySlots.AddRange(InventoryObj.GetComponentsInChildren<KS_Slot>());
        bagSlots.AddRange(BagObj.GetComponentsInChildren<KS_Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(bagSlots);
    }

    private void Update()
    {
        startDrag();
        updateDragIcon();
        endDrag();

    }

    public void AddItem(KS_Item item)
    {
        foreach (KS_Slot slot in allSlots)
        {
            if(slot.hasItem() == false)
            {
                slot.set_item(item);
                break;
            }
        }
    }


    private void updateDragIcon()
    {
        if(isDragging)
        {
            Vector2 mousePos = Input.mousePosition;
            draggedSlotImg.transform.position = mousePos;

        }
    }
    private void startDrag()
    {
        if(Input.GetMouseButtonDown(0))
        {
            
            KS_Slot hovered_slot = GetHoveredSlot();
            Debug.Log(hovered_slot!=null);
            if (hovered_slot != null && hovered_slot.hasItem())
            {
                draggedSlot = hovered_slot;
                isDragging = true;

                draggedSlotImg.enabled = true;
                draggedSlotImg.sprite = draggedSlot.GetItem().icon;
                draggedSlotImg.color = new Color(1, 1, 1, 0.5f);
                
            }
        }
    }

    private void endDrag()
    {
        if(Input.GetMouseButtonUp(0) && isDragging)
        {
            KS_Slot hovered_slot = GetHoveredSlot();
            if (hovered_slot != null)
            {
                handleDropSlot(draggedSlot, hovered_slot);
            }
            draggedSlotImg.enabled = false;
            draggedSlot = null;
            isDragging = false;
        }
    }
    private KS_Slot GetHoveredSlot()
    {
        foreach (KS_Slot slot in allSlots)
        {
            if(slot.isHovered)
            {
                return slot;
            }
        }

        return null;
    }

    private void handleDropSlot(KS_Slot from,KS_Slot to)
    {
        if (from == to) return;

        if(to.hasItem()) // if moving to a slot that already has an item then we will swap them
        {
            KS_Item temp_item1 = to.GetItem();
            KS_Item temp_item2 = from.GetItem();

            //if its bag slot then it will update stealth and bulk values
            //so we need to remove the old item values before swapping
            to.clearSlot();
            from.clearSlot();

            to.set_item(temp_item2);
            from.set_item(temp_item1);

        }
        else // else we will just move the item to the new slot
        {
            to.set_item(from.GetItem());
            from.clearSlot();
        }
    }
}
