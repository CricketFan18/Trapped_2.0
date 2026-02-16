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

            if (_draggable != null && _draggable.IsDragging)
            {
                // While being dragged on the table, use the object's position
                // and point straight down so the cone covers the sheets below
                pos = transform.position;
                dir = Vector3.down;
            }
            else
            {
                pos = FlashlightInfo.transform.position;
                dir = FlashlightInfo.transform.forward;
            }

            Shader.SetGlobalVector(_UVLightPosID, pos);
            Shader.SetGlobalVector(_UVLightDirID, dir);

            float cosAngle = Mathf.Cos((SpotAngle * 0.5f) * Mathf.Deg2Rad);
            Shader.SetGlobalFloat(_UVLightAngleID, cosAngle);
            Shader.SetGlobalFloat(_UVLightEnabledID, 1f);
        }
        else
        {
            Shader.SetGlobalFloat(_UVLightEnabledID, 0f);
        }
    }
}
