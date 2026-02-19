using UnityEngine;

public class XORPuzzle : MonoBehaviour // <-- Changed from PuzzleBase
{
    [Header("System integration")]
    public string PuzzleID = "Zone1_XOR";
    public int ScoreAward = 100;
    public bool IsSolved { get; private set; } = false;

    [Header("Puzzle Components")]
    public GameObject LayerA;
    public GameObject LayerB;

    [Header("Solution Data")]
    public string SolutionCode = "1234";
    [TextArea] public string[] Clues;

    [Header("Overlap Detection")]
    public float OverlapThreshold = 0.25f;

    private static readonly int _SheetsOverlappingID = Shader.PropertyToID("_SheetsOverlapping");
    private bool _sheetsOverlapping = false;
    private float _debugTimer;

    public bool SheetsOverlapping => _sheetsOverlapping;

    private void Start()
    {
        if (LayerA == null)
        {
            var t = transform.Find("Message_Sheet");
            if (t != null) LayerA = t.gameObject;
        }
        if (LayerB == null)
        {
            var t = transform.Find("Filter_Sheet");
            if (t != null) LayerB = t.gameObject;
        }

        EnsureSheet(LayerA, "Message Sheet");
        EnsureSheet(LayerB, "Filter Sheet");
        EnsureTable();

        if (GetComponentInChildren<UVLight>() == null)
            SpawnUVFlashlight(transform);

        EnsureUVFlashlightDraggable();
        WireEncryptedTexture();
        Shader.SetGlobalFloat(_SheetsOverlappingID, 0f);
    }

    private void Update()
    {
        UpdateOverlapState();

        _debugTimer += Time.deltaTime;
        if (_debugTimer >= 3f)
        {
            _debugTimer = 0f;
            // Diagnostic logging removed for brevity, but you can keep it if needed.
        }
    }

    private void UpdateOverlapState()
    {
        if (LayerA == null || LayerB == null)
        {
            SetOverlap(false);
            return;
        }

        var rendA = LayerA.GetComponent<Renderer>();
        var rendB = LayerB.GetComponent<Renderer>();

        if (rendA != null && rendB != null)
        {
            Bounds bA = rendA.bounds;
            Bounds bB = rendB.bounds;
            bool xOverlap = bA.min.x <= bB.max.x && bA.max.x >= bB.min.x;
            bool zOverlap = bA.min.z <= bB.max.z && bA.max.z >= bB.min.z;
            float yDist = Mathf.Abs(bA.center.y - bB.center.y);
            SetOverlap(xOverlap && zOverlap && yDist < 0.1f);
        }
        else
        {
            Vector3 posA = LayerA.transform.position;
            Vector3 posB = LayerB.transform.position;
            float xzDist = Vector2.Distance(new Vector2(posA.x, posA.z), new Vector2(posB.x, posB.z));
            float yDist = Mathf.Abs(posA.y - posB.y);
            SetOverlap(xzDist < OverlapThreshold && yDist < 0.1f);
        }
    }

    private void SetOverlap(bool overlapping)
    {
        if (_sheetsOverlapping != overlapping)
        {
            Debug.Log($"[XORPuzzle] Sheets overlapping changed: {overlapping}");
        }
        _sheetsOverlapping = overlapping;
        Shader.SetGlobalFloat(_SheetsOverlappingID, overlapping ? 1f : 0f);
    }

    private void OnDisable()
    {
        Shader.SetGlobalFloat(_SheetsOverlappingID, 0f);
    }

    private void EnsureSheet(GameObject obj, string sheetName)
    {
        if (obj == null) return;

        var grab = obj.GetComponent<GrabbableObject>();
        if (grab != null && grab.GetType() == typeof(GrabbableObject))
        {
            bool usePhysics = grab.UsePhysics;
            Vector3 offset = grab.HeldRotationOffset;
            DestroyImmediate(grab);

            var sheet = obj.AddComponent<PuzzleSheet>();
            sheet.ObjectName = sheetName;
            sheet.UsePhysics = usePhysics;
            sheet.HeldRotationOffset = offset;
        }

        if (obj.GetComponent<PuzzleSheet>() == null && obj.GetComponent<GrabbableObject>() == null)
        {
            var sheet = obj.AddComponent<PuzzleSheet>();
            sheet.ObjectName = sheetName;
            sheet.UsePhysics = true;
        }

        if (obj.GetComponent<TableDraggable>() == null)
            obj.AddComponent<TableDraggable>();

        obj.layer = 6;
    }

    private void EnsureTable()
    {
        Transform t = transform.Find("LightTable");
        if (t == null) return;

        if (t.GetComponent<LightTableSurface>() == null)
            t.gameObject.AddComponent<LightTableSurface>();

        t.gameObject.layer = 6;
    }

    public void AttemptSolve(string code)
    {
        if (code == SolutionCode)
        {
            OnPuzzleSolved();
        }
        else
        {
            Debug.Log("Incorrect Code");
        }
    }

    // NEW LOGIC: Connect to GameManager instead of PuzzleBase
    public void OnPuzzleSolved()
    {
        if (IsSolved) return;
        IsSolved = true;

        GameManager.Instance.AddScore(ScoreAward, PuzzleID);
        Debug.Log($"[XORPuzzle] Puzzle Solved: {PuzzleID}");
    }

    private void EnsureUVFlashlightDraggable()
    {
        var uvLights = GetComponentsInChildren<UVLight>(true);
        foreach (var uv in uvLights)
        {
            if (uv.GetComponent<TableDraggable>() == null)
                uv.gameObject.AddComponent<TableDraggable>();
            uv.gameObject.layer = 6;
        }
    }

    private void WireEncryptedTexture()
    {
        if (LayerA == null || LayerB == null) return;
        var rendA = LayerA.GetComponent<Renderer>();
        var rendB = LayerB.GetComponent<Renderer>();
        if (rendA == null || rendB == null) return;

        Texture encTex = null;
        if (rendA.sharedMaterial != null) encTex = rendA.sharedMaterial.mainTexture;
        if (encTex == null && rendA.material != null) encTex = rendA.material.mainTexture;

        Material filterMat = rendB.material;
        if (filterMat.shader.name != "Custom/XORReveal")
        {
            Shader xorShader = Shader.Find("Custom/XORReveal");
            if (xorShader != null)
            {
                Texture existingKey = filterMat.HasProperty("_MainTex") ? filterMat.GetTexture("_MainTex") : null;
                filterMat.shader = xorShader;
                filterMat.renderQueue = 3100;
                if (existingKey != null) filterMat.SetTexture("_MainTex", existingKey);
            }
        }

        if (encTex != null && filterMat.HasProperty("_EncryptedTex"))
            filterMat.SetTexture("_EncryptedTex", encTex);
    }

    public void SpawnUVFlashlight(Transform puzzleRoot)
    {
        GameObject flashlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flashlight.name = "UV_Flashlight";
        flashlight.transform.SetParent(puzzleRoot.transform);
        flashlight.transform.localPosition = new Vector3(0.6f, 0.6f, 0);
        flashlight.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
        flashlight.transform.localRotation = Quaternion.Euler(90, 0, 0);
        flashlight.layer = 6;
        flashlight.GetComponent<Renderer>().material.color = Color.black;

        GameObject lightObj = new GameObject("LightSource");
        lightObj.transform.SetParent(flashlight.transform);
        lightObj.transform.localPosition = new Vector3(0, 1.0f, 0);
        lightObj.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Spot;
        l.color = new Color(0.5f, 0f, 1f);
        l.intensity = 5f;
        l.range = 5f;
        l.spotAngle = 60f;

        var uvScript = flashlight.AddComponent<UVLight>();
        uvScript.FlashlightInfo = l;

        var rbF = flashlight.AddComponent<Rigidbody>();
        rbF.collisionDetectionMode = CollisionDetectionMode.Continuous;

        var grabF = flashlight.AddComponent<GrabbableObject>();
        grabF.ObjectName = "UV Flashlight";
        grabF.PointAtCamera = true;

        flashlight.AddComponent<TableDraggable>();
    }
}