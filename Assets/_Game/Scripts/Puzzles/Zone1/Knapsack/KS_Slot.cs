using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class KS_Slot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public Image slotImage;
    public bool isHovered;
    private KS_Item heldItem;
    public bool isBagSlot = false;
    [SerializeField] private knapsackManager KS_manager;
    private void Awake()
    {
        KS_manager = FindObjectOfType<knapsackManager>();
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
        updateStats();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        Debug.Log("Hovering over slot with item: " + (heldItem != null ? heldItem.itemName : "Empty Slot"));
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
        updateStats(true);
        heldItem = null;
        updateSlot();
        
        
    }

    public bool hasItem()
    {
        return heldItem != null;
    }

    public void updateStats(bool removeItem=false)
    {
        if (isBagSlot)
        {
            if (!removeItem)
            {
                KS_manager.stealthLevel += heldItem.stealthValue;
                KS_manager.bulk += heldItem.bulkValue;
            }
            else
            {
                KS_manager.stealthLevel -= heldItem.stealthValue;
                KS_manager.bulk -= heldItem.bulkValue;
            }

            KS_manager.updateStealth();


        }


    }

    


}
