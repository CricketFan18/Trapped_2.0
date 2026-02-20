using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GemManager : MonoBehaviour
{
    public bool holding = false;
    public Gem heldGem;
    public static GemManager instance;
    public int pickupFrame;
    public int placeFrame;
    public Transform cam;
    public List<Gem> gems = new List<Gem>();
    public Transform spawnerButton;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void MakeFake()
    {
        int randomIndex = Random.Range(0, gems.Count);
        gems[randomIndex].fake = true;
        gems[randomIndex].weight += 5;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && holding)
        {
            Debug.Log("Place");
            heldGem.PlaceGem();
        }
    }

    public void RemoveAllGems()
    {
        while (gems.Count > 0)
        {
            Gem gem = gems[0];
            gems.RemoveAt(0);
            Destroy(gem.gameObject);
        }
    }
}
