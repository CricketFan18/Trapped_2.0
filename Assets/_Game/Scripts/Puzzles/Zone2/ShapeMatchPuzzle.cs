using UnityEngine;

/// <summary>
/// Shape-matching puzzle table.
/// The player must first discover the secret shape by overlapping both XOR sheets
/// and shining the UV light. Then they drag the matching card onto the answer zone.
/// No reference image is shown — the player must remember what they saw.
/// </summary>
public class ShapeMatchPuzzle : PuzzleBase
{
    [Header("Puzzle References")]
    public LightTableSurface Table;
    public Transform AnswerZone;
    public ShapeCard[] Cards;

    [Header("XOR Puzzle Link")]
    [Tooltip("Reference to the XOR puzzle. Shape matching only accepts answers after the XOR decode has been seen.")]
    public XORPuzzle LinkedXORPuzzle;

    [Header("Feedback")]
    public float SnapDistance = 0.15f;

    [Header("Torch Auto-Snap")]
    [Tooltip("Max distance from the table center before the torch is auto-snapped onto the table.")]
    public float TorchSnapMaxDistance = 5f;

    private ShapeCard _submittedCard;
    private ShapeCard.Shape _correctShape = ShapeCard.Shape.Cross;
    private bool _xorRevealed = false;
    private ShapeCard _lastDraggedCard;

    private void Start()
    {
        if (Table == null)
        {
            var t = transform.Find("ShapeTable");
            if (t != null) Table = t.GetComponent<LightTableSurface>();
        }

        if (AnswerZone == null)
        {
            var t = transform.Find("AnswerZone");
            if (t == null)
            {
                var zone = transform.Find("ShapeTable/AnswerZone");
                if (zone != null) AnswerZone = zone;
            }
            else
            {
                AnswerZone = t;
            }
        }

        if (Cards == null || Cards.Length == 0)
            Cards = GetComponentsInChildren<ShapeCard>();

        if (LinkedXORPuzzle == null)
            LinkedXORPuzzle = GetComponent<XORPuzzle>()
                           ?? GetComponentInParent<XORPuzzle>();

        // Unparent cards from the non-uniformly scaled table so their
        // colliders are not crushed to sub-millimetre thickness.
        // This keeps them in world space while preserving their visual position.
        FixCardParenting();

        SnapTorchToTable();
    }

    /// <summary>
    /// If shape cards are children of a non-uniformly scaled table, unparent
    /// them so their BoxColliders have correct world-space dimensions.
    /// </summary>
    private void FixCardParenting()
    {
        if (Cards == null) return;

        foreach (var card in Cards)
        {
            if (card == null) continue;

            Transform parent = card.transform.parent;
            if (parent == null) continue;

            Vector3 parentScale = parent.lossyScale;
            bool isNonUniform = !Mathf.Approximately(parentScale.x, parentScale.y)
                             || !Mathf.Approximately(parentScale.y, parentScale.z);

            if (isNonUniform)
            {
                // Capture correct world pose before unparenting
                Vector3 worldPos = card.transform.position;
                Quaternion worldRot = card.transform.rotation;

                // Compute the actual world-space visual size
                // Card localScale is applied on top of the parent's lossy scale
                Vector3 ls = card.transform.localScale;
                Vector3 worldScale = new Vector3(
                    ls.x * parentScale.x,
                    ls.y * parentScale.y,
                    ls.z * parentScale.z);

                // Reparent to puzzle root (uniform scale)
                card.transform.SetParent(transform, true);
                card.transform.position = worldPos;
                card.transform.rotation = worldRot;

                // Recompute local scale so the card looks the same size
                Vector3 newParentScale = transform.lossyScale;
                card.transform.localScale = new Vector3(
                    worldScale.x / newParentScale.x,
                    worldScale.y / newParentScale.y,
                    worldScale.z / newParentScale.z);

                // Re-save the start position for ResetPosition()
                card.SaveStartPose();

                Debug.Log($"[ShapeMatch] Reparented card '{card.name}' from non-uniform parent. WorldPos={worldPos}");
            }
        }
    }

