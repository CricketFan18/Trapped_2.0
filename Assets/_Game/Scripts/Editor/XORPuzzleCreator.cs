using UnityEngine;
using UnityEditor;
using System.IO;

public class XORPuzzleCreator : EditorWindow
{
    [MenuItem("Tools/Create XOR Puzzle Prefab")]
    public static void CreateXORPuzzlePrefab()
    {
        // 1. Root
        GameObject puzzleRoot = new GameObject("XOR_Puzzle_Prefab");

        XORPuzzle puzzleScript = puzzleRoot.AddComponent<XORPuzzle>();
        puzzleScript.PuzzleName = "Hidden Message";
        puzzleScript.PuzzleDescription = "Overlay the UV filter to reveal the code.";
        puzzleScript.SolutionCode = "1234";

        // --- Light Table ---
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "LightTable";
        table.layer = 6;
        table.transform.SetParent(puzzleRoot.transform);
        table.transform.localPosition = new Vector3(0, 0.5f, 0);
        table.transform.localScale = new Vector3(1.5f, 0.05f, 1.0f);

        var tableRenderer = table.GetComponent<Renderer>();
        if (tableRenderer)
        {
            var tableMat = new Material(Shader.Find("Standard"));
            tableMat.color = new Color(0.2f, 0.2f, 0.2f);
            tableRenderer.sharedMaterial = tableMat;
        }

        // Table legs
        CreateLeg(table, new Vector3(-0.4f, -5f, -0.4f));
        CreateLeg(table, new Vector3(0.4f, -5f, -0.4f));
        CreateLeg(table, new Vector3(-0.4f, -5f, 0.4f));
        CreateLeg(table, new Vector3(0.4f, -5f, 0.4f));

        // --- LightTableSurface component ---
        var tableScript = table.AddComponent<LightTableSurface>();

        // --- Camera Focus Point ---
        // Set world position/rotation BEFORE parenting to avoid non-uniform scale distortion
        GameObject camPivot = new GameObject("CameraFocusPoint");
        // Table surface top is at world Y = 0.5 + 0.025 = 0.525. Camera 1.2m above that.
        camPivot.transform.position = table.transform.position + Vector3.up * (0.025f + 1.2f);
        camPivot.transform.rotation = Quaternion.Euler(75f, 0f, 0f); // Look down at table
        // Parent with worldPositionStays=true so non-uniform table scale doesn't distort
        camPivot.transform.SetParent(table.transform, true);
        tableScript.CameraFocusPoint = camPivot.transform;

        // --- Plane A (Message Sheet) ---
        GameObject planeA = CreateSheet(puzzleRoot, "Message_Sheet", new Vector3(-0.25f, 0.56f, 0));

        var grabA = planeA.AddComponent<PuzzleSheet>();
        grabA.ObjectName = "Message Sheet";
        grabA.UsePhysics = true;
        planeA.AddComponent<TableDraggable>();

        // --- Plane B (Filter Sheet) ---
        GameObject planeB = CreateSheet(puzzleRoot, "Filter_Sheet", new Vector3(0.25f, 0.56f, 0));

        var grabB = planeB.AddComponent<PuzzleSheet>();
        grabB.ObjectName = "Filter Sheet";
        grabB.UsePhysics = true;
        planeB.AddComponent<TableDraggable>();

        // --- UV Flashlight ---
        GameObject flashlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flashlight.name = "UV_Flashlight";
        flashlight.transform.SetParent(puzzleRoot.transform);
        flashlight.transform.localPosition = new Vector3(0.6f, 0.6f, 0);
        flashlight.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
        flashlight.transform.localRotation = Quaternion.Euler(90, 0, 0);
        flashlight.layer = 6;

        var flashMat = new Material(Shader.Find("Standard"));
        flashMat.color = Color.black;
        flashlight.GetComponent<Renderer>().sharedMaterial = flashMat;

        // Spot Light child — points out the +Z end of the cylinder (top)
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
        grabF.HeldRotationOffset = new Vector3(0, 0, 0);
        grabF.PointAtCamera = true;
        flashlight.AddComponent<TableDraggable>();

        // --- Link References ---
        puzzleScript.LayerA = planeA;
        puzzleScript.LayerB = planeB;

        // --- Textures & Materials ---
        GenerateAndAssignTextures(planeA, planeB);

        // --- Shape Matching Table (second table) ---
        CreateShapeMatchTable(puzzleRoot);

        // --- Save Prefab ---
        string prefabPath = "Assets/_Game/Prefabs";
        if (!Directory.Exists(prefabPath)) Directory.CreateDirectory(prefabPath);

        string prefabAssetPath = $"{prefabPath}/XOR_Puzzle.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(puzzleRoot, prefabAssetPath, InteractionMode.UserAction);

        Debug.Log($"XOR Puzzle Prefab created at: {prefabAssetPath}");
    }

    private static GameObject CreateSheet(GameObject parent, string name, Vector3 localPos)
    {
        GameObject sheet = GameObject.CreatePrimitive(PrimitiveType.Quad);
        sheet.name = name;
        sheet.transform.SetParent(parent.transform);
        sheet.transform.localPosition = localPos;
        sheet.transform.localRotation = Quaternion.Euler(90f, 0, 0); // Lay flat face up
        sheet.transform.localScale = new Vector3(0.4f, 0.3f, 1f);
        sheet.layer = 6;

        // Replace MeshCollider with thin BoxCollider for better physics
        DestroyImmediate(sheet.GetComponent<Collider>());
        var box = sheet.AddComponent<BoxCollider>();
        box.size = new Vector3(1f, 1f, 0.02f);

        var rb = sheet.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.mass = 0.1f;

        return sheet;
    }

    private static void CreateLeg(GameObject parent, Vector3 localPos)
    {
        GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leg.name = "Leg";
        leg.transform.SetParent(parent.transform);
        leg.transform.localPosition = localPos;
        leg.transform.localScale = new Vector3(0.05f, 10f, 0.05f);
        DestroyImmediate(leg.GetComponent<Collider>());
    }

    private static void GenerateAndAssignTextures(GameObject planeA, GameObject planeB)
    {
        string assetPath = "Assets/_Game/Art/Materials/Generated";
        if (!Directory.Exists(assetPath)) Directory.CreateDirectory(assetPath);

        int texSize = 512;
        Texture2D secretTex = GenerateSecretTexture(texSize);
        Texture2D keyTex = GenerateNoiseTexture(texSize);
        Texture2D encryptedTex = GenerateEncryptedTexture(secretTex, keyTex);

        SaveTextureAsAsset(secretTex, $"{assetPath}/XOR_Secret.png");
        SaveTextureAsAsset(keyTex, $"{assetPath}/XOR_Key.png");
        SaveTextureAsAsset(encryptedTex, $"{assetPath}/XOR_Encrypted.png");

        Texture2D keyTexAsset = AssetDatabase.LoadAssetAtPath<Texture2D>($"{assetPath}/XOR_Key.png");
        Texture2D encryptedTexAsset = AssetDatabase.LoadAssetAtPath<Texture2D>($"{assetPath}/XOR_Encrypted.png");

        Material matA = new Material(Shader.Find("Standard"));
        matA.mainTexture = encryptedTexAsset;
        matA.SetFloat("_Glossiness", 0f);
        AssetDatabase.CreateAsset(matA, $"{assetPath}/Mat_XOR_Message.mat");
        planeA.GetComponent<Renderer>().sharedMaterial = matA;

        Material matB;
        Shader s = Shader.Find("Custom/XORReveal");
        if (s == null) s = Shader.Find("Standard");

        matB = new Material(s);
        matB.SetTexture("_MainTex", keyTexAsset);
        matB.SetTexture("_EncryptedTex", encryptedTexAsset);
        matB.renderQueue = 3100;

        AssetDatabase.CreateAsset(matB, $"{assetPath}/Mat_XOR_Filter.mat");
        planeB.GetComponent<Renderer>().sharedMaterial = matB;
    }

    private static void SaveTextureAsAsset(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        // Set import settings for crisp binary textures
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }

