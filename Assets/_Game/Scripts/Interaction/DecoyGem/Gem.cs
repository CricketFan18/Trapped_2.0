using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gem : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Press E to Pick-Up";
    public int weight = 5;
    public bool fake = false;
    public List<AudioClip> audioClips = new List<AudioClip>();
    private int pickupFrame;

    void Start()
    {
        GemManager.instance.gems.Add(this);
    }

    bool IInteractable.Interact(Interactor interactor)
    {
        if (GemManager.instance.holding) return false;
        
        pickupFrame = Time.frameCount;
        Debug.Log(interactor.name + " is holding");
        GetComponent<Rigidbody>().isKinematic = true;
        this.transform.SetParent(interactor.transform);
        transform.localPosition = new Vector3(0.5f, -0.3f, 1f);
        GemManager.instance.cam = interactor.transform;
        transform.localRotation = Quaternion.identity;
        GemManager.instance.heldGem = this;
        GemManager.instance.holding = true;
        return true;
    }

    private void OnCollisionEnter(Collision other)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClips[Random.Range(0, audioClips.Count)];
        audioSource.Play();
    }

    public void PlaceGem()
    {
        if (Time.frameCount == pickupFrame) return;

        Ray ray = new Ray(GemManager.instance.cam.transform.position, GemManager.instance.cam.transform.forward);
        RaycastHit hit; 
        
        if (Physics.Raycast(ray, out hit, 5f))
        {
            transform.SetParent(null);
            transform.position = hit.point;
            transform.GetComponent<Rigidbody>().isKinematic = false;
            GemManager.instance.holding = false;
            GemManager.instance.heldGem = null;
        }
    }
}
