using UnityEngine;

/// <summary>
/// Attach to any object that should be draggable by mouse on a LightTableSurface.
/// Works for PuzzleSheets and the UV flashlight (torch).
/// </summary>
public class TableDraggable : MonoBehaviour
{
    [Header("Drag Settings")]
    public float DragSpeed = 20f;
    public float SheetDragHeight = 0.03f;
    public float SheetDropHeight = 0.005f;

    [Header("UV Light Drag Settings")]
    [Tooltip("Height above the table surface to hold the UV light while dragging.")]
    public float LightDragHeight = 0.4f;
    [Tooltip("Height above the table surface to rest the UV light after dropping.")]
    public float LightDropHeight = 0.2f;

    [HideInInspector] public bool IsDragging = false;

    private Vector3 _dragOffset;
    private float _surfaceY;
    private Camera _cam;
    private Collider _collider;
    private Rigidbody _rb;
    private bool _wasKinematic;
    private UVLight _uvLight;
    private int _stackOrder;

    private float ActiveDragHeight => _uvLight != null ? LightDragHeight : SheetDragHeight;
    private float ActiveDropHeight => _uvLight != null ? LightDropHeight : SheetDropHeight;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
        _uvLight = GetComponent<UVLight>();
    }

    public void BeginDrag(Camera cam, RaycastHit hit, float surfaceY)
    {
        IsDragging = true;
        _cam = cam;
        _surfaceY = surfaceY;

        _dragOffset = transform.position - hit.point;
        _dragOffset.y = 0f;

        if (_rb != null)
        {
            _wasKinematic = _rb.isKinematic;
            _rb.isKinematic = true;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        Vector3 pos = transform.position;
        pos.y = _surfaceY + ActiveDragHeight;
        transform.position = pos;
    }

    /// <summary>
    /// Set the stacking order so overlapping sheets render at distinct heights.
    /// 0 = bottom sheet, 1 = sheet on top, etc.
    /// </summary>
    public void SetStackOrder(int order)
    {
        _stackOrder = order;
    }

    public void UpdateDrag()
    {
        if (!IsDragging || _cam == null) return;

        float dragY = _surfaceY + ActiveDragHeight;
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0, dragY, 0));
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            Vector3 targetPos = worldPoint + _dragOffset;
            targetPos.y = dragY;

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * DragSpeed);

            if (_uvLight != null)
            {
                // Orient the cylinder so its local Y points straight down.
                // The LightSource child is at local (0,1,0) with localRotation Euler(-90,0,0),
                // so when the cylinder's local Y aims downward the Light's forward = down.
                Quaternion targetRot = Quaternion.Euler(180f, transform.eulerAngles.y, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }
        }
    }

    public void EndDrag()
    {
        if (!IsDragging) return;
        IsDragging = false;

        // Settle onto the surface at the appropriate height
        float dropY = _surfaceY + ActiveDropHeight;

        // For sheets: detect stacking — if we land on top of another sheet, sit above it
        if (_uvLight == null)
        {
            dropY = CalculateSheetDropHeight();
        }

        Vector3 pos = transform.position;
        pos.y = dropY;
        transform.position = pos;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        _cam = null;
    }

    private float CalculateSheetDropHeight()
    {
        // Raycast downward from current XZ to find what's below
        bool colWasEnabled = _collider != null && _collider.enabled;
        if (_collider != null) _collider.enabled = false;

        float resultY = _surfaceY + SheetDropHeight;

        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            var otherDraggable = hit.collider.GetComponent<TableDraggable>();
            if (otherDraggable != null && otherDraggable != this && otherDraggable._uvLight == null)
            {
                // Stack on top of the other sheet
                resultY = hit.point.y + SheetDropHeight;
            }
        }

        if (_collider != null) _collider.enabled = colWasEnabled;
        return resultY;
    }
}