    private static Texture2D GenerateSecretTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] cols = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                cols[y * size + x] = Color.white;

                if (Mathf.Abs(x - size / 2) < size / 10 && Mathf.Abs(y - size / 2) < size / 3)
                    cols[y * size + x] = Color.black;
                if (Mathf.Abs(x - size / 2) < size / 3 && Mathf.Abs(y - size / 2) < size / 10)
                    cols[y * size + x] = Color.black;
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateNoiseTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
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
        Color[] s = secret.GetPixels();
        Color[] k = key.GetPixels();
        Color[] r = new Color[s.Length];

        for (int i = 0; i < s.Length; i++)
        {
            bool isSecretBlack = s[i].r < 0.5f;
            bool isKeyBlack = k[i].r < 0.5f;
            bool isResultBlack = isSecretBlack ^ isKeyBlack;
            float val = isResultBlack ? 0f : 1f;
            r[i] = new Color(val, val, val, 1f);
        }

        Texture2D tex = new Texture2D(secret.width, secret.height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(r);
        tex.Apply();
        return tex;
    }

    // ?????????????????????????????????????????????????????????????????
    //  Shape Matching Table
    // ?????????????????????????????????????????????????????????????????

    private static void CreateShapeMatchTable(GameObject puzzleRoot)
    {
        string assetPath = "Assets/_Game/Art/Materials/Generated";
        if (!Directory.Exists(assetPath)) Directory.CreateDirectory(assetPath);

        // --- Table ---
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "ShapeTable";
        table.layer = 6;
        table.transform.SetParent(puzzleRoot.transform);
        table.transform.localPosition = new Vector3(2.0f, 0.5f, 0);
        table.transform.localScale = new Vector3(1.5f, 0.05f, 1.0f);

        var tableRend = table.GetComponent<Renderer>();
        if (tableRend)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.25f, 0.2f, 0.18f);
            tableRend.sharedMaterial = mat;
        }

        CreateLeg(table, new Vector3(-0.4f, -5f, -0.4f));
        CreateLeg(table, new Vector3(0.4f, -5f, -0.4f));
        CreateLeg(table, new Vector3(-0.4f, -5f, 0.4f));
        CreateLeg(table, new Vector3(0.4f, -5f, 0.4f));

        var tableScript = table.AddComponent<LightTableSurface>();
        tableScript.InteractionText = "Use Shape Table";

        // Camera focus point
        GameObject camPivot = new GameObject("CameraFocusPoint");
        camPivot.transform.position = table.transform.position + Vector3.up * (0.025f + 1.2f);
        camPivot.transform.rotation = Quaternion.Euler(75f, 0f, 0f);
        camPivot.transform.SetParent(table.transform, true);
        tableScript.CameraFocusPoint = camPivot.transform;

        // --- Answer Zone (visual marker on the table) ---
        GameObject answerZone = GameObject.CreatePrimitive(PrimitiveType.Quad);
        answerZone.name = "AnswerZone";
        // Position at right side of table surface
        answerZone.transform.SetParent(table.transform, false);
        answerZone.transform.localPosition = new Vector3(0.25f, 0.51f, 0);
        answerZone.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        answerZone.transform.localScale = new Vector3(0.22f, 0.22f, 1f);
        answerZone.layer = 0; // Not interactable

        // Remove collider — it's just a visual marker
        DestroyImmediate(answerZone.GetComponent<Collider>());

        // Dashed-border material for the answer zone
        var zoneMat = new Material(Shader.Find("Standard"));
        zoneMat.color = new Color(0.3f, 1f, 0.3f, 0.4f);
        zoneMat.SetFloat("_Mode", 3f); // Transparent
        zoneMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        zoneMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        zoneMat.SetInt("_ZWrite", 0);
        zoneMat.DisableKeyword("_ALPHATEST_ON");
        zoneMat.EnableKeyword("_ALPHABLEND_ON");
        zoneMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        zoneMat.renderQueue = 3000;
        answerZone.GetComponent<Renderer>().sharedMaterial = zoneMat;

        // --- Reference image (the secret shape the player saw via XOR) ---
        GameObject refImage = GameObject.CreatePrimitive(PrimitiveType.Quad);
        refImage.name = "ReferenceShape";
        refImage.transform.SetParent(table.transform, false);
        refImage.transform.localPosition = new Vector3(-0.25f, 0.51f, 0.25f);
        refImage.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        refImage.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
        refImage.layer = 0;
        DestroyImmediate(refImage.GetComponent<Collider>());

        // Use the cross texture from the XOR puzzle as the reference
        int texSize = 256;
        Texture2D refTex = ShapeMatchPuzzle.GenerateCrossTexture(texSize);
        SaveTextureAsAsset(refTex, $"{assetPath}/Shape_Reference.png");
        Texture2D refTexAsset = AssetDatabase.LoadAssetAtPath<Texture2D>($"{assetPath}/Shape_Reference.png");

        var refMat = new Material(Shader.Find("Standard"));
        refMat.mainTexture = refTexAsset;
        refMat.SetFloat("_Glossiness", 0f);
        AssetDatabase.CreateAsset(refMat, $"{assetPath}/Mat_Shape_Reference.mat");
        refImage.GetComponent<Renderer>().sharedMaterial = refMat;

        // --- Label above reference: "Match this shape" ---
        GameObject labelObj = new GameObject("ReferenceLabel");
        labelObj.transform.SetParent(table.transform, false);
        labelObj.transform.localPosition = new Vector3(-0.25f, 0.52f, 0.38f);
        labelObj.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        labelObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        var tm = labelObj.AddComponent<TextMesh>();
        tm.text = "MATCH THIS SHAPE";
        tm.fontSize = 32;
        tm.characterSize = 0.5f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;

        // --- Three shape cards ---
        Texture2D crossTex = ShapeMatchPuzzle.GenerateCrossTexture(texSize);
        Texture2D circleTex = ShapeMatchPuzzle.GenerateCircleTexture(texSize);
        Texture2D triangleTex = ShapeMatchPuzzle.GenerateTriangleTexture(texSize);

        SaveTextureAsAsset(crossTex, $"{assetPath}/Shape_Cross.png");
        SaveTextureAsAsset(circleTex, $"{assetPath}/Shape_Circle.png");
        SaveTextureAsAsset(triangleTex, $"{assetPath}/Shape_Triangle.png");

        Texture2D crossTexAsset = AssetDatabase.LoadAssetAtPath<Texture2D>($"{assetPath}/Shape_Cross.png");
        Texture2D circleTexAsset = AssetDatabase.LoadAssetAtPath<Texture2D>($"{assetPath}/Shape_Circle.png");
        Texture2D triangleTexAsset = AssetDatabase.LoadAssetAtPath<Texture2D>($"{assetPath}/Shape_Triangle.png");

        // Randomize card positions — shuffle the X offsets
        float[] xPositions = { -0.3f, 0f, 0.3f };
        ShuffleArray(xPositions);

        GameObject cardCross = CreateShapeCard(table, "Card_Cross", crossTexAsset,
            new Vector3(xPositions[0], 0.52f, -0.25f), ShapeCard.Shape.Cross, assetPath, "Mat_Card_Cross");
        GameObject cardCircle = CreateShapeCard(table, "Card_Circle", circleTexAsset,
            new Vector3(xPositions[1], 0.52f, -0.25f), ShapeCard.Shape.Circle, assetPath, "Mat_Card_Circle");
        GameObject cardTriangle = CreateShapeCard(table, "Card_Triangle", triangleTexAsset,
            new Vector3(xPositions[2], 0.52f, -0.25f), ShapeCard.Shape.Triangle, assetPath, "Mat_Card_Triangle");

        // --- ShapeMatchPuzzle component ---
        var puzzle = puzzleRoot.AddComponent<ShapeMatchPuzzle>();
        puzzle.PuzzleName = "Shape Match";
        puzzle.PuzzleDescription = "Identify the shape revealed by the XOR decode and drag it to the answer zone.";
        puzzle.Table = tableScript;
        puzzle.AnswerZone = answerZone.transform;
        puzzle.Cards = new ShapeCard[]
        {
            cardCross.GetComponent<ShapeCard>(),
            cardCircle.GetComponent<ShapeCard>(),
            cardTriangle.GetComponent<ShapeCard>()
        };
    }

    private static GameObject CreateShapeCard(GameObject tableParent, string name, Texture2D tex,
        Vector3 localPos, ShapeCard.Shape shape, string assetPath, string matName)
    {
        GameObject card = GameObject.CreatePrimitive(PrimitiveType.Quad);
        card.name = name;
        card.transform.SetParent(tableParent.transform, false);
        card.transform.localPosition = localPos;
        card.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        card.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
        card.layer = 6;

        // Replace MeshCollider with BoxCollider for drag detection
        DestroyImmediate(card.GetComponent<Collider>());
        var box = card.AddComponent<BoxCollider>();
        box.size = new Vector3(1f, 1f, 0.02f);

        var rb = card.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.mass = 0.1f;
        rb.isKinematic = true;
        rb.useGravity = false;

        // Material
        var mat = new Material(Shader.Find("Standard"));
        mat.mainTexture = tex;
        mat.SetFloat("_Glossiness", 0f);

        string matPath = $"{assetPath}/{matName}.mat";
        // Avoid overwrite errors
        if (AssetDatabase.LoadAssetAtPath<Material>(matPath) != null)
            AssetDatabase.DeleteAsset(matPath);
        AssetDatabase.CreateAsset(mat, matPath);
        card.GetComponent<Renderer>().sharedMaterial = mat;

        // Components
        var sc = card.AddComponent<ShapeCard>();
        sc.CardShape = shape;
        card.AddComponent<TableDraggable>();

        return card;
    }

    private static void ShuffleArray(float[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            float temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
