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
    public Button craftingCloseButton;
    public GameObject craftButtonUI;

    [Header("Feedback Messages")]
    public GameObject failMessageUI;
    public GameObject insufficientMessageUI;
    public GameObject successMessageUI; // NEW: Drag your green "Success!" text here

    [Header("Crafting Setup")]
    public CraftingSlot[] inputSlots;
    public Transform resultSlot; // The dark Armed C4 background box
    public Sprite c4ResultSprite;

    [Header("Position Settings")]
    public float shiftedXPosition = -100f;

    private float originalXPosition;
    public bool isCraftingActive = false;

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

        if (failMessageUI != null) failMessageUI.SetActive(false);
        if (insufficientMessageUI != null) insufficientMessageUI.SetActive(false);
        if (successMessageUI != null) successMessageUI.SetActive(false);

        // DEFAULT STATE: Hide the result box, Show the craft button
        if (resultSlot != null) resultSlot.gameObject.SetActive(false);
        if (craftButtonUI != null) craftButtonUI.SetActive(true);

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

    public void OnItemPlaced() { }

    public void TryCraft()
    {
        int count = 0;
        for (int i = 0; i < inputSlots.Length; i++)
        {
            if (inputSlots[i] != null && inputSlots[i].transform.childCount > 0) count++;
        }

        if (count < 6)
        {
            if (insufficientMessageUI != null) StartCoroutine(ShowMessageRoutine(insufficientMessageUI));
        }
        else
        {
            CheckRecipe();
        }
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
            // --- SUCCESS ---
            if (successMessageUI != null) StartCoroutine(ShowMessageRoutine(successMessageUI));

            // Show the result box, Hide the craft button!
            if (resultSlot != null) resultSlot.gameObject.SetActive(true);
            if (craftButtonUI != null) craftButtonUI.SetActive(false);

            GameObject c4Item = null;

            for (int i = 0; i < inputSlots.Length; i++)
            {
                while (inputSlots[i].transform.childCount > 0)
                {
                    GameObject item = inputSlots[i].transform.GetChild(0).gameObject;

                    if (c4Item == null)
                    {
                        c4Item = item;
                        Image img = c4Item.GetComponent<Image>();
                        if (img != null && InventorySystem.Instance != null)
                        {
                            int idx = InventorySystem.Instance.itemIcons.IndexOf(img);
                            if (idx >= 0) InventorySystem.Instance.itemList[idx] = "Armed C4";
                            img.sprite = c4ResultSprite;
                            img.enabled = true;
                        }

                        if (resultSlot != null)
                        {
                            c4Item.transform.SetParent(resultSlot);
                            c4Item.transform.SetAsLastSibling(); // Ensures C4 renders on top of the dark box

                            RectTransform r = c4Item.GetComponent<RectTransform>();
                            if (r != null) { r.anchoredPosition = Vector2.zero; r.localScale = Vector3.one; }
                            c4Item.transform.localPosition = Vector3.zero;
                        }

                        temporaryItemStorage.Remove(c4Item);
                    }
                    else
                    {
                        Image img = item.GetComponent<Image>();
                        if (img != null && InventorySystem.Instance != null)
                        {
                            int idx = InventorySystem.Instance.itemIcons.IndexOf(img);
                            if (idx >= 0) InventorySystem.Instance.itemList[idx] = "";
                            img.sprite = null;
                            img.enabled = false;
                        }

                        if (temporaryItemStorage.TryGetValue(item, out Transform home) && home != null)
                        {
                            item.transform.SetParent(home);
                            RectTransform r = item.GetComponent<RectTransform>();
                            if (r != null) { r.anchoredPosition = Vector2.zero; r.localScale = Vector3.one; }
                            item.transform.localPosition = Vector3.zero;
                        }
                        temporaryItemStorage.Remove(item);
                    }
                }
                if (inputSlots[i] != null) inputSlots[i].currentItemName = "";
            }
        }
        else
        {
            // --- FAILED ---
            if (failMessageUI != null) StartCoroutine(ShowMessageRoutine(failMessageUI));
            ResetIngredients();
        }
    }

    private System.Collections.IEnumerator ShowMessageRoutine(GameObject messageUI)
    {
        messageUI.SetActive(true);
        yield return new WaitForSeconds(2f);
        if (messageUI != null) messageUI.SetActive(false);
    }

    public void ResetIngredients()
    {
        List<Transform> invSlots = new List<Transform>();
        if (InventorySystem.Instance != null && InventorySystem.Instance.inventoryScreenUI != null)
        {
            Transform[] allUI = InventorySystem.Instance.inventoryScreenUI.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allUI)
            {
                if (t.CompareTag("Slot") && t.GetComponent<CraftingSlot>() == null) invSlots.Add(t);
            }
        }

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
                        if (invSlot.childCount == 0) { item.SetParent(invSlot); placed = true; break; }
                    }
                }

                if (!placed && InventorySystem.Instance != null)
                    item.SetParent(InventorySystem.Instance.inventoryScreenUI.transform);

                RectTransform rect = item.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                    rect.localScale = Vector3.one;
                }
                item.localPosition = Vector3.zero;

                temporaryItemStorage.Remove(item.gameObject);
            }
            if (slot != null) slot.currentItemName = "";
        }

        if (resultSlot != null)
        {
            while (resultSlot.childCount > 0)
            {
                Transform abandonedC4 = resultSlot.GetChild(0);
                bool placed = false;
                foreach (Transform invSlot in invSlots)
                {
                    if (invSlot.childCount == 0) { abandonedC4.SetParent(invSlot); placed = true; break; }
                }
                if (!placed && InventorySystem.Instance != null)
                    abandonedC4.SetParent(InventorySystem.Instance.inventoryScreenUI.transform);

                RectTransform rect = abandonedC4.GetComponent<RectTransform>();
                if (rect != null) { rect.anchoredPosition = Vector2.zero; rect.localScale = Vector3.one; }
                abandonedC4.localPosition = Vector3.zero;
            }
        }

        // RESET UI STATE
        if (resultSlot != null) resultSlot.gameObject.SetActive(false);
        if (craftButtonUI != null) craftButtonUI.SetActive(true);
    }

    public void OpenCraftingStation()
    {
        if (isCraftingActive) return;

        isCraftingActive = true;
        craftingUIPanel.SetActive(true);

        // ENSURE UI IS IN DEFAULT STATE WHEN OPENED
        if (resultSlot != null) resultSlot.gameObject.SetActive(false);
        if (craftButtonUI != null) craftButtonUI.SetActive(true);
        if (successMessageUI != null) successMessageUI.SetActive(false);

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

    public void CloseCraftingStation()
    {
        if (!isCraftingActive) return;

        ResetIngredients();

        isCraftingActive = false;

        if (craftingUIPanel != null && craftingUIPanel.activeSelf) craftingUIPanel.SetActive(false);
        if (failMessageUI != null) failMessageUI.SetActive(false);
        if (insufficientMessageUI != null) insufficientMessageUI.SetActive(false);
        if (successMessageUI != null) successMessageUI.SetActive(false);

        if (inventoryRectTransform != null)
        {
            Vector2 pos = inventoryRectTransform.anchoredPosition;
            pos.x = originalXPosition;
            inventoryRectTransform.anchoredPosition = pos;
        }

        if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) InventorySystem.Instance.CloseInventory();
    }
}