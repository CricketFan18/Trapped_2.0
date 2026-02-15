using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;
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

        //Toggle Inventory
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //container contains the inventory so we toggle it on and off
            container.SetActive(!container.activeInHierarchy);
            GameManager.Instance.IsGamePaused = !GameManager.Instance.IsGamePaused;
            Cursor.lockState = GameManager.Instance.IsGamePaused ? CursorLockMode.None:CursorLockMode.Locked;
            Cursor.visible = GameManager.Instance.IsGamePaused ? true : false;
        }
        checkBulkLevel();

        
        if (Time.time - infoDisplay_start > 5f)
            showInfo = false;
        
        displayInfo();
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
            if (items_collected < 4)
                infoText.text = "Collect all 10 items";
            else if (stealthLevel < 95)
                infoText.text = "Increase your stealth level to 95 or more";
        }
        else
        {
            infoText.text = "";
        }
    }
    public void updateStats()
    {
        StealthBar.value = (float)stealthLevel;
        BulkBar.value = (float)bulk;
        Debug.Log("Stealth Level: " + stealthLevel + "Bulk Level" + bulk);
    }

    private void checkBulkLevel()
    {   if(bulk >= 50)
        {
            Debug.Log("You are too bulky to escape!");
            items_collected = 0;
            enableAllItems();

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
        inventory.clearAllSlots();
    }

}
