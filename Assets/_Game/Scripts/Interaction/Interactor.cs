using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Transform InteractionSource; // The Camera
    public float InteractionRange = 3f;
    public LayerMask InteractableLayer; // Layer 6

    private IInteractable _currentTarget;

    private void Update()
    {
        if (GameManager.Instance.IsGamePaused) return;

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

    private void ClearInteraction()
    {
        if (_currentTarget != null)
        {
            _currentTarget = null;
            UIManager.Instance.SetInteractionPrompt("");
        }
    }
}