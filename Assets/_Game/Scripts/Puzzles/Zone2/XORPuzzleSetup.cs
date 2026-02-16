using UnityEngine;

public class XORPuzzleSetup : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    public GameObject PlaneA;
    public GameObject PlaneB;

    [Header("Materials")]
    public Material PlaneAMaterial;
    public Material RevealerMaterial;

    private void Start()
    {
        SetupPuzzle();
    }

    [ContextMenu("Setup Puzzle")]
    public void SetupPuzzle()
    {
        SetupSheet(PlaneA, PlaneAMaterial, "Message Sheet");
        SetupSheet(PlaneB, RevealerMaterial, "Filter Sheet");
        WireEncryptedTexture();
    }

    private void WireEncryptedTexture()
    {
        // The filter shader needs _EncryptedTex to compute the XOR decode.
        // Copy the encrypted texture from PlaneA's material into PlaneB's material.
        if (PlaneA == null || PlaneB == null) return;

        var rendA = PlaneA.GetComponent<Renderer>();
        var rendB = PlaneB.GetComponent<Renderer>();
        if (rendA == null || rendB == null) return;

        Texture encTex = rendA.sharedMaterial != null
            ? rendA.sharedMaterial.mainTexture
            : null;
        if (encTex == null && PlaneAMaterial != null)
            encTex = PlaneAMaterial.mainTexture;

        if (encTex != null && rendB.material.HasProperty("_EncryptedTex"))
        {
            rendB.material.SetTexture("_EncryptedTex", encTex);
        }
    }

    private void SetupSheet(GameObject obj, Material mat, string sheetName)
    {
        if (obj == null) return;

        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();

        if (obj.GetComponent<Rigidbody>() == null)
        {
            var rb = obj.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        var sheet = obj.GetComponent<PuzzleSheet>();
        if (sheet == null) sheet = obj.AddComponent<PuzzleSheet>();
        sheet.ObjectName = sheetName;
        sheet.UsePhysics = true;

        // Ensure it can be dragged on the table
        if (obj.GetComponent<TableDraggable>() == null)
            obj.AddComponent<TableDraggable>();

        obj.layer = 6;

        if (mat != null)
        {
            var rend = obj.GetComponent<Renderer>();
            if (rend != null) rend.material = mat;
        }
    }
}
