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
    private Renderer _renderer;
    private Color _originalColor = Color.white;
    private float _flashTimer = -1f;
    private Color _flashColor;
    private float _flashDuration = 0.5f;
    private bool _startPoseSaved = false;

    private void Start()
    {
        // Only save if not already set by FixCardParenting / SaveStartPose
        if (!_startPoseSaved)
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _startPoseSaved = true;
        }

        _renderer = GetComponent<Renderer>();
        if (_renderer != null && _renderer.material != null)
        {
            _originalColor = _renderer.material.color;
        }
    }

    /// <summary>
    /// Save the current world pose as the reset target.
    /// Called after reparenting to ensure ResetPosition returns to the correct spot.
    /// </summary>
    public void SaveStartPose()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        _startPoseSaved = true;
    }

    private void Update()
    {
        if (_flashTimer >= 0f && _renderer != null)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f)
            {
                _renderer.material.color = _originalColor;
                _flashTimer = -1f;
            }
            else
            {
                float t = _flashTimer / _flashDuration;
                _renderer.material.color = Color.Lerp(_originalColor, _flashColor, t);
            }
        }
    }

    public void ResetPosition()
    {
        transform.position = _startPosition;
        transform.rotation = _startRotation;

        if (_renderer != null)
            _renderer.material.color = _originalColor;
        _flashTimer = -1f;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Flash the card a color briefly to give visual feedback.
    /// </summary>
    public void Flash(Color color, float duration = 0.5f)
    {
        _flashColor = color;
        _flashDuration = duration;
        _flashTimer = duration;

        if (_renderer != null)
            _renderer.material.color = color;
    }

    /// <summary>
    /// Set a persistent highlight color (e.g. while being dragged).
    /// </summary>
    public void SetHighlight(bool highlighted)
    {
        if (_renderer == null) return;

        if (highlighted)
        {
            _renderer.material.color = new Color(
                _originalColor.r + 0.15f,
                _originalColor.g + 0.15f,
                _originalColor.b + 0.05f,
                _originalColor.a);
        }
        else if (_flashTimer < 0f)
        {
            _renderer.material.color = _originalColor;
        }
    }

    /// <summary>
    /// Lock the card in the correct-answer color permanently.
    /// </summary>
    public void SetCorrect()
    {
        if (_renderer != null)
            _renderer.material.color = new Color(0.4f, 1f, 0.4f, 1f);
        _flashTimer = -1f;
    }
}
