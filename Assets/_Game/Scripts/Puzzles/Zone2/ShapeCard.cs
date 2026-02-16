using UnityEngine;

/// <summary>
/// A draggable card on the shape-matching table that knows its shape identity.
/// </summary>
public class ShapeCard : MonoBehaviour
{
    public enum Shape { Cross, Circle, Triangle }

    public Shape CardShape;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    public void ResetPosition()
    {
        transform.position = _startPosition;
        transform.rotation = _startRotation;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