    private void Update()
    {
        if (IsSolved) return;

        // Track whether the player has ever seen the XOR reveal
        // (sheets overlapping while UV light is on)
        if (!_xorRevealed && LinkedXORPuzzle != null && LinkedXORPuzzle.SheetsOverlapping)
        {
            // Check if UV light is actually on
            float uvEnabled = Shader.GetGlobalFloat("_UVLightEnabled");
            if (uvEnabled > 0.5f)
            {
                _xorRevealed = true;
                Debug.Log("[ShapeMatch] XOR pattern revealed — shape matching now active.");
            }
        }

        if (Table == null || !Table.IsInUse()) return;
        if (AnswerZone == null) return;

        // Don't evaluate until the player has seen the XOR decode
        if (!_xorRevealed)
        {
            return;
        }

        // Track card drag highlight
        UpdateDragHighlight();

        // Check if a card was just dropped near the answer zone
        foreach (var card in Cards)
        {
            if (card == null) continue;

            var draggable = card.GetComponent<TableDraggable>();
            if (draggable == null) continue;

            // Only check cards that are not currently being dragged
            if (draggable.IsDragging) continue;

            // Use full 3D distance so cards at different heights still register
            Vector3 cardXZ = new Vector3(card.transform.position.x, 0, card.transform.position.z);
            Vector3 zoneXZ = new Vector3(AnswerZone.position.x, 0, AnswerZone.position.z);
            float dist = Vector3.Distance(cardXZ, zoneXZ);

            if (dist < SnapDistance)
            {
                // A card is in the answer zone — evaluate only once per drop
                if (_submittedCard != card)
                {
                    _submittedCard = card;
                    EvaluateAnswer(card);
                }
                return;
            }
        }

        // If no card is near the zone, clear the submission so a card
        // that was moved away can be re-evaluated when dropped again
        _submittedCard = null;
    }

    private void EvaluateAnswer(ShapeCard card)
    {
        if (card.CardShape == _correctShape)
        {
            // Snap to center of answer zone
            Vector3 snapPos = AnswerZone.position;
            snapPos.y = card.transform.position.y;
            card.transform.position = snapPos;

            card.SetCorrect();
            OnPuzzleSolved();
            Debug.Log($"[ShapeMatch] Correct! Shape: {card.CardShape}");
        }
        else
        {
            Debug.Log($"[ShapeMatch] Wrong shape: {card.CardShape}, expected {_correctShape}");
            card.Flash(new Color(1f, 0.3f, 0.3f, 1f), 0.6f);
            card.ResetPosition();
            _submittedCard = null;
        }
    }

    private void UpdateDragHighlight()
    {
        ShapeCard currentlyDragged = null;

        foreach (var card in Cards)
        {
            if (card == null) continue;
            var draggable = card.GetComponent<TableDraggable>();
            if (draggable != null && draggable.IsDragging)
            {
                currentlyDragged = card;
                break;
            }
        }

        if (currentlyDragged != _lastDraggedCard)
        {
            // Remove highlight from previous card
            if (_lastDraggedCard != null)
                _lastDraggedCard.SetHighlight(false);

            // Apply highlight to new card
            if (currentlyDragged != null)
                currentlyDragged.SetHighlight(true);

            _lastDraggedCard = currentlyDragged;
        }
    }

