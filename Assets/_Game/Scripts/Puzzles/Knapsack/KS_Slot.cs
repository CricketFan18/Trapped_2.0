using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KS_Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image slotImage;
    public bool isHovered;
    private KS_Item heldItem;
    public bool isBagSlot = false;

    private void Awake()
    {
        if (transform.childCount > 0)
            slotImage = transform.GetChild(0).GetComponent<Image>();
    }

    public KS_Item GetItem() => heldItem;

    public void set_item(KS_Item item)
    {
        heldItem = item;
        updateSlot();
        updateStats(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (heldItem != null && KnapsackPuzzleUI.Instance != null && KnapsackPuzzleUI.Instance.itemText != null)
        {
            KnapsackPuzzleUI.Instance.itemText.text = $"{heldItem.itemName}\nStealth: {heldItem.stealthValue}\nBulk: {heldItem.bulkValue}";
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (KnapsackPuzzleUI.Instance != null && KnapsackPuzzleUI.Instance.itemText != null)
        {
            KnapsackPuzzleUI.Instance.itemText.text = "";
        }
    }

    public void updateSlot()
    {
        if (slotImage == null) return;

        if (heldItem != null)
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

    public bool hasItem() => heldItem != null;

    public void updateStats(bool removeItem = false)
    {
        if (isBagSlot && heldItem != null && KnapsackPuzzleUI.Instance != null)
        {
            if (!removeItem)
            {
                KnapsackPuzzleUI.Instance.stealthLevel += heldItem.stealthValue;
                KnapsackPuzzleUI.Instance.bulk += heldItem.bulkValue;
            }
            else
            {
                KnapsackPuzzleUI.Instance.stealthLevel -= heldItem.stealthValue;
                KnapsackPuzzleUI.Instance.bulk -= heldItem.bulkValue;
            }
            KnapsackPuzzleUI.Instance.updateStats();
        }
    }
}