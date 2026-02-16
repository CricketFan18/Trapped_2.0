using UnityEngine;
using UnityEditor;
using System.IO;

public class XORPuzzleCreator : EditorWindow
{
    [MenuItem("Tools/Create XOR Puzzle Prefab")]
    public static void CreateXORPuzzlePrefab()
    {
        // 1. Create the Root Object
        GameObject puzzleRoot = new GameObject("XOR_Puzzle_Prefab");
        
        // 2. Add the Logic Script
        XORPuzzle puzzleScript = puzzleRoot.AddComponent<XORPuzzle>();
        puzzleScript.PuzzleName = "Hidden Message";
        puzzleScript.PuzzleDescription = "Overlay the UV filter to reveal the code.";
        puzzleScript.SolutionCode = "1234";

        // --- Light Table ---
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "LightTable";
        table.transform.SetParent(puzzleRoot.transform);
        table.transform.localPosition = new Vector3(0, -1.05f, 0); 
        table.transform.localScale = new Vector3(2f, 1f, 1.5f); 
        var tableRenderer = table.GetComponent<Renderer>();
        if (tableRenderer) tableRenderer.sharedMaterial = new Material(Shader.Find("Standard")) { color = Color.gray };

        // --- Plane A (Hidden Message) ---
        GameObject planeA = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeA.name = "Message_Sheet";
        planeA.transform.SetParent(puzzleRoot.transform);
        planeA.transform.localPosition = new Vector3(-0.4f, -0.5f, 0); 
        planeA.transform.localRotation = Quaternion.identity; 
        planeA.transform.localScale = Vector3.one * 0.1f; 

        DestroyImmediate(planeA.GetComponent<Collider>());
        var boxA = planeA.AddComponent<BoxCollider>();
        boxA.size = new Vector3(10f, 1f, 10f); 
        planeA.layer = 6; // Interactable

        var rbA = planeA.AddComponent<Rigidbody>();
        rbA.collisionDetectionMode = CollisionDetectionMode.Continuous;

        var grabA = planeA.AddComponent<GrabbableObject>();
        grabA.ObjectName = "Message Sheet";
        grabA.HeldRotationOffset = new Vector3(-90, 0, 0); 

        // --- Plane B (Filter) ---
        GameObject planeB = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeB.name = "Filter_Sheet";
        planeB.transform.SetParent(puzzleRoot.transform);
        planeB.transform.localPosition = new Vector3(0.4f, -0.5f, 0);
        planeB.transform.localRotation = Quaternion.identity; 
        planeB.transform.localScale = Vector3.one * 0.1f;

        DestroyImmediate(planeB.GetComponent<Collider>());
        var boxB = planeB.AddComponent<BoxCollider>();
        boxB.size = new Vector3(10f, 1f, 10f);
        planeB.layer = 6; // Interactable

        var rbB = planeB.AddComponent<Rigidbody>();
        rbB.collisionDetectionMode = CollisionDetectionMode.Continuous;

        var grabB = planeB.AddComponent<GrabbableObject>();
        grabB.ObjectName = "Filter Sheet";
        grabB.HeldRotationOffset = new Vector3(-90, 0, 0); 

        // Link References
        puzzleScript.LayerA = planeA;
        puzzleScript.LayerB = planeB;

        // --- Texture Generation & Saving ---
        string assetPath = "Assets/_Game/Art/Materials/Generated";
        if (!Directory.Exists(assetPath)) Directory.CreateDirectory(assetPath);

        int texSize = 256;
        Texture2D secretTex = GenerateSecretTexture(texSize);
        Texture2D keyTex = GenerateNoiseTexture(texSize);
        Texture2D encryptedTex = GenerateEncryptedTexture(secretTex, keyTex);

        // Save Textures as Assets
        SaveTextureAsAsset(secretTex, $"{assetPath}/XOR_Secret.png");
        SaveTextureAsAsset(keyTex, $"{assetPath}/XOR_Key.png");
        SaveTextureAsAsset(encryptedTex, $"{assetPath}/XOR_Encrypted.png");

        // Load them back to assign
        AssetDatabase.Refresh();
        Texture2D keyTexAsset = AssetDatabase.LoadAssetAtPath<Texture2D>($"{assetPath}/XOR_Key.png");
        Texture2D encryptedTexAsset = AssetDatabase.LoadAssetAtPath<Texture2D>($"{assetPath}/XOR_Encrypted.png");

        // Create Materials for Plane A
        Shader unlitShader = Shader.Find("Unlit/Texture");
        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (urpUnlit != null) unlitShader = urpUnlit;
        
        Material matA = new Material(unlitShader);
        matA.mainTexture = encryptedTexAsset;
        if (urpUnlit != null) matA.SetTexture("_BaseMap", encryptedTexAsset);
        
        AssetDatabase.CreateAsset(matA, $"{assetPath}/Mat_XOR_Message.mat");
        planeA.GetComponent<Renderer>().sharedMaterial = matA;

        // Create Materials for Plane B
        Material matB;
        Shader s = Shader.Find("Custom/XORReveal");
        if (s != null)
        {
            matB = new Material(s);
        }
        else
        {
             Debug.LogError("Custom/XORReveal shader not found! Using Standard shader.");
             matB = new Material(Shader.Find("Standard"));
        }
        
        matB.mainTexture = keyTexAsset;
        AssetDatabase.CreateAsset(matB, $"{assetPath}/Mat_XOR_Filter.mat");
        planeB.GetComponent<Renderer>().sharedMaterial = matB;

        // --- Create Prefab ---
        string prefabPath = "Assets/_Game/Prefabs";
        if (!Directory.Exists(prefabPath)) Directory.CreateDirectory(prefabPath);
        
        string prefabAssetPath = $"{prefabPath}/XOR_Puzzle.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(puzzleRoot, prefabAssetPath, InteractionMode.UserAction);

        Debug.Log($"XOR Puzzle Prefab created at: {prefabAssetPath}");
    }

    private static void SaveTextureAsAsset(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }
    
    private static Texture2D GenerateSecretTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] cols = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                cols[y * size + x] = Color.black;
                if (x > size * 0.25f && x < size * 0.75f && y > size * 0.25f && y < size * 0.75f)
                {
                    if (!(x > size * 0.4f && x < size * 0.6f && y > size * 0.4f && y < size * 0.6f))
                    {
                        cols[y * size + x] = Color.white;
                    }
                }
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateNoiseTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++)
        {
            float val = Random.value > 0.5f ? 1f : 0f;
            cols[i] = new Color(val, val, val, 1f);
        }
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateEncryptedTexture(Texture2D secret, Texture2D key)
    {
        int size = secret.width;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] secretCols = secret.GetPixels();
        Color[] keyCols = key.GetPixels();
        Color[] resultCols = new Color[secretCols.Length];
        for (int i = 0; i < secretCols.Length; i++)
        {
            bool s = secretCols[i].r > 0.5f;
            bool k = keyCols[i].r > 0.5f;
            bool r = (s != k);
            float val = r ? 1f : 0f;
            resultCols[i] = new Color(val, val, val, 1f);
        }
        tex.SetPixels(resultCols);
        tex.Apply();
        return tex;
    }
}
