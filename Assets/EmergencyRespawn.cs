using UnityEngine;

public class EmergencyRespawn : MonoBehaviour
{
    [Header("Respawn Configuration")]
    [Tooltip("Drag the empty GameObject representing your safe spawn point here.")]
    public Transform safeSpawnLocation;

    // Cache the physics components to avoid heavy GetComponent calls in the Update loop
    private CharacterController _characterController;
    private Rigidbody _rigidbody;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Listen for Left Ctrl OR Right Ctrl, AND the R key pressed this exact frame
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.R))
        {
            ExecuteEmergencyRespawn();
        }
    }

    private void ExecuteEmergencyRespawn()
    {
        if (safeSpawnLocation == null)
        {
            Debug.LogError("Emergency Respawn failed: safeSpawnLocation is not assigned in the Inspector!");
            return;
        }

        // Scenario A: The player uses a CharacterController
        if (_characterController != null)
        {
            // CharacterControllers cache their position. We must disable it, move the transform, and re-enable it.
            _characterController.enabled = false;
            transform.position = safeSpawnLocation.position;
            transform.rotation = safeSpawnLocation.rotation;
            _characterController.enabled = true;
        }
        // Scenario B: The player uses Rigidbody Physics
        else if (_rigidbody != null)
        {
            transform.position = safeSpawnLocation.position;
            transform.rotation = safeSpawnLocation.rotation;

            // Kill all momentum so they don't go flying upon respawning
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
        // Scenario C: The player is just a standard Transform
        else
        {
            transform.position = safeSpawnLocation.position;
            transform.rotation = safeSpawnLocation.rotation;
        }

        Debug.Log("Emergency Respawn Triggered. Player moved to safe location.");
    }
}