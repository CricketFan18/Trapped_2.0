using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Transform InteractionSource; // The Camera
    public float InteractionRange = 3f;
    public LayerMask InteractableLayer; // Layer 6
    public Transform HoldPoint; // Where objects are held

    private IInteractable _currentTarget;
    public IInteractable CurrentHeldObject { get; private set; }

    private void Update()
    {
        if (GameManager.Instance.IsGamePaused) return;

        // If holding an object, handle drop logic or secondary interact
        if (CurrentHeldObject != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Trigger interact on the held object to potentially drop or use it
                CurrentHeldObject.Interact(this);
            }
            // While holding, we might want to disable raycasting for new interactions
            // Or raycast past the object. For now, let's keep it simple: No new interactions while holding.
            UIManager.Instance.SetInteractionPrompt("Press E to Drop/Use");
            return;
        }

        // 1. Raycast forward
        Ray r = new Ray(InteractionSource.position, InteractionSource.forward);

        if (Physics.Raycast(r, out RaycastHit hit, InteractionRange, InteractableLayer))
        {
            // 2. Check if object has IInteractable
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                _currentTarget = interactable;

                // 3. Update UI
                UIManager.Instance.SetInteractionPrompt(interactable.InteractionPrompt);

                // 4. Listen for Input
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact(this);
                }
            }
            else
            {
                ClearInteraction();
            }
        }
        else
        {
            ClearInteraction();
        }
    }

    public void SetHeldObject(IInteractable heldObject)
    {
        CurrentHeldObject = heldObject;
    }

    public void ClearHeldObject()
    {
        CurrentHeldObject = null;
    }

    private void ClearInteraction()
    {
        if (_currentTarget != null)
        {
            _currentTarget = null;
            UIManager.Instance.SetInteractionPrompt("");
        }
    }
}