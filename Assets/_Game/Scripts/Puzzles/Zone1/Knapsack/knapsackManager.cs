using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class knapsackManager : MonoBehaviour
{
    [SerializeField] private GameObject bagSlot;
    public int stealthLevel = 0;
    public int bulk = 0;
    public GameObject container;

    private void Awake()
    {
        container.SetActive(true);
    }
    private void Start()
    {
        container.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }
    public void Update()
    {

        //Toggle Inventory
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //it contains the inventory so we toggle it on and off
            container.SetActive(!container.activeInHierarchy);
            GameManager.Instance.IsGamePaused = !GameManager.Instance.IsGamePaused;
            Cursor.lockState = GameManager.Instance.IsGamePaused ? CursorLockMode.None:CursorLockMode.Locked;
            Cursor.visible = GameManager.Instance.IsGamePaused ? true : false;
        }
        checkBulkLevel();
    }
    public void updateStealth()
    {
        Debug.Log("Stealth Level: " + stealthLevel + "Bulk Level" + bulk);
    }

    private void checkBulkLevel()
    {   if(bulk >= 50)
        {
            Debug.Log("You are too bulky to escape!");
            GameManager.Instance.GameOver();
        }
    }
}
