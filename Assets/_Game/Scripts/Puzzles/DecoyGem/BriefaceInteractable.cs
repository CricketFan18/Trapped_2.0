using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BriefaceInteractable : MonoBehaviour, IInteractable
{
    public CheckGem check;
    public string InteractionPrompt => check._interactionPrompt;

    public bool Interact(Interactor interactor)
    {
        return false;
    }
}
