using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class knapsackManager : MonoBehaviour
{
    [SerializeField] private GameObject bagSlot;
    public int stealthLevel = 0;
    public int bulk = 0;
    public GameObject container;
    public TextMeshProUGUI itemText;
    [SerializeField] private Slider StealthBar;
    [SerializeField] private Slider BulkBar;
    [SerializeField] private TextMeshProUGUI StealthValueText;
    [SerializeField] private TextMeshProUGUI BulkValueText;
    
    public bool puzzle_completed_flag = false;
    public int items_collected = 0;

    public float infoDisplay_start = 0f;
    public bool showInfo = false;
    [SerializeField]private TextMeshProUGUI infoText;

    public GameObject itemContainer;
    public KS_Inventory inventory;

    private void Start()
    {
        container.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Update()
    {
        //if E is pressed and inventory is open then close inventory

        
        if (container.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                
                
                toggleInventory();
            }
        }

        if (Time.time - infoDisplay_start > 5f)
            showInfo = false;
        
        displayInfo();
        checkBulkLevel();
    }


    public void toggleInventory()
    {
        //container contains the inventory so we toggle it on and off
        container.SetActive(!container.activeInHierarchy);
        GameManager.Instance.IsGamePaused = !GameManager.Instance.IsGamePaused;
        Cursor.lockState = GameManager.Instance.IsGamePaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = GameManager.Instance.IsGamePaused ? true : false;
    }

    //this function will be called when the player tries to interact with the door
    //but doesn't meet the requirements to escape.
    //It will display a message for 5 seconds to inform the player of what they need to do to escape.
    public void DisplayDoorMsg()
    {
        infoDisplay_start = Time.time;
        showInfo = true;
    }

    public void displayInfo()
    {
        if (showInfo)
        {
            if (items_collected < 10)
                infoText.text = "Collect all 10 items";
            else if (stealthLevel < 95)
                infoText.text = "Increase your stealth level to 95 or more";
            else // for debugging purpose this message will shown when player has completed the level
                infoText.text = "Player Escaped"; 
        }
        else
        {
            infoText.text = "";
        }
    }
    public void updateStats()
    {
        //updating the stealth and bulk values based on the items collected
        StealthBar.value = (float)stealthLevel; 
        StealthValueText.text = stealthLevel + "%";
        BulkBar.value = (float)bulk;
        BulkValueText.text = bulk + "KG";

        //checking if the player matched the win condition
        if(stealthLevel >= 95 && items_collected >= 10 && bulk < 50)
        {
            //player wins
            puzzle_completed();
        }
    }

    private void checkBulkLevel()
    {   if(bulk >= 50)
        {
            Debug.Log("You are too bulky to escape!");
            inventory.clearBagSlot();

        }
    }

    private void enableAllItems()
    {
        
        KS_ItemObj[] items = itemContainer.GetComponentsInChildren<KS_ItemObj>(true);
        foreach(KS_ItemObj item in items)
        {
            item.gameObject.SetActive(true);
            Debug.Log("Enabling slot: " + item.name);
        }
    }

    public void puzzle_completed()
    {
        puzzle_completed_flag = true;
        inventory.puzzle_completed();

    }


}
