using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class KS_Slot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    private Image slotImage;
    private bool isHovered;
    private KS_Item heldItem;
    private void Awake()
    {
        slotImage = transform.GetChild(0).GetComponent<Image>();

    }

    public KS_Item GetItem()
    {
        return heldItem;
    }

    public void set_item(KS_Item item)
    {
        heldItem = item;
        updateSlot();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void updateSlot()
    {
        if(heldItem != null)
        {
            slotImage.enabled = true;
            slotImage.sprite = heldItem.icon;
            slotImage.color = Color.white;
        }
        else
        {
            slotImage.enabled = false;
        }
    }

    public void clearSlot()
    {
        heldItem = null;
        updateSlot();
    }

    public bool hasItem()
    {
        return heldItem != null;
    }
}
