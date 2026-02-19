using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 1. Inherit from BasePuzzleUI
public class KnapsackPuzzleUI : BasePuzzleUI
{
    public static KnapsackPuzzleUI Instance;

    // Buffer for items picked up BEFORE the UI is spawned
    public static List<KS_Item> PreCollectedItems = new List<KS_Item>();
    public static int GlobalItemsCollected = 0;

    [Header("Stats")]
    public int stealthLevel = 0;
    public int bulk = 0;
    public int items_collected = 0;

    [Header("UI References")]
    public KS_Inventory inventory;
    public TextMeshProUGUI itemText;
    [SerializeField] private Slider StealthBar;
    [SerializeField] private Slider BulkBar;
    [SerializeField] private TextMeshProUGUI StealthValueText;
    [SerializeField] private TextMeshProUGUI BulkValueText;
    //[SerializeField] private TextMeshProUGUI infoText;

    [Header("Containers")]
    [SerializeField] private GameObject bagPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject progressBarPanel;
    [SerializeField] private GameObject puzzleCompleteImg;

    private float infoDisplay_start = 0f;
    private bool showInfo = false;
    private bool _isSolved = false;

    // 2. Override OnSetup
    protected override void OnSetup()
    {
        Instance = this;
        if (puzzleCompleteImg) puzzleCompleteImg.SetActive(false);
        //if (infoText) infoText.text = "";

        // Load any items the player picked up before opening this terminal
        items_collected = GlobalItemsCollected;
        foreach (var item in PreCollectedItems)
        {
            inventory.AddItem(item);
        }
        PreCollectedItems.Clear(); // Empty the buffer

        updateStats();
    }

    private void Update()
    {
        if (showInfo && Time.time - infoDisplay_start > 5f)
        {
            showInfo = false;
            //if (infoText) infoText.text = "";
        }
    }

    public void updateStats()
    {
        if (_isSolved) return;

        if (StealthBar) StealthBar.value = stealthLevel;
        if (StealthValueText) StealthValueText.text = stealthLevel + "%";
        if (BulkBar) BulkBar.value = bulk;
        if (BulkValueText) BulkValueText.text = bulk + "KG";

        // Check fail state (Too heavy)
        if (bulk >= 50)
        {
            //if (infoText) infoText.text = "<color=red>Too heavy! Bag dropped.</color>";
            showInfo = true;
            infoDisplay_start = Time.time;
            inventory.clearBagSlot();
        }
        // Check win state
        else if (stealthLevel >= 95 && items_collected >= 10 && bulk < 50)
        {
            _isSolved = true;
            CompletePuzzle(); // Tells the Core Architecture we won!
        }
    }

    // 3. System forces us to write this: What happens when solved?
    protected override void OnShowSolvedState()
    {
        _isSolved = true;
        if (bagPanel) bagPanel.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (progressBarPanel) progressBarPanel.SetActive(false);
        if (puzzleCompleteImg) puzzleCompleteImg.SetActive(true);
        //if (infoText) infoText.text = "<color=green>KNAPSACK OPTIMIZED (SOLVED)</color>";
    }
}