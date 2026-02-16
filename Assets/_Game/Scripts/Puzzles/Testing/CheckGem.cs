using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGem : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "The counterfeit belongs within";
    public Transform win;
    public Transform fail;
    public AudioClip alarmSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Gem selectedGem = other.GetComponent<Gem>();
        if (selectedGem)
        {
            if (selectedGem.fake)
            {
                Destroy(this);
                Debug.Log("Succeeed");
                win.gameObject.SetActive(true);
            }
            else
            {
                GemManager.instance.RemoveAllGems();
                TriggerAlarm();
            }
        }
    }
    
    public void TriggerAlarm()
    {
        audioSource.clip = alarmSound; audioSource.Play();
        GameManager.Instance.TimePenalty(300f);
    }
    
    bool IInteractable.Interact(Interactor interactor)
    {
        return true;
    }
}
