using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject drop = eventData.pointerDrag;
        DraggableItem draggableItem;
        if (!gameObject.GetComponentInChildren<Image>().IsActive())
        {
            draggableItem = drop.GetComponent<DraggableItem>();
            draggableItem.parentAfterDrag = transform;
        }
    }
}
