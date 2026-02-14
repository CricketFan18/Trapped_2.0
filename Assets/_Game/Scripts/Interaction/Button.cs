using UnityEngine;
using UnityEngine.Events;
public class Button : MonoBehaviour, IInteractable
{
    public string interactionPrompt;
    public ButtonEvent interactionEvent;
    public string InteractionPrompt => interactionPrompt;
    
    [System.Serializable]
    public class ButtonEvent : UnityEvent<Button> { }
    
    bool IInteractable.Interact(Interactor interactor)
    {
        interactionEvent.Invoke(this);
        return true;
    }

}