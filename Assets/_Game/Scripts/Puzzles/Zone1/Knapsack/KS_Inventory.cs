using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class KS_Inventory : MonoBehaviour
{
    public KS_Item item1;
    public KS_Item item2;
    public GameObject BagObj;
    public GameObject InventoryObj;

    private List<KS_Slot> bagSlots = new List<KS_Slot>();
    private List<KS_Slot> inventorySlots = new List<KS_Slot>();
    private List<KS_Slot> allSlots = new List<KS_Slot>();
    private void Awake()
    {
        inventorySlots.AddRange(InventoryObj.GetComponentsInChildren<KS_Slot>());
        bagSlots.AddRange(BagObj.GetComponentsInChildren<KS_Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(bagSlots);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            AddItem(item1);
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            AddItem(item2);
        }


    }

    private void AddItem(KS_Item item)
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
}
