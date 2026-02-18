using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; set; }

    public GameObject inventoryScreenUI;
    public List<GameObject> slotList = new List<GameObject>();
    public List<string> itemList = new List<string>();
    private GameObject itemToAdd;
    private int whatSlotToEquip;
    public bool isOpen;
    public Sprite srcImg;
    public List<UnityEngine.UI.Image> slotImages = new List<UnityEngine.UI.Image>();
    public List<UnityEngine.UI.Image> itemIcons = new List<UnityEngine.UI.Image>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        isOpen = false;
        PopulateSlotList();

        // Automatically hide the inventory screen when the game starts
        if (inventoryScreenUI != null)
        {
            inventoryScreenUI.SetActive(false);
        }
    }

    private void PopulateSlotList()
    {
        itemIcons.Clear();
        itemList.Clear();

        Image[] images = inventoryScreenUI.GetComponentsInChildren<UnityEngine.UI.Image>(true);

        foreach (UnityEngine.UI.Image img in images)
        {
            if (img.transform.parent.CompareTag("Slot"))
            {
                img.enabled = false;
                itemIcons.Add(img);
                itemList.Add(""); // Create an empty string for every single slot
            }
        }

        Debug.Log("Item icon slots found: " + itemIcons.Count);
    }

    // The Update method that checked for Input.GetKeyDown(KeyCode.I) has been removed.
    // The player can no longer open this manually.

    public void CloseInventory()
    {
        if (isOpen)
        {
            inventoryScreenUI.SetActive(false);
            isOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void AddToInventory(string itemName, Sprite iconSprite)
    {
        int slotIndex = FindNextEmptySlot();

        if (slotIndex == -1)
        {
            Debug.Log("Inventory full");
            return;
        }

        itemIcons[slotIndex].sprite = iconSprite;
        itemIcons[slotIndex].enabled = true;
        itemIcons[slotIndex].gameObject.SetActive(true);

        // Assign the name to the EXACT same index as the visual icon
        itemList[slotIndex] = itemName;
    }

    private int FindNextEmptySlot()
    {
        for (int i = 0; i < itemIcons.Count; i++)
        {
            if (!itemIcons[i].enabled)
                return i;
        }
        return -1;
    }
}