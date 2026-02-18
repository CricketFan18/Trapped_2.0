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
    public List<string> itemList = new List<string>(); // We will sync this perfectly now
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

        // NEW: Automatically hide the inventory screen when the game starts
        if (inventoryScreenUI != null)
        {
            inventoryScreenUI.SetActive(false);
        }
    }

    private void PopulateSlotList()
    {
        itemIcons.Clear();
        itemList.Clear(); // Clear names too

        Image[] images = inventoryScreenUI.GetComponentsInChildren<UnityEngine.UI.Image>(true);

        foreach (UnityEngine.UI.Image img in images)
        {
            if (img.transform.parent.CompareTag("Slot"))
            {
                img.enabled = false;
                itemIcons.Add(img);
                itemList.Add(""); // FIX: Create an empty string for every single slot
            }
        }

        Debug.Log("Item icon slots found: " + itemIcons.Count);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inventoryScreenUI.SetActive(true);
            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.I) && isOpen)
        {
            CloseInventory();
        }
    }

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

        // FIX: Assign the name to the EXACT same index as the visual icon!
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