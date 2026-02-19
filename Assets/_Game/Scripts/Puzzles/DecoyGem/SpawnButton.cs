using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
public class SpawnButton : MonoBehaviour, IInteractable
{
    public string interactionPrompt;
    public Transform gemPrefab;
    public Transform spawnPoint;
    public string InteractionPrompt => "Press E to Spawn";


    public bool Interact(Interactor interactor)
    {
        GetComponent<AudioSource>().Play();
        transform.DOMoveY(transform.position.y - 0.065f, 0.1f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);
        StartCoroutine(SpawnCoroutine());
        GetComponent<Renderer>().material.color = Color.gray; 
        return true;
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

}