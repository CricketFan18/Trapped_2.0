using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public Transform door1;
    public Transform door2;
    private AudioSource audioSource;
    BoxCollider[] doorColliders =  new BoxCollider[2];

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        doorColliders[0] = door1.GetComponent<BoxCollider>();
        doorColliders[1] = door2.GetComponent<BoxCollider>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        audioSource.Play();
        door1.DOLocalRotate(Vector3.zero, 0.3f, RotateMode.Fast).SetEase(Ease.InQuint);
        door2.DOLocalRotate(Vector3.zero, 0.3f, RotateMode.Fast).SetEase(Ease.InQuint).OnComplete(() =>
        {
            doorColliders[0].enabled = false;
            doorColliders[1].enabled = false;
            EscapeDoor escapeDoor = door1.transform.parent.parent.AddComponent<EscapeDoor>();
            escapeDoor.doorModel = door1.parent.gameObject;
            escapeDoor.GetComponent<BoxCollider>().enabled = true;
            Destroy(gameObject);
        });
    }
}
