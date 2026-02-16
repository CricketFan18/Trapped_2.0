using UnityEngine;

public class XORPuzzle : PuzzleBase
{
    [Header("Puzzle Components")]
    public GameObject LayerA;
    public GameObject LayerB;

    [Header("Solution Data")]
    public string SolutionCode = "1234";
    [TextArea] public string[] Clues;

    private void Start()
    {
        EnsureSheet(LayerA, "Message Sheet");
        EnsureSheet(LayerB, "Filter Sheet");
        EnsureTable();
        EnsureUVFlashlightDraggable();
        WireEncryptedTexture();
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
        if (LayerA == null || LayerB == null) return;

        var rendA = LayerA.GetComponent<Renderer>();
        var rendB = LayerB.GetComponent<Renderer>();
        if (rendA == null || rendB == null) return;

        Texture encTex = rendA.sharedMaterial != null
            ? rendA.sharedMaterial.mainTexture
            : null;

        if (encTex != null)
        {
            Material filterMat = rendB.material;
            if (filterMat.HasProperty("_EncryptedTex"))
            {
                filterMat.SetTexture("_EncryptedTex", encTex);
            }
        }
    }
}
