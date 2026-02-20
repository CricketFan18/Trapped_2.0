using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSystem : BasePuzzleUI
{
    public static CraftingSystem Instance { get; private set; }

    [Header("Screens")]
    public GameObject InventoryScreen; // Left side
    public GameObject CraftingScreen;  // Right side

    [Header("Buttons & Feedback")]
    public Button closeButton;
    public GameObject craftButtonUI;
    public GameObject failMessageUI;
    public GameObject insufficientMessageUI;
    public GameObject successMessageUI;

    [Header("Crafting Setup")]
    public CraftingSlot[] inputSlots;
    public Transform resultSlot;
    public Sprite c4ResultSprite;

    public Dictionary<GameObject, Transform> temporaryItemStorage = new Dictionary<GameObject, Transform>();
    private readonly string[] c4Recipe = { "Casing", "Numpad", "Timer", "Detonator", "Wires", "Mercury Switch" };

    private bool _isSolved = false;
    private bool waitingForPickup = false;

    private void Awake() { Instance = this; }

    protected override void OnSetup()
    {
        // SAFETY CHECK: Warn us if the Inspector is not set up!
        if (InventoryScreen == null || CraftingScreen == null)
        {
            Debug.LogError("[CraftingSystem] ERROR: InventoryScreen or CraftingScreen is not assigned in the Inspector!");
            return;
        }

        if (failMessageUI) failMessageUI.SetActive(false);
        if (insufficientMessageUI) insufficientMessageUI.SetActive(false);
        if (successMessageUI) successMessageUI.SetActive(false);
        if (resultSlot) resultSlot.gameObject.SetActive(false);
        if (craftButtonUI) craftButtonUI.SetActive(true);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => UIManager.Instance.CloseActivePuzzle());
        }

        PopulateInventoryUI();
    }

    public void PopulateInventoryUI()
    {
        Debug.Log("[CraftingSystem] Populating Inventory UI");
        if (InventorySystem.Instance == null || InventoryScreen == null) return;

        List<Image> invIcons = new List<Image>();
        Image[] allImages = InventoryScreen.GetComponentsInChildren<Image>(true);

        foreach (Image img in allImages)
        {
            if (img.transform.parent != null)
            {
                if (img.transform.parent.CompareTag("Slot"))
                {
                    // Ensure it is a pure inventory slot (no CraftingSlot script)
                    if (img.transform.parent.GetComponent<CraftingSlot>() == null)
                    {
                        img.enabled = false;
                        invIcons.Add(img);
                    }
                }
            }
        }

        // Fill the slots with collected items
        for (int i = 0; i < InventorySystem.Instance.CollectedItemsNames.Count; i++)
        {
            if (i < invIcons.Count)
            {
                invIcons[i].sprite = InventorySystem.Instance.CollectedItemSprites[i];

                // Force visibility just in case the prefab was saved hidden or transparent
                invIcons[i].enabled = true;
                invIcons[i].gameObject.SetActive(true);
                invIcons[i].color = Color.white;

                invIcons[i].gameObject.tag = "Item";

                ItemData data = invIcons[i].gameObject.GetComponent<ItemData>();
                if (data == null) data = invIcons[i].gameObject.AddComponent<ItemData>();
                data.ItemName = InventorySystem.Instance.CollectedItemsNames[i];
            }
        }
    }

    private void OnDisable()
    {
        if (_isSolved) return;
        ResetIngredients();
    }

    public void TryCraft()
    {
        int count = 0;
        foreach (var slot in inputSlots)
        {
            if (slot != null && slot.transform.childCount > 0) count++;
        }

        if (count < 6)
        {
            if (insufficientMessageUI) StartCoroutine(ShowMessageRoutine(insufficientMessageUI));
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
            _isSolved = true;
            CompletePuzzle();
            GenerateC4Visuals();
            
        }
        else
        {
            if (failMessageUI) StartCoroutine(ShowMessageRoutine(failMessageUI));
            ResetIngredients();
        }
    }

    protected override void OnShowSolvedState()
    {
        if (successMessageUI) successMessageUI.SetActive(true);
        if (resultSlot) resultSlot.gameObject.SetActive(true);
        if (craftButtonUI) craftButtonUI.SetActive(false);
    }

    private void GenerateC4Visuals()
    {
        GameObject c4Item = null;

        for (int i = 0; i < inputSlots.Length; i++)
        {
            for (int j = inputSlots[i].transform.childCount - 1; j >= 0; j--)
            {
                GameObject item = inputSlots[i].transform.GetChild(j).gameObject;

                if (c4Item == null)
                {
                    c4Item = item;

                    Image img = c4Item.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = c4ResultSprite;
                        img.enabled = true;
                    }

                    if (resultSlot != null)
                    {
                        c4Item.transform.SetParent(resultSlot);
                        c4Item.transform.localPosition = Vector3.zero;

                        ItemData data = c4Item.GetComponent<ItemData>();
                        if (data != null)
                            data.ItemName = "Armed C4";
                    }
                    OnC4Crafted();
                }
                else
                {
                    Destroy(item);
                }
            }

            if (inputSlots[i] != null)
                inputSlots[i].currentItemName = "";
        }
    }

    private IEnumerator ShowMessageRoutine(GameObject messageUI)
    {
        messageUI.SetActive(true);
        yield return new WaitForSeconds(2f);
        if (messageUI) messageUI.SetActive(false);
    }
    
    private void OnC4Crafted()
    {
        resultSlot.parent = transform;
        RectTransform c4Rect = resultSlot.GetComponent<RectTransform>();
        resultSlot.GetChild(0).tag = "Untagged";
        float slideDistance = 1500f; float animationDuration = 0.6f; 
        
        //Sliding Inventory And Crafting Screen
        InventoryScreen.transform.DOMoveX(InventoryScreen.transform.position.x - slideDistance, animationDuration).SetEase(Ease.InOutCubic);
        CraftingScreen.transform.DOMoveX(InventoryScreen.transform.position.x + slideDistance, animationDuration).SetEase(Ease.InOutCubic);
        
        resultSlot.GetComponent<Image>().enabled = false;
        TMP_Text successText = successMessageUI.gameObject.GetComponentInChildren<TMP_Text>();
        successText.alpha = 0;
        
        // Move C4 to center
        c4Rect.DOAnchorPos(Vector2.zero, animationDuration)
            .SetEase(Ease.InOutCubic);

        // Scale C4
        resultSlot.DOScale(5f, animationDuration)
            .SetEase(Ease.OutBack).OnComplete(() =>
            {
                waitingForPickup = true;
            });
        
        successText.DOFade(1, animationDuration).SetEase(Ease.OutBack);
    }
    
    public void ResetIngredients()
    {
        // SAFETY CHECK: Do nothing if the screen isn't assigned
        if (InventoryScreen == null) return;

        List<Transform> invSlots = new List<Transform>();
        Transform[] allUI = InventoryScreen.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allUI)
        {
            if (t.CompareTag("Slot")) invSlots.Add(t);
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

                RectTransform rect = item.GetComponent<RectTransform>();
                if (rect != null) { rect.anchoredPosition = Vector2.zero; rect.localScale = Vector3.one; }
                item.localPosition = Vector3.zero;

                temporaryItemStorage.Remove(item.gameObject);
            }
            if (slot) slot.currentItemName = "";
        }
    }

    private void Update()
    {
        if (waitingForPickup && Input.GetKeyDown(KeyCode.E))
        {
            waitingForPickup = false;

            InventorySystem.Instance.CollectItem("Armed C4", c4ResultSprite);

            UIManager.Instance.CloseActivePuzzle();
        }
    }
}