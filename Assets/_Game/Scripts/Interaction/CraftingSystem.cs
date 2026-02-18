using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [Header("UI References")]
    public GameObject craftingUIPanel;
    public RectTransform inventoryRectTransform;
    public GameObject inventoryCloseButton;

    [Header("Optional: Drag your Crafting Close Button here")]
    public Button craftingCloseButton;

    [Header("Crafting Setup")]
    public CraftingSlot[] inputSlots;

    [Tooltip("Drag the Parent GameObject of your Result Slot here")]
    public Transform resultSlot; // CHANGED to Transform so it acts as a physical container
    public Sprite c4ResultSprite;

    [Header("Position Settings")]
    public float shiftedXPosition = -100f;

    private float originalXPosition;
    public bool isCraftingActive = false;

    // THE TEMPORARY BLOCK
    public Dictionary<GameObject, Transform> temporaryItemStorage = new Dictionary<GameObject, Transform>();
    private readonly string[] c4Recipe = { "Casing", "Battery", "Timer", "Detonator", "Wires", "Mercury Switch" };

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (inventoryRectTransform != null) originalXPosition = inventoryRectTransform.anchoredPosition.x;
        craftingUIPanel.SetActive(false);

        if (craftingCloseButton != null)
            craftingCloseButton.onClick.AddListener(CloseCraftingStation);
    }

    void Update()
    {
        if (!isCraftingActive) return;

        if (InventorySystem.Instance != null && !InventorySystem.Instance.isOpen)
            CloseCraftingStation();
        else if (craftingUIPanel != null && !craftingUIPanel.activeInHierarchy)
            CloseCraftingStation();
    }

    public void OpenCraftingStation()
    {
        if (isCraftingActive) return;

        isCraftingActive = true;
        craftingUIPanel.SetActive(true);
        if (inventoryCloseButton != null) inventoryCloseButton.SetActive(false);

        if (InventorySystem.Instance != null && !InventorySystem.Instance.isOpen)
        {
            InventorySystem.Instance.isOpen = true;
            if (InventorySystem.Instance.inventoryScreenUI != null)
                InventorySystem.Instance.inventoryScreenUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (inventoryRectTransform != null)
        {
            Vector2 pos = inventoryRectTransform.anchoredPosition;
            pos.x = originalXPosition + shiftedXPosition;
            inventoryRectTransform.anchoredPosition = pos;
        }
    }

    public void OnItemPlaced()
    {
        int count = 0;
        for (int i = 0; i < inputSlots.Length; i++)
        {
            if (inputSlots[i] != null && inputSlots[i].transform.childCount > 0) count++;
        }

        if (count == 6) CheckRecipe();
    }

    private void CheckRecipe()
    {
        bool isCorrect = true;
        for (int i = 0; i < c4Recipe.Length; i++)
        {
            if (inputSlots[i] != null && inputSlots[i].currentItemName != c4Recipe[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            GameObject c4Item = null;

            for (int i = 0; i < inputSlots.Length; i++)
            {
                while (inputSlots[i].transform.childCount > 0)
                {
                    GameObject item = inputSlots[i].transform.GetChild(0).gameObject;

                    if (c4Item == null)
                    {
                        // 1. TRANSMUTE the first ingredient into the physical Armed C4
                        c4Item = item;
                        Image img = c4Item.GetComponent<Image>();
                        if (img != null && InventorySystem.Instance != null)
                        {
                            int idx = InventorySystem.Instance.itemIcons.IndexOf(img);
                            // Register the new name in the inventory data
                            if (idx >= 0) InventorySystem.Instance.itemList[idx] = "Armed C4";

                            img.sprite = c4ResultSprite;
                            img.enabled = true;
                        }

                        // 2. Move it to the Result Slot
                        if (resultSlot != null)
                        {
                            c4Item.transform.SetParent(resultSlot);
                            c4Item.transform.localPosition = Vector3.zero;
                            c4Item.transform.localScale = Vector3.one;
                        }

                        // Unlink it from the auto-return memory so it doesn't get ripped out of your hands
                        temporaryItemStorage.Remove(c4Item);
                    }
                    else
                    {
                        // 3. CONSUME the other 5 ingredients invisibly (protects inventory capacity)
                        Image img = item.GetComponent<Image>();
                        if (img != null && InventorySystem.Instance != null)
                        {
                            int idx = InventorySystem.Instance.itemIcons.IndexOf(img);
                            if (idx >= 0) InventorySystem.Instance.itemList[idx] = "";
                            img.sprite = null;
                            img.enabled = false;
                        }

                        // Send the empty ghosts back to their homes silently
                        if (temporaryItemStorage.TryGetValue(item, out Transform home) && home != null)
                        {
                            item.transform.SetParent(home);
                            item.transform.localPosition = Vector3.zero;
                            item.transform.localScale = Vector3.one;
                        }
                        temporaryItemStorage.Remove(item);
                    }
                }
                if (inputSlots[i] != null) inputSlots[i].currentItemName = "";
            }
        }
        else
        {
            ResetIngredients();
        }
    }

    public void ResetIngredients()
    {
        List<Transform> invSlots = new List<Transform>();
        if (InventorySystem.Instance != null && InventorySystem.Instance.inventoryScreenUI != null)
        {
            Transform[] allUI = InventorySystem.Instance.inventoryScreenUI.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allUI)
            {
                if (t.CompareTag("Slot") && t.GetComponent<CraftingSlot>() == null)
                    invSlots.Add(t);
            }
        }

        // 1. Return all active ingredients
        for (int i = 0; i < inputSlots.Length; i++)
        {
            CraftingSlot slot = inputSlots[i];

            while (slot != null && slot.transform.childCount > 0)
            {
                Transform item = slot.transform.GetChild(0);
                bool placed = false;

                if (temporaryItemStorage.TryGetValue(item.gameObject, out Transform homeSlot) && homeSlot != null && homeSlot.childCount == 0)
                {
                    item.SetParent(homeSlot);
                    placed = true;
                }

                if (!placed)
                {
                    foreach (Transform invSlot in invSlots)
                    {
                        if (invSlot.childCount == 0)
                        {
                            item.SetParent(invSlot);
                            placed = true;
                            break;
                        }
                    }
                }

                if (!placed && InventorySystem.Instance != null)
                    item.SetParent(InventorySystem.Instance.inventoryScreenUI.transform);

                RectTransform rect = item.GetComponent<RectTransform>();
                if (rect != null) { rect.anchoredPosition = Vector2.zero; rect.localScale = Vector3.one; }
                item.localPosition = Vector3.zero;
                item.localScale = Vector3.one;

                temporaryItemStorage.Remove(item.gameObject);
            }
            if (slot != null) slot.currentItemName = "";
        }

        // 2. NEW: Force grab any abandoned C4 left in the result slot and put it in the inventory
        if (resultSlot != null)
        {
            while (resultSlot.childCount > 0)
            {
                Transform abandonedC4 = resultSlot.GetChild(0);
                bool placed = false;
                foreach (Transform invSlot in invSlots)
                {
                    if (invSlot.childCount == 0)
                    {
                        abandonedC4.SetParent(invSlot);
                        placed = true;
                        break;
                    }
                }
                if (!placed && InventorySystem.Instance != null)
                    abandonedC4.SetParent(InventorySystem.Instance.inventoryScreenUI.transform);

                abandonedC4.localPosition = Vector3.zero;
                abandonedC4.localScale = Vector3.one;
            }
        }
    }

    public void CloseCraftingStation()
    {
        if (!isCraftingActive) return;

        ResetIngredients();

        isCraftingActive = false;

        if (craftingUIPanel != null && craftingUIPanel.activeSelf)
            craftingUIPanel.SetActive(false);

        if (inventoryCloseButton != null) inventoryCloseButton.SetActive(true);

        if (inventoryRectTransform != null)
        {
            Vector2 pos = inventoryRectTransform.anchoredPosition;
            pos.x = originalXPosition;
            inventoryRectTransform.anchoredPosition = pos;
        }

        if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen)
        {
            InventorySystem.Instance.CloseInventory();
        }
    }
}