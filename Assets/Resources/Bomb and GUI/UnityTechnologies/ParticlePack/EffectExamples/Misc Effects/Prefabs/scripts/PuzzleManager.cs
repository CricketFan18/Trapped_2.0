using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;
    public List<Switch> allSwitches = new List<Switch>();
    public int maxActiveSwitches = 3;
    private float lastResetTime = 0f;
    public bool canInteract = true;
    public TextMeshPro TimerText;
    private int timeInt = 0;
    private float maxTime = 61f; //cooldown time in seconds
    [SerializeField]private Switch s1;
    [SerializeField] private Switch s2;
    [SerializeField] private Switch s3;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Switch[] foundSwitches = FindObjectsOfType<Switch>();
        allSwitches.AddRange(foundSwitches);
    }

    private void Update()
    {

        //timer
        if(!canInteract && Time.time - lastResetTime < maxTime)
        {
            timeInt = (int)(Time.time - lastResetTime);
            if (TimerText) TimerText.text = "Countdown" + "\n     " + timeInt;
        }
        else if(!canInteract && Time.time - lastResetTime > maxTime)
        {
            canInteract = true;
            if (TimerText) TimerText.text = "";
        }
    }
    public void CheckSwitchLimit()
    {
        int count = 0;
        foreach (Switch s in allSwitches)
        {
            if (s.isOn) count++;
        }

        //check win condition
        if (s1.isOn && s2.isOn && s3.isOn)
        {
            TimerText.text = "Solved!"; //using timer text to display win message
            GameManager.Instance.AddScore(100, "SwitchPuzzle");
        }
        else
        {
            if (count > maxActiveSwitches)
            {
                Debug.Log("OVERLOAD! Resetting all switches...");
                ResetAll();
            }

        }

    }
    void ResetAll()
    {

        lastResetTime = Time.time;
        foreach (Switch s in allSwitches)
        {
            s.TurnOff(); 
        }
        canInteract = false;
    }
}
