using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    //Put all level based functions here
    
    public Transform gemPrefab;
    public Transform spawnPoint;

    public AudioClip alarmSound;
    public static LevelScript instance;
    
    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null) instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public string InteractionPrompt => "Press E to Spawn Gems";
    
    public void SpawnGems(Button button)
    {
        button.GetComponent<AudioSource>().Play();
        button.transform.DOMoveY(button.transform.position.y - 0.065f, 0.1f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);
        StartCoroutine(SpawnCoroutine());
        button.GetComponent<Renderer>().material.color = Color.gray;
    }
    
    IEnumerator SpawnCoroutine()
    {
        for (int i = 0; i < 9; i++)
        {
            Instantiate(gemPrefab, spawnPoint.position, spawnPoint.rotation);
            yield return new WaitForSeconds(0.2f);
        }
        GemManager.instance.MakeFake();
        Destroy(this);
    }

    public void TriggerAlarm()
    {
        audioSource.clip = alarmSound; audioSource.Play();
        GameManager.Instance.TimePenalty(300f);
    }
}
