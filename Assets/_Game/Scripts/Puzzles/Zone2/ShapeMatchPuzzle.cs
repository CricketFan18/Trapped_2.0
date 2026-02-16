using UnityEngine;

/// <summary>
/// Shape-matching puzzle table.
/// The player sees the decoded secret shape as a reference and must drag
/// the matching shape card onto the answer zone from three choices.
/// </summary>
public class ShapeMatchPuzzle : PuzzleBase
{
    [Header("Puzzle References")]
    public LightTableSurface Table;
    public Transform AnswerZone;
    public ShapeCard[] Cards;

    [Header("Feedback")]
    public float SnapDistance = 0.15f;

    private ShapeCard _submittedCard;
    private ShapeCard.Shape _correctShape = ShapeCard.Shape.Cross;

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
    }

    private void Update()
    {
        if (IsSolved) return;
        if (Table == null || !Table.IsInUse()) return;
        if (AnswerZone == null) return;

        // Check if a card was just dropped near the answer zone
        foreach (var card in Cards)
        {
            if (card == null) continue;

            var draggable = card.GetComponent<TableDraggable>();
            if (draggable == null) continue;

            // Only check cards that are not currently being dragged
            if (draggable.IsDragging) continue;

            float dist = Vector3.Distance(
                new Vector3(card.transform.position.x, 0, card.transform.position.z),
                new Vector3(AnswerZone.position.x, 0, AnswerZone.position.z));

            if (dist < SnapDistance)
            {
                // A card is in the answer zone
                if (_submittedCard != card)
                {
                    _submittedCard = card;
                    EvaluateAnswer(card);
                }
                return;
            }
        }

        // If no card is near the zone, clear the submission
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

            OnPuzzleSolved();
            Debug.Log($"[ShapeMatch] Correct! Shape: {card.CardShape}");
        }
        else
        {
            Debug.Log($"[ShapeMatch] Wrong shape: {card.CardShape}, expected {_correctShape}");
            card.ResetPosition();
            _submittedCard = null;
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
    }

    public override void ResetPuzzle()
    {
        base.ResetPuzzle();
        _submittedCard = null;

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
