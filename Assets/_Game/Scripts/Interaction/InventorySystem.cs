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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        isOpen = false;

        PopulateSlotList();
    }

    private void PopulateSlotList()
    {
        itemIcons.Clear();

        Image[] images = inventoryScreenUI.GetComponentsInChildren<UnityEngine.UI.Image>(true);

        foreach (UnityEngine.UI.Image img in images)
        {
            if (img.transform.parent.CompareTag("Slot"))
            {
                img.enabled = false;
                itemIcons.Add(img);
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
            Debug.Log("i is pressed");
            inventoryScreenUI.SetActive(true);
            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.I) && isOpen)
        {
            // We replaced the manual code here with a call to our new method
            CloseInventory();
        }
    }

    // NEW METHOD: This is public so your UI Button can see and trigger it!
    public void CloseInventory()
    {
        if (isOpen)
        {
            inventoryScreenUI.SetActive(false);
            isOpen = false;

            // Re-lock the cursor so the player can look around again
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
        itemList.Add(itemName);

        //this.transform.DOMove(Vector3.zero, );
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