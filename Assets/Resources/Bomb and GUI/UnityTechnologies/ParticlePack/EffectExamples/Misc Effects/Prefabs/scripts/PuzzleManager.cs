using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;
    public List<Switch> allSwitches = new List<Switch>();
    public int maxActiveSwitches = 3;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Switch[] foundSwitches = FindObjectsOfType<Switch>();
        allSwitches.AddRange(foundSwitches);
    }
    public void CheckSwitchLimit()
    {
        int count = 0;
        foreach (Switch s in allSwitches)
        {
            if (s.isOn) count++;
        }
        if (count > maxActiveSwitches)
        {
            Debug.Log("OVERLOAD! Resetting all switches...");
            ResetAll();
        }
    }
    void ResetAll()
    {
        foreach (Switch s in allSwitches)
        {
            s.TurnOff(); 
        }
    }
}
