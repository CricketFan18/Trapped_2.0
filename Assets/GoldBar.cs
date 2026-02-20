using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldBar : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "E to pickup";

    public bool Interact(Interactor interactor)
    {
        Destroy(gameObject);
        return true;
    }
}
