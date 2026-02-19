using UnityEngine;

public class PuzzleSheet : GrabbableObject
{
    private LightTableSurface _snappedTable;

    protected override void Drop(Interactor interactor)
    {
        base.Drop(interactor);
        SnapToTable();
    }

    private void SnapToTable()
    {
        _snappedTable = null;

        var rb = GetComponent<Rigidbody>();
        var col = GetComponent<Collider>();

        // Temporarily disable our own collider so the raycast doesn't hit ourselves
        bool colWasEnabled = col != null && col.enabled;
        if (col != null) col.enabled = false;

        // Raycast down from above current position
        bool didHit = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 5.0f);

        // Re-enable collider
        if (col != null) col.enabled = colWasEnabled;

        if (!didHit)
            return;

        // If we hit another PuzzleSheet, align on top of it
        if (hit.collider.TryGetComponent(out PuzzleSheet otherSheet) && otherSheet != this)
        {
            transform.position = new Vector3(
                otherSheet.transform.position.x,
                hit.point.y + 0.005f,
                otherSheet.transform.position.z);
            transform.rotation = otherSheet.transform.rotation;
            FreezeOnSurface(rb);
            return;
        }

        // If we hit a LightTable, snap to its center
        if (hit.collider.TryGetComponent(out LightTableSurface table))
        {
            _snappedTable = table;

            if (table.TryGetSurfacePose(out Vector3 snapPos, out Quaternion snapRot))
            {
                transform.position = snapPos + Vector3.up * 0.005f;
                transform.rotation = snapRot;
            }
            else
            {
                transform.position = new Vector3(hit.point.x, hit.point.y + 0.005f, hit.point.z);
                transform.rotation = Quaternion.identity;
            }
            FreezeOnSurface(rb);
            return;
        }

        // If we hit any other surface (floor, etc.) just leave it where physics puts it
    }

    private void FreezeOnSurface(Rigidbody rb)
    {
        if (rb == null) return;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
    }
}
