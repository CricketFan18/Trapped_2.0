using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float WalkSpeed = 5f;
    public float SprintSpeed = 8f;
    public float Gravity = -9.81f;
    
    [Header("Look")]
    public Transform CameraTransform;
    public float MouseSensitivity = 2f;
    public float LookXLimit = 85f;
    
    private CharacterController _characterController;
    private Vector3 _moveDirection = Vector3.zero;
    private float _rotationX = 0;

    // --- Focus Mode Support ---
    public bool IsFocused { get; private set; } = false;
    private Vector3 _savedCamLocalPos;
    private Quaternion _savedCamLocalRot;
    private Vector3 _savedBodyPos;
    private Quaternion _savedBodyRot;
    private Transform _focusTarget;

    public void EnterFocusMode(Transform target)
    {
        if (IsFocused || target == null) return;

        IsFocused = true;
        _focusTarget = target;

        _savedCamLocalPos = CameraTransform.localPosition;
        _savedCamLocalRot = CameraTransform.localRotation;
        _savedBodyPos = transform.position;
        _savedBodyRot = transform.rotation;

        _characterController.enabled = false;

        Debug.Log($"[FPS] Entered Focus Mode -> {target.name} at {target.position}");
    }

    public void ExitFocusMode()
    {
        if (!IsFocused) return;

        IsFocused = false;
        _focusTarget = null;

        CameraTransform.localPosition = _savedCamLocalPos;
        CameraTransform.localRotation = _savedCamLocalRot;
        transform.position = _savedBodyPos;
        transform.rotation = _savedBodyRot;

        _characterController.enabled = true;

        Debug.Log("[FPS] Exited Focus Mode");
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (IsFocused)
        {
            UpdateFocusMode();
            return;
        }
        
        // 1. Calculate Movement (Local Space)
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = (isSprinting ? SprintSpeed : WalkSpeed) * Input.GetAxis("Vertical");
        float curSpeedY = (isSprinting ? SprintSpeed : WalkSpeed) * Input.GetAxis("Horizontal");
        
        float movementDirectionY = _moveDirection.y;
        _moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // 2. Apply Gravity 
        if (!_characterController.isGrounded) { _moveDirection.y = movementDirectionY + (Gravity * Time.deltaTime); }

        // 3. Move Character
        _characterController.Move(_moveDirection * Time.deltaTime);

        // 4. Camera Rotation
        _rotationX += -Input.GetAxis("Mouse Y") * MouseSensitivity;
        _rotationX = Mathf.Clamp(_rotationX, -LookXLimit, LookXLimit);
        CameraTransform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * MouseSensitivity, 0);
    }

    private void UpdateFocusMode()
    {
        if (_focusTarget == null) return;

        float speed = 8f;
        CameraTransform.position = Vector3.Lerp(CameraTransform.position, _focusTarget.position, Time.deltaTime * speed);
        CameraTransform.rotation = Quaternion.Slerp(CameraTransform.rotation, _focusTarget.rotation, Time.deltaTime * speed);
    }
}