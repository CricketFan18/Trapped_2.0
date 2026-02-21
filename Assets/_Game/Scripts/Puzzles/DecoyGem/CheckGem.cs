using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class CheckGem : MonoBehaviour, IInteractable
{
    private bool foundReal = false;
    private string _interactionPrompt = "\"The counterfeit belongs within\"";
    public string InteractionPrompt => _interactionPrompt;
    public AudioClip closeSound;
    public AudioClip alarmSound;
    private AudioSource audioSource;
    public Transform topLid;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CloseBriefcase(() =>
        {
            Gem selectedGem = other.GetComponent<Gem>();
            if (selectedGem)
            {
                if (selectedGem.fake)
                {
                    Debug.Log("Succeeed");
                    _interactionPrompt = "E to pick suitcase";
                    GoldSpawner.instance.SpawnGoldbar();
                    foundReal = true;
                    GemManager.instance.RemoveAllGems();
                }
                else
                {
                    GemManager.instance.RemoveAllGems();
                    CloseBriefcase((() =>
                    {
                        GemManager.instance.spawnerButton.AddComponent<SpawnButton>().EnableButton();
                    }), false);
                    TriggerAlarm();
                }
            }
        });
    }

    private void CloseBriefcase(Action callback, bool closing = true)
    {
        audioSource.PlayOneShot(closeSound);
        topLid.DOLocalRotate(new Vector3(((closing) ? 80 : 0)
            , topLid.localRotation.y, topLid.localRotation.z), 1f, RotateMode.Fast).SetEase(closing? Ease.OutQuint: Ease.InQuint)
            .OnComplete(() =>
            {
                callback.Invoke();
            });
    }
    
    public void TriggerAlarm()
    {
        audioSource.clip = alarmSound; audioSource.Play();
        GameManager.Instance.TimePenalty(300f);
    }
    
    bool IInteractable.Interact(Interactor interactor)
    {
        if (!foundReal) return false;
        //Add to inventory
        return true;
    }
}
