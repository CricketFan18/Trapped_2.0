using UnityEngine;

public class XORPuzzle : PuzzleBase
{
    [Header("Puzzle Components")]
    public GameObject LayerA;
    public GameObject LayerB;

    [Header("Solution Data")]
    public string SolutionCode = "1234";
    [TextArea] public string[] Clues;

    [Header("Overlap Detection")]
    [Tooltip("Max XZ distance between sheet centers to count as overlapping.")]
    public float OverlapThreshold = 0.25f;

    private static readonly int _SheetsOverlappingID = Shader.PropertyToID("_SheetsOverlapping");
    private bool _sheetsOverlapping = false;
    private float _debugTimer;

    /// <summary>True when LayerA and LayerB are stacked on top of each other.</summary>
    public bool SheetsOverlapping => _sheetsOverlapping;

    private void Start()
    {
        // Auto-discover sheets by child name if references are not set
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

        if (LayerA == null) Debug.LogError("[XORPuzzle] LayerA (Message_Sheet) is null!");
        if (LayerB == null) Debug.LogError("[XORPuzzle] LayerB (Filter_Sheet) is null!");

        EnsureSheet(LayerA, "Message Sheet");
        EnsureSheet(LayerB, "Filter Sheet");
        EnsureTable();
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
            float globalOverlap = Shader.GetGlobalFloat(_SheetsOverlappingID);
            if (LayerA != null && LayerB != null)
            {
                Vector3 posA = LayerA.transform.position;
                Vector3 posB = LayerB.transform.position;
                float xzDist = Vector2.Distance(new Vector2(posA.x, posA.z), new Vector2(posB.x, posB.z));
                float yDist = Mathf.Abs(posA.y - posB.y);
                Debug.Log($"[XORPuzzle] A={posA}, B={posB}, xzDist={xzDist:F3}, yDist={yDist:F3}, overlap={_sheetsOverlapping}, global={globalOverlap:F1}");
            }
        }
    }

    private void UpdateOverlapState()
    {
        if (LayerA == null || LayerB == null)
        {
            SetOverlap(false);
            return;
        }

        // Use renderer bounds for a more reliable overlap check.
        // This handles sheets of any size/orientation — if their XZ footprints
        // intersect and they are close in Y, they count as overlapping.
        var rendA = LayerA.GetComponent<Renderer>();
        var rendB = LayerB.GetComponent<Renderer>();

        if (rendA != null && rendB != null)
        {
            Bounds bA = rendA.bounds;
            Bounds bB = rendB.bounds;

            // Check XZ overlap: project bounds onto the XZ plane
            bool xOverlap = bA.min.x <= bB.max.x && bA.max.x >= bB.min.x;
            bool zOverlap = bA.min.z <= bB.max.z && bA.max.z >= bB.min.z;
            float yDist = Mathf.Abs(bA.center.y - bB.center.y);

            SetOverlap(xOverlap && zOverlap && yDist < 0.1f);
        }
        else
        {
            // Fallback to center-distance check
            Vector3 posA = LayerA.transform.position;
            Vector3 posB = LayerB.transform.position;

            float xzDist = Vector2.Distance(
                new Vector2(posA.x, posA.z),
                new Vector2(posB.x, posB.z));
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

        // If it has GrabbableObject but NOT PuzzleSheet, swap it
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

        // Make sure it has PuzzleSheet at minimum
        if (obj.GetComponent<PuzzleSheet>() == null && obj.GetComponent<GrabbableObject>() == null)
        {
            var sheet = obj.AddComponent<PuzzleSheet>();
            sheet.ObjectName = sheetName;
            sheet.UsePhysics = true;
        }

        // Ensure it can be dragged on the table
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

    private void EnsureUVFlashlightDraggable()
    {
        // Find any UVLight in or under this puzzle and ensure it has TableDraggable
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
        // The XORReveal shader on LayerB needs _EncryptedTex set to LayerA's texture
        // so it can compute the XOR decode in the fragment shader.
        if (LayerA == null || LayerB == null)
        {
            Debug.LogWarning("[XORPuzzle] LayerA or LayerB is null — cannot wire textures.");
            return;
        }

        var rendA = LayerA.GetComponent<Renderer>();
        var rendB = LayerB.GetComponent<Renderer>();
        if (rendA == null || rendB == null)
        {
            Debug.LogWarning("[XORPuzzle] LayerA or LayerB has no Renderer.");
            return;
        }

        // Get the encrypted texture from LayerA
        Texture encTex = null;
        if (rendA.sharedMaterial != null)
            encTex = rendA.sharedMaterial.mainTexture;
        if (encTex == null && rendA.material != null)
            encTex = rendA.material.mainTexture;

        // Get the filter material instance for LayerB
        Material filterMat = rendB.material;

        // Verify the filter material uses the XORReveal shader
        if (filterMat.shader.name != "Custom/XORReveal")
        {
            Debug.LogWarning($"[XORPuzzle] Filter sheet shader is '{filterMat.shader.name}', expected 'Custom/XORReveal'. Attempting to assign correct shader.");
            Shader xorShader = Shader.Find("Custom/XORReveal");
            if (xorShader != null)
            {
                // Preserve textures before swapping shader
                Texture existingKey = filterMat.HasProperty("_MainTex") ? filterMat.GetTexture("_MainTex") : null;
                filterMat.shader = xorShader;
                filterMat.renderQueue = 3100;
                if (existingKey != null)
                    filterMat.SetTexture("_MainTex", existingKey);
            }
            else
            {
                Debug.LogError("[XORPuzzle] Cannot find 'Custom/XORReveal' shader!");
                return;
            }
        }

        if (encTex != null && filterMat.HasProperty("_EncryptedTex"))
        {
            filterMat.SetTexture("_EncryptedTex", encTex);
            Debug.Log($"[XORPuzzle] Wired _EncryptedTex ({encTex.name}) onto filter sheet.");
        }
        else
        {
            Debug.LogWarning($"[XORPuzzle] Could not wire encrypted texture. encTex={encTex}, HasProperty={filterMat.HasProperty("_EncryptedTex")}");
        }
    }
}
