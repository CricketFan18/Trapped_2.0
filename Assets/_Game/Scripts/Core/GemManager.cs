using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GemManager : MonoBehaviour
{
    public bool holding = false;
    public Gem heldGem;
    public static GemManager instance;
    public Transform cam;
    public List<Gem> gems = new List<Gem>();

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
            heldGem.PlaceGem();
        }
    }
}
