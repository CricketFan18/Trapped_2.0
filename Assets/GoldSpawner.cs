using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GoldSpawner : MonoBehaviour
{
    public static GoldSpawner instance;
    public Transform goldBar;
    public Transform spawnLocation;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnGoldbar();
        }
    }

    public void SpawnGoldbar(int numberOfGoldbars = 0)
    {
        int n = ((numberOfGoldbars == 0) ? Random.Range(1, 3) : 1);
        for (int i = 0; i < n; i++)
        {
            Instantiate(goldBar, spawnLocation.position, Quaternion.identity);
        }
    }
}
