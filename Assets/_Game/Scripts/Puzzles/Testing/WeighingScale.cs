using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WeighingScale : MonoBehaviour, IInteractable
{
    
    private string interactionPrompt = "Press E to Weight";
    public string InteractionPrompt => interactionPrompt;
    public Transform rightScale;
    public Transform leftScale;
    private float balancedYpos;
    private int battery = 3;
    private float cooldown = 0;


    private void Start()
    {
        balancedYpos = leftScale.position.y;
    }

    bool IInteractable.Interact(Interactor interactor)
    {
        if (cooldown > 0) return false;
        
        GetComponent<AudioSource>().Play();
        transform.DOMoveY(transform.position.y - 0.065f, 0.1f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);
        
        int leftWeight = CalculateWeight(leftScale);
        int rightWeight = CalculateWeight(rightScale);
        if (leftWeight == rightWeight)
        {
            leftScale.DOMoveY(balancedYpos, 0.3f);
            rightScale.DOMoveY(balancedYpos, 0.3f);
        }
        else if (leftWeight > rightWeight) UpdateScale(leftScale, rightScale);
        else UpdateScale(rightScale, leftScale);

        if (--battery == 0)
        {
            interactionPrompt = "Wait " +  cooldown.ToString() + "s to continue";
            GetComponent<Renderer>().material.color = Color.gray;
            cooldown = 60;
            DOTween.To(() => cooldown,  x => cooldown = x, 0, 60f).OnComplete(() =>
            {
                battery = 2;
                interactionPrompt = "Press E to Weight";
                GetComponent<Renderer>().material.color = Color.red;
            });
        }
        return true;
    }

    void UpdateScale(Transform heavySide, Transform lightSide)
    {
        float moveAmount = 0.2f; 
        heavySide.DOMoveY(balancedYpos - moveAmount, 0.3f);
        lightSide.DOMoveY(balancedYpos + moveAmount, 0.3f);
    }
    
    int CalculateWeight(Transform t)
    {
        int weight = 0;
        Vector3 checkPos = t.position + Vector3.up * 0.3f;
        Collider[] hits = Physics.OverlapSphere(checkPos, 0.75f);
        foreach (Collider c in hits)
        {
            Gem g = c.GetComponent<Gem>();
            if (g != null)
            {
                weight += g.weight;
            }
        }
        Debug.Log(weight);
        return weight;
    }
}
