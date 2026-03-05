using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class CheckGem : MonoBehaviour
{
    private bool foundReal = false;
    public string _interactionPrompt = "\"The counterfeit belongs within\"";

    public AudioClip closeSound;
    public AudioClip alarmSound;
    private AudioSource audioSource;
    public Transform topLid;
    private int remainingTries = 3;

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
                    GameManager.Instance.AddScore(100, "DecoyGem");
                }
                else
                {
                    remainingTries--;
                    GemManager.instance.RemoveAllGems();
                    if (remainingTries <= 0)
                    {
                        _interactionPrompt = "Too many attempts";
                        return;
                    }

                    CloseBriefcase((() => { _interactionPrompt = $"{remainingTries} tries remaining"; }), false);
                    TriggerAlarm();
                }
            }
        });
    }

    private void CloseBriefcase(Action callback, bool closing = true)
    {
        audioSource.PlayOneShot(closeSound);
        topLid.DOLocalRotate(new Vector3(((closing) ? 80 : 0)
                , topLid.localRotation.y, topLid.localRotation.z), 1f, RotateMode.Fast)
            .SetEase(closing ? Ease.OutQuint : Ease.InQuint)
            .OnComplete(() => { callback.Invoke(); });
    }

    private void Update()
    {
        Debug.Log(remainingTries);
    }

    public void TriggerAlarm()
    {
        audioSource.clip = alarmSound;
        audioSource.Play();
    }
}
