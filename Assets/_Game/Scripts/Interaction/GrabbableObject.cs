using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GrabbableObject : MonoBehaviour, IInteractable
{
    [Header("Grabbable Settings")]
    public string ObjectName = "Item";
    [Tooltip("If true, requires Rigidbody for physics simulation on drop.")]
    public bool UsePhysics = true;
    [Tooltip("Rotation offset applied when the object is held.")]
    public Vector3 HeldRotationOffset = Vector3.zero;
    
    private Rigidbody _rb;
    private Collider _collider;
    private Transform _originalParent;
    private bool _isHeld = false;

    public string InteractionPrompt => _isHeld ? $"Press E to Drop {ObjectName}" : $"Press E to Grab {ObjectName}";

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _originalParent = transform.parent;
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
        if (interactor.CurrentHeldObject != null) return; // Already holding something

        _isHeld = true;
        interactor.SetHeldObject(this);

        // Physics Setup
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        // Collider Setup (Trigger to avoid pushing player)
        if (_collider != null)
        {
            _collider.isTrigger = true;
        }

        // Parenting Logic
        // Prioritize HoldPoint, then Camera (InteractionSource) with offset, then Body
        Transform targetParent = interactor.HoldPoint;
        Vector3 targetPos = Vector3.zero;

        if (targetParent == null)
        {
            // Fallback to camera if HoldPoint is missing
            targetParent = interactor.InteractionSource;
            // Add decent offset so it's not in the camera
            targetPos = new Vector3(0.3f, -0.3f, 1.0f); 
        }
        
        if (targetParent == null) targetParent = interactor.transform;

        transform.SetParent(targetParent);
        transform.localPosition = targetPos;
        transform.localRotation = Quaternion.Euler(HeldRotationOffset);
    }

    private void Drop(Interactor interactor)
    {
        _isHeld = false;
        interactor.ClearHeldObject();

        // Parenting
        transform.SetParent(_originalParent); // Or null

        // Reset rotation to be flat (horizontal) to ensure it lands nicely on tables
        // and doesn't penetrate geometry with its vertical corners
        Vector3 currentEuler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0, currentEuler.y, 0);

        // Physics Setup
        if (_rb != null && UsePhysics)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.velocity = Vector3.zero; // Important: Clear hand-movement velocity
            _rb.angularVelocity = Vector3.zero;

            // Limit Throw Force to prevent driving objects into tables (Tunneling)
            Vector3 throwDir = interactor.transform.forward;
            if (throwDir.y < 0) throwDir.y = 0; // Flatten throw if looking down
            throwDir.Normalize();
            
            // Add slight forward force (Reduced magnitude to prevent shooting off tables)
            _rb.AddForce(throwDir * 1f, ForceMode.Impulse);
        }

        // Collider Setup
        if (_collider != null)
        {
            _collider.isTrigger = false;
        }
    }
}
