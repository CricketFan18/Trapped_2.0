using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Transform InteractionSource; // The Camera
    public float InteractionRange = 3f;
    public float InteractionRadius = 0.2f;
    public LayerMask InteractableLayer; // Layer 6
    public Transform HoldPoint; // Where objects are held

    private IInteractable _currentTarget;
    public IInteractable CurrentHeldObject { get; private set; }

    // Track current table for focus mode exit
    private LightTableSurface _activeTable;
    private FirstPersonController _fps;

    private void Awake()
    {
        _fps = GetComponent<FirstPersonController>()
            ?? GetComponentInParent<FirstPersonController>()
            ?? GetComponentInChildren<FirstPersonController>();
    }

    private void Update()
    {
        bool isFocused = _fps != null && _fps.IsFocused;

        // --- FOCUS MODE (Table Mode) ---
        if (isFocused)
        {
            HandleFocusModeInput();
            return;
        }

        // --- HOLDING AN OBJECT (Not Focused) ---
        if (CurrentHeldObject != null)
        {
            HandleHeldObjectInput();
            return;
        }

        // --- NORMAL INTERACTION (Not Holding, Not Focused) ---
        HandleNormalInteraction();
    }

    private void HandleFocusModeInput()
    {
        // While in focus mode, E drops held object or exits table
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (CurrentHeldObject != null)
            {
                // Drop the held object onto the table
                CurrentHeldObject.Interact(this);
            }
            else if (_activeTable != null)
            {
                // Exit focus mode
                _activeTable.ExitTableMode();
                _activeTable = null;
            }
        }

        // Only handle click interactions if the table isn't actively dragging something
        bool tableIsDragging = _activeTable != null && _activeTable.GetDragTarget() != null;

        if (Input.GetMouseButtonDown(0) && !tableIsDragging)
        {
            if (CurrentHeldObject != null)
            {
                // If holding something usable, use it (e.g. toggle flashlight)
                if (CurrentHeldObject is IUsable usable)
                {
                    usable.Use(this);
                }
            }
        }

        string prompt = CurrentHeldObject != null
            ? "Press E to Drop | Click to Use"
            : "Press E to Leave Table | Drag objects to move them";
        UIManager.Instance.SetInteractionPrompt(prompt);
    }

    private void TryClickPickup()
    {
        Camera cam = null;
        if (InteractionSource != null)
            cam = InteractionSource.GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        int mask = InteractableLayer.value != 0 ? InteractableLayer.value : Physics.DefaultRaycastLayers;

        if (Physics.Raycast(ray, out RaycastHit hit, 10f, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                // Don't re-interact with the table itself via click
                if (!(interactable is LightTableSurface))
                {
                    interactable.Interact(this);
                }
            }
        }
    }

    private void HandleHeldObjectInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CurrentHeldObject.Interact(this);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (CurrentHeldObject is IUsable usable)
            {
                usable.Use(this);
            }
        }

        // While holding, also allow interacting with the table to enter focus mode
        var source = InteractionSource != null ? InteractionSource : (Camera.main != null ? Camera.main.transform : transform);
        Ray r = new Ray(source.position, source.forward);
        LayerMask mask = InteractableLayer.value == 0 ? Physics.DefaultRaycastLayers : InteractableLayer;

        if (Physics.SphereCast(r, InteractionRadius, out RaycastHit hit, InteractionRange, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out LightTableSurface table))
            {
                UIManager.Instance.SetInteractionPrompt($"Press F to Use Table | Press E to Drop {GetHeldName()}");
                if (Input.GetKeyDown(KeyCode.F))
                {
                    _activeTable = table;
                    table.Interact(this);
                }
                return;
            }
        }

        UIManager.Instance.SetInteractionPrompt($"Press E to Drop {GetHeldName()} | Click to Use");
    }

    private void HandleNormalInteraction()
    {
        var source = InteractionSource != null ? InteractionSource : (Camera.main != null ? Camera.main.transform : transform);
        Ray r = new Ray(source.position, source.forward);
        LayerMask mask = InteractableLayer.value == 0 ? Physics.DefaultRaycastLayers : InteractableLayer;

        if (Physics.SphereCast(r, InteractionRadius, out RaycastHit hit, InteractionRange, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                _currentTarget = interactable;
                UIManager.Instance.SetInteractionPrompt(interactable.InteractionPrompt);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    // If it's a table, track it
                    if (interactable is LightTableSurface table)
                    {
                        _activeTable = table;
                    }
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

    private string GetHeldName()
    {
        if (CurrentHeldObject is GrabbableObject grab)
            return grab.ObjectName;
        return "Item";
    }

    public void SetHeldObject(IInteractable heldObject)
    {
        CurrentHeldObject = heldObject;
    }

    public void ClearHeldObject()
    {
        CurrentHeldObject = null;
    }

    public void SetActiveTable(LightTableSurface table)
    {
        _activeTable = table;
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