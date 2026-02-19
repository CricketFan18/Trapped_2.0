using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionDistance = 3.5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private GameObject interactPrompt; // "Press E to Interact" text

    private Camera playerCam;
    private bool isPuzzleActive = false;

    private void Start()
    {
        playerCam = Camera.main;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (isPuzzleActive) return;

        // Draw a visible line in the Scene view for debugging
        Debug.DrawRay(playerCam.transform.position, playerCam.transform.forward * interactionDistance, Color.red);

        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Check if we hit ANY collider
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Log what we hit
            // Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

            if (hit.collider.gameObject == gameObject)
            {
                if (interactPrompt != null) interactPrompt.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("E Pressed! Opening Puzzle..."); // Check for this log
                    OpenPuzzleInterface();
                }
            }
        }
        else
        {
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

    private void OpenPuzzleInterface()
    {
        isPuzzleActive = true;
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // Call the Puzzle Controller
        Puzzle_MSTNetwork.Instance.OpenPuzzle(this);
    }

    // Called by the Puzzle Controller when closed/solved
    public void OnPuzzleClosed()
    {
        isPuzzleActive = false;
    }
}
