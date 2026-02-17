using UnityEngine;

public class UVLight : MonoBehaviour, IUsable
{
    public Light FlashlightInfo;
    public float Range = 10f;
    public float SpotAngle = 45f;
    public bool IsOn = true;

    private static readonly int _UVLightPosID = Shader.PropertyToID("_UVLightPosition");
    private static readonly int _UVLightDirID = Shader.PropertyToID("_UVLightDirection");
    private static readonly int _UVLightAngleID = Shader.PropertyToID("_UVLightAngle");
    private static readonly int _UVLightEnabledID = Shader.PropertyToID("_UVLightEnabled");

    private TableDraggable _draggable;
    private GrabbableObject _grabbable;
    private float _debugTimer;

    public string InteractionPrompt => IsOn ? "Click to Turn Off" : "Click to Turn On";

    public void Use(Interactor interactor)
    {
        ToggleLight();
    }

    public void ToggleLight()
    {
        if (FlashlightInfo != null)
        {
            FlashlightInfo.enabled = !FlashlightInfo.enabled;
            IsOn = FlashlightInfo.enabled;
        }
    }

    private void Start()
    {
        if (FlashlightInfo == null)
            FlashlightInfo = GetComponentInChildren<Light>();

        if (FlashlightInfo != null)
        {
            IsOn = FlashlightInfo.enabled;
            Range = FlashlightInfo.range;
            SpotAngle = FlashlightInfo.spotAngle;
        }

        _draggable = GetComponent<TableDraggable>();
        _grabbable = GetComponent<GrabbableObject>();
    }

    private void Update()
    {
        if (FlashlightInfo != null)
        {
            IsOn = FlashlightInfo.enabled;
        }

        if (IsOn && FlashlightInfo != null)
        {
            Vector3 pos;
            Vector3 dir;

            bool heldByPlayer = _grabbable != null && _grabbable.IsHeld;

            if (heldByPlayer)
            {
                // Held in hand: use the Light component's actual position/direction
                // so aiming follows the player's camera
                pos = FlashlightInfo.transform.position;
                dir = FlashlightInfo.transform.forward;
            }
            else
            {
                // On the table (dragging or resting): point straight down
                // so the cone covers the sheets below
                pos = transform.position;
                dir = Vector3.down;
            }

            Shader.SetGlobalVector(_UVLightPosID, pos);
            Shader.SetGlobalVector(_UVLightDirID, dir);

            float cosAngle;
            if (heldByPlayer)
            {
                // Held: use the actual spot angle for a focused beam
                cosAngle = Mathf.Cos((SpotAngle * 0.5f) * Mathf.Deg2Rad);
            }
            else
            {
                // On the table: use a very wide cone (160° full angle) so the
                // close-range light covers the sheets beneath it properly
                cosAngle = Mathf.Cos(80f * Mathf.Deg2Rad); // ~0.17
            }
            Shader.SetGlobalFloat(_UVLightAngleID, cosAngle);
            Shader.SetGlobalFloat(_UVLightEnabledID, 1f);

            _debugTimer += Time.deltaTime;
            if (_debugTimer >= 2f)
            {
                _debugTimer = 0f;
                Debug.Log($"[UVLight] pos={pos}, dir={dir}, cosAngle={cosAngle:F3}, held={heldByPlayer}");
            }
        }
        else
        {
            Shader.SetGlobalFloat(_UVLightEnabledID, 0f);
        }
    }
}
