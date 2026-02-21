using UnityEngine;

public class LightTableSurface : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    public string InteractionText = "Use Light Table";
    public Transform CameraFocusPoint;

    [Header("Camera Settings")]
    public float CameraHeight = 1.2f;
    public float CameraLookAngle = 75f;

    private bool _inUse = false;
    private Interactor _currentUser;
    private TableDraggable _dragTarget;
    private Camera _focusCamera;

    public string InteractionPrompt => _inUse ? "Press E to Leave Table" : InteractionText;

    private void Start()
    {
        EnsureCollider();
        EnsureInteractableLayer();

        if (CameraFocusPoint == null)
        {
            var child = transform.Find("CameraFocusPoint");
            if (child != null)
            {
                CameraFocusPoint = child;
            }
            else
            {
                CreateCameraPoint();
            }
        }
    }

    private void CreateCameraPoint()
    {
        GameObject pivot = new GameObject("CameraFocusPoint");

        Vector3 surfaceCenter = GetSurfaceCenter();
        Vector3 camPos = surfaceCenter + Vector3.up * CameraHeight;

        // Set world position/rotation BEFORE parenting to avoid non-uniform scale distortion
        pivot.transform.position = camPos;
        pivot.transform.rotation = Quaternion.Euler(CameraLookAngle, transform.eulerAngles.y, 0f);

        // Parent with worldPositionStays=true to keep the world-space values intact
        pivot.transform.SetParent(transform, true);

        CameraFocusPoint = pivot.transform;

        Debug.Log($"[LightTable] Created CameraFocusPoint at {camPos}, rotation {pivot.transform.eulerAngles}");
    }

    private Vector3 GetSurfaceCenter()
    {
        if (TryGetSurfacePose(out Vector3 pos, out Quaternion _))
            return pos;
        return transform.position + Vector3.up * 0.05f;
    }

    public bool Interact(Interactor interactor)
    {
        if (!_inUse)
        {
            EnterTableMode(interactor);
        }
        else if (_currentUser == interactor)
        {
            ExitTableMode();
        }
        return true;
    }

    private void EnterTableMode(Interactor interactor)
    {
        _currentUser = interactor;
        var fps = interactor.GetComponent<FirstPersonController>()
               ?? interactor.GetComponentInParent<FirstPersonController>()
               ?? interactor.GetComponentInChildren<FirstPersonController>();
        if (fps == null) return;

        _inUse = true;
        fps.EnterFocusMode(CameraFocusPoint);

        // Cache camera for drag raycasting
        _focusCamera = interactor.InteractionSource != null
            ? interactor.InteractionSource.GetComponent<Camera>()
            : null;
        if (_focusCamera == null) _focusCamera = Camera.main;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"[LightTable] Entered Table Mode, cam target: {CameraFocusPoint.position}");
    }

    public void ExitTableMode()
    {
        if (_currentUser != null)
        {
            var fps = _currentUser.GetComponent<FirstPersonController>()
                   ?? _currentUser.GetComponentInParent<FirstPersonController>()
                   ?? _currentUser.GetComponentInChildren<FirstPersonController>();
            if (fps != null)
            {
                fps.ExitFocusMode();
                if (!GameManager.Instance.IsInPuzzleMode)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            _currentUser = null;
        }
        if (_dragTarget != null)
        {
            _dragTarget.EndDrag();
            _dragTarget = null;
        }
        _focusCamera = null;
        _inUse = false;
    }

    private void Update()
    {
        if (_inUse && _currentUser == null)
        {
            ExitTableMode();
        }

        if (_inUse)
        {
            HandleDragInput();
        }

        if (_inUse && Input.GetKeyDown(KeyCode.Escape))
        {
            // Clear interactor's active table reference before exiting
            if (_currentUser != null)
            {
                var interactor = _currentUser.GetComponent<Interactor>();
                if (interactor != null)
                    interactor.SetActiveTable(null);
            }
            ExitTableMode();
        }
    }

    private void HandleDragInput()
    {
        if (_focusCamera == null) return;

        // Get the surface Y for dragging
        float surfaceY = transform.position.y;
        if (TryGetSurfacePose(out Vector3 surfPos, out Quaternion _))
            surfaceY = surfPos.y;

        // Mouse down — start drag
        if (Input.GetMouseButtonDown(0) && _dragTarget == null)
        {
            Ray ray = _focusCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                // Don't drag the table itself
                if (hit.collider.gameObject == gameObject) return;

                // Check the hit object and its parents for a TableDraggable
                var draggable = hit.collider.GetComponent<TableDraggable>()
                             ?? hit.collider.GetComponentInParent<TableDraggable>();
                if (draggable != null)
                {
                    _dragTarget = draggable;
                    _dragTarget.BeginDrag(_focusCamera, hit, surfaceY);
                }
            }
        }

        // Mouse held — update drag
        if (Input.GetMouseButton(0) && _dragTarget != null)
        {
            _dragTarget.UpdateDrag();
        }

        // Mouse up — end drag
        if (Input.GetMouseButtonUp(0) && _dragTarget != null)
        {
            _dragTarget.EndDrag();
            _dragTarget = null;
        }
    }

    public TableDraggable GetDragTarget()
    {
        return _dragTarget;
    }

    public bool IsInUse() => _inUse;
    public Interactor GetCurrentUser() => _currentUser;

    private void EnsureInteractableLayer()
    {
        if (gameObject.layer != 6)
            gameObject.layer = 6;
    }

    private void EnsureCollider()
    {
        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();
    }

    public bool TryGetSurfacePose(out Vector3 position, out Quaternion rotation)
    {
        rotation = transform.rotation;

        var box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Vector3 localTop = box.center + new Vector3(0, box.size.y * 0.5f, 0);
            position = transform.TransformPoint(localTop) + transform.up * 0.002f;
            return true;
        }

        var r = GetComponent<Renderer>();
        if (r != null)
        {
            Bounds b = r.bounds;
            position = new Vector3(b.center.x, b.max.y + 0.002f, b.center.z);
            return true;
        }

        position = transform.position;
        return false;
    }
}
