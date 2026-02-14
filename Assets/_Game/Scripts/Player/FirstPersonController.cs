using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float WalkSpeed = 5f;
    public float SprintSpeed = 8f;
    public float Gravity = -9.81f;
    public List<AudioClip> footstepsAudioClips = new List<AudioClip>();
    
    [Header("Look")]
    public Transform CameraTransform;
    public float MouseSensitivity = 2f;
    public float LookXLimit = 85f;
    
    private AudioSource audioSource;
    private CharacterController _characterController;
    private Vector3 _moveDirection = Vector3.zero;
    private float _rotationX = 0;
    public float stepTimer = 0.5f;
    private float _stepTimer;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        _stepTimer = stepTimer;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGamePaused) return;

        
        // 1. Calculate Movement (Local Space)
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = (isSprinting ? SprintSpeed : WalkSpeed) * Input.GetAxis("Vertical");
        float curSpeedY = (isSprinting ? SprintSpeed : WalkSpeed) * Input.GetAxis("Horizontal");

        if(_stepTimer > 0) _stepTimer -= Time.deltaTime;
        else if (_characterController.velocity.magnitude > 0.1f)
        {
            audioSource.clip = footstepsAudioClips[Random.Range(0, footstepsAudioClips.Count)];
            audioSource.Play();
            _stepTimer = stepTimer;
        }
        
        float movementDirectionY = _moveDirection.y;
        _moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // 2. Apply Gravity
        if (!_characterController.isGrounded)
        {
            _moveDirection.y = movementDirectionY + (Gravity * Time.deltaTime);
        }

        // 3. Move Character
        _characterController.Move(_moveDirection * Time.deltaTime);

        // 4. Camera Rotation
        _rotationX += -Input.GetAxis("Mouse Y") * MouseSensitivity;
        _rotationX = Mathf.Clamp(_rotationX, -LookXLimit, LookXLimit);
        CameraTransform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * MouseSensitivity, 0);
    }
}