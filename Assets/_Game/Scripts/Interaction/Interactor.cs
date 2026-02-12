using UnityEngine;
using TMPro;

public class Interactor : MonoBehaviour
{
    [Header("Settings")]
    public float InteractionRange = 3f;
    public LayerMask InteractionLayer;
    public Transform InteractorSource; // Assign MainCamera here

    [Header("UI")]
    public TextMeshProUGUI PromptText; // Assign a centered UI text

    void Update()
    {
        Ray r = new Ray(InteractorSource.position, InteractorSource.forward);
        if (Physics.Raycast(r, out RaycastHit hit, InteractionRange, InteractionLayer))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (PromptText) PromptText.text = interactable.InteractionPrompt;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.OnInteract();
                }
            }
        }
        else
        {
            if (PromptText) PromptText.text = "";
        }
    }
}