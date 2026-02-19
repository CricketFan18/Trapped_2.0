using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GrabbableObject : MonoBehaviour, IInteractable
{
    [Header("Grabbable Settings")]
    public string ObjectName = "Item";
    [Tooltip("If true, requires Rigidbody for physics simulation on drop.")]
    public bool UsePhysics = true;
    [Tooltip("Rotation offset applied when the object is held in first person.")]
    public Vector3 HeldRotationOffset = Vector3.zero;
    [Tooltip("If true, the object points where the camera looks when held (for flashlights).")]
    public bool PointAtCamera = false;

    private Rigidbody _rb;
    private Collider _collider;
    private Transform _originalParent;
    private bool _isHeld = false;
    private Interactor _currentInteractor;

    public bool IsHeld => _isHeld;

    public string InteractionPrompt => _isHeld ? $"Press E to Drop {ObjectName}" : $"Press E to Grab {ObjectName}";

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _originalParent = transform.parent;
    }

    private void Update()
    {
        if (_isHeld && _currentInteractor != null)
        {
            HandleHeldUpdate();
        }
    }

    private void HandleHeldUpdate()
    {
        var fps = _currentInteractor.GetComponent<FirstPersonController>();
        bool isFocused = fps != null && fps.IsFocused;

        if (isFocused)
        {
            HandleTableModeHeld();
        }
        else if (PointAtCamera)
        {
            // In first person, orient the object to point where the camera looks
            Transform camT = _currentInteractor.InteractionSource;
            if (camT != null)
            {
                transform.rotation = camT.rotation * Quaternion.Euler(HeldRotationOffset);
            }
        }
    }

    private void HandleTableModeHeld()
    {
        Camera cam = null;
        if (_currentInteractor.InteractionSource != null)
            cam = _currentInteractor.InteractionSource.GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Temporarily disable our own collider so the raycast doesn't hit ourselves
        bool colWasEnabled = _collider != null && _collider.enabled;
        if (_collider != null) _collider.enabled = false;

        bool didHit = Physics.Raycast(ray, out RaycastHit hit, 10f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        if (_collider != null) _collider.enabled = colWasEnabled;

        if (!didHit) return;

        Vector3 targetPos = hit.point + hit.normal * 0.02f;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 15f);

        if (PointAtCamera)
        {
            // For flashlights: orient so the Light child (which faces local +Z via -90 X rotation)
            // ends up pointing toward the surface. We want transform.up to point into the surface.
            Quaternion targetRot = Quaternion.LookRotation(hit.normal, cam.transform.up) * Quaternion.Euler(90f, 0f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
        else
        {
            // For sheets: lay flat on the surface
            Quaternion flatRot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, flatRot, Time.deltaTime * 10f);
        }
    }

    public bool Interact(Interactor interactor)
    {
        if (_isHeld)
        {
            Drop(interactor);
        }
        else
        {
            Grab(interactor);
        }
        return true;
    }

    private void Grab(Interactor interactor)
    {
        if (interactor.CurrentHeldObject != null) return;

        _isHeld = true;
        _currentInteractor = interactor;
        interactor.SetHeldObject(this);

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        if (_collider != null)
            _collider.isTrigger = true;

        Transform targetParent = interactor.HoldPoint;
        Vector3 targetPos = Vector3.zero;

        if (targetParent == null)
        {
            targetParent = interactor.InteractionSource;
            targetPos = new Vector3(0.3f, -0.3f, 1.0f);
        }

        if (targetParent == null) targetParent = interactor.transform;

        transform.SetParent(targetParent);
        transform.localPosition = targetPos;
        transform.localRotation = Quaternion.Euler(HeldRotationOffset);
    }

    protected virtual void Drop(Interactor interactor)
    {
        _isHeld = false;
        _currentInteractor = null;
        interactor.ClearHeldObject();

        // Save world position/rotation before re-parenting so the object stays where it is
        Vector3 worldPos = transform.position;
        Quaternion worldRot = transform.rotation;

        transform.SetParent(_originalParent);

        // Restore world-space pose after re-parenting
        transform.position = worldPos;
        transform.rotation = worldRot;

        if (_collider != null)
            _collider.isTrigger = false;

        if (_rb != null && UsePhysics)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        OnDropped(interactor);
    }

    protected virtual void OnDropped(Interactor interactor) { }
}