    /// <summary>
    /// Finds any UVLight in the scene and snaps it onto the table surface
    /// if it is too far away from the table.
    /// </summary>
    private void SnapTorchToTable()
    {
        if (Table == null) return;

        // Find all UVLight instances (could be children of this puzzle or elsewhere in the scene)
        UVLight[] allUV = Object.FindObjectsOfType<UVLight>();
        if (allUV.Length == 0) return;

        Vector3 tableCenter = Table.transform.position;
        if (Table.TryGetSurfacePose(out Vector3 surfPos, out Quaternion _))
            tableCenter = surfPos;

        foreach (var uv in allUV)
        {
            float dist = Vector3.Distance(uv.transform.position, tableCenter);
            if (dist > TorchSnapMaxDistance)
            {
                // Determine drop height from TableDraggable settings
                var draggable = uv.GetComponent<TableDraggable>();
                float lightDropHeight = draggable != null ? draggable.LightDropHeight : 0.2f;

                // Place the torch slightly offset from center so it doesn't overlap sheets
                Vector3 snapPos = tableCenter + Table.transform.right * 0.3f;
                snapPos.y = tableCenter.y + lightDropHeight;
                uv.transform.position = snapPos;

                // Orient upside-down so the light points downward at the table
                uv.transform.rotation = Quaternion.Euler(180f, Table.transform.eulerAngles.y, 0f);

                // Ensure it has a TableDraggable component
                if (draggable == null)
                    uv.gameObject.AddComponent<TableDraggable>();

                // Make sure the rigidbody is kinematic so it doesn't fall
                var rb = uv.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                uv.gameObject.layer = 6;

                Debug.Log($"[ShapeMatch] Snapped torch '{uv.name}' to table at {snapPos} (was {dist:F1}m away)");
            }
        }
    }

    public override void OnPuzzleSolved()
    {
        base.OnPuzzleSolved();

        // Disable dragging on all cards
        foreach (var card in Cards)
        {
            if (card == null) continue;
            var draggable = card.GetComponent<TableDraggable>();
            if (draggable != null) draggable.enabled = false;
        }

        // Notify the linked XOR puzzle that the full puzzle chain is complete
        if (LinkedXORPuzzle != null && !LinkedXORPuzzle.IsSolved)
        {
            LinkedXORPuzzle.OnPuzzleSolved();
            Debug.Log("[ShapeMatch] Linked XOR puzzle marked as solved.");
        }
    }

    public override void ResetPuzzle()
    {
        base.ResetPuzzle();
        _submittedCard = null;
        _xorRevealed = false;
        _lastDraggedCard = null;

        foreach (var card in Cards)
        {
            if (card == null) continue;
            card.ResetPosition();
            var draggable = card.GetComponent<TableDraggable>();
            if (draggable != null) draggable.enabled = true;
        }
    }

    // ?????????????????????????????????????????????????????????????
    //  Static texture generators for the three shapes
    // ?????????????????????????????????????????????????????????????

    public static Texture2D GenerateCrossTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] cols = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inVertBar = Mathf.Abs(x - size / 2) < size / 10
                              && Mathf.Abs(y - size / 2) < size / 3;
                bool inHorzBar = Mathf.Abs(x - size / 2) < size / 3
                              && Mathf.Abs(y - size / 2) < size / 10;

                cols[y * size + x] = (inVertBar || inHorzBar) ? Color.black : Color.white;
            }
        }

        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    public static Texture2D GenerateCircleTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] cols = new Color[size * size];
        float center = size * 0.5f;
        float outerR = size * 0.35f;
        float innerR = size * 0.28f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                bool inRing = dist <= outerR && dist >= innerR;
                cols[y * size + x] = inRing ? Color.black : Color.white;
            }
        }

        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    public static Texture2D GenerateTriangleTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] cols = new Color[size * size];
        float cx = size * 0.5f;
        int margin = size / 6;
        int thickness = size / 16;

        // Triangle vertices (pointing up)
        Vector2 a = new Vector2(cx, size - margin);                 // top
        Vector2 b = new Vector2(margin, margin);                    // bottom-left
        Vector2 c = new Vector2(size - margin, margin);             // bottom-right

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y);
                bool near = DistToSegment(p, a, b) < thickness
                         || DistToSegment(p, b, c) < thickness
                         || DistToSegment(p, c, a) < thickness;
                cols[y * size + x] = near ? Color.black : Color.white;
            }
        }

        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    private static float DistToSegment(Vector2 p, Vector2 v, Vector2 w)
    {
        float l2 = (w - v).sqrMagnitude;
        if (l2 < 0.001f) return (p - v).magnitude;
        float t = Mathf.Clamp01(Vector2.Dot(p - v, w - v) / l2);
        Vector2 proj = v + t * (w - v);
        return (p - proj).magnitude;
    }
}
