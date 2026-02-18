using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGameObject : MonoBehaviour, IInteractable
{
    public string InteractionPrompt =>  "Press E to interact";
    public bool Interact(Interactor interactor)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CarsManager.instance.UICanvas.SetActive(true);
        return true;
    }
}
