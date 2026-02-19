using UnityEngine;

/// <summary>
/// Shape-matching puzzle table. (3D Physical Puzzle)
/// </summary>
public class ShapeMatchPuzzle : MonoBehaviour // <-- Changed from PuzzleBase
{
    [Header("System integration")]
    public string PuzzleID = "Zone1_ShapeMatch";
    public int ScoreAward = 100;
    public bool IsSolved { get; private set; } = false; // Replaced PuzzleBase property

    [Header("Puzzle References")]
    public LightTableSurface Table;
    public Transform AnswerZone;
    public ShapeCard[] Cards;

    [Header("XOR Puzzle Link")]
    public XORPuzzle LinkedXORPuzzle;

    [Header("Feedback")]
    public float SnapDistance = 0.15f;
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
            var t = transform.Find("AnswerZone") ?? transform.Find("ShapeTable/AnswerZone");
            if (t != null) AnswerZone = t;
        }

        if (Cards == null || Cards.Length == 0)
            Cards = GetComponentsInChildren<ShapeCard>();

        if (LinkedXORPuzzle == null)
            LinkedXORPuzzle = GetComponent<XORPuzzle>() ?? GetComponentInParent<XORPuzzle>();

        FixCardParenting();
        SnapTorchToTable();
    }

    private void FixCardParenting()
    {
        if (Cards == null) return;
        foreach (var card in Cards)
        {
            if (card == null || card.transform.parent == null) continue;

            Vector3 parentScale = card.transform.parent.lossyScale;
            bool isNonUniform = !Mathf.Approximately(parentScale.x, parentScale.y)
                             || !Mathf.Approximately(parentScale.y, parentScale.z);

            if (isNonUniform)
            {
                Vector3 worldPos = card.transform.position;
                Quaternion worldRot = card.transform.rotation;
                Vector3 ls = card.transform.localScale;
                Vector3 worldScale = new Vector3(ls.x * parentScale.x, ls.y * parentScale.y, ls.z * parentScale.z);

                card.transform.SetParent(transform, true);
                card.transform.position = worldPos;
                card.transform.rotation = worldRot;

                Vector3 newParentScale = transform.lossyScale;
                card.transform.localScale = new Vector3(worldScale.x / newParentScale.x, worldScale.y / newParentScale.y, worldScale.z / newParentScale.z);

                card.SaveStartPose();
            }
        }
    }

    private void Update()
    {
        if (IsSolved) return;

        if (!_xorRevealed && LinkedXORPuzzle != null && LinkedXORPuzzle.SheetsOverlapping)
        {
            if (Shader.GetGlobalFloat("_UVLightEnabled") > 0.5f)
            {
                _xorRevealed = true;
                Debug.Log("[ShapeMatch] XOR pattern revealed — shape matching now active.");
            }
        }

        if (Table == null || !Table.IsInUse() || AnswerZone == null || !_xorRevealed) return;

        UpdateDragHighlight();

        foreach (var card in Cards)
        {
            if (card == null) continue;
            var draggable = card.GetComponent<TableDraggable>();
            if (draggable != null && draggable.IsDragging) continue;

            Vector3 cardXZ = new Vector3(card.transform.position.x, 0, card.transform.position.z);
            Vector3 zoneXZ = new Vector3(AnswerZone.position.x, 0, AnswerZone.position.z);

            if (Vector3.Distance(cardXZ, zoneXZ) < SnapDistance)
            {
                if (_submittedCard != card)
                {
                    _submittedCard = card;
                    EvaluateAnswer(card);
                }
                return;
            }
        }
        _submittedCard = null;
    }

    private void EvaluateAnswer(ShapeCard card)
    {
        if (card.CardShape == _correctShape)
        {
            Vector3 snapPos = AnswerZone.position;
            snapPos.y = card.transform.position.y;
            card.transform.position = snapPos;

            card.SetCorrect();
            OnPuzzleSolved(); // Trigger new solve logic
        }
        else
        {
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
            if (_lastDraggedCard != null) _lastDraggedCard.SetHighlight(false);
            if (currentlyDragged != null) currentlyDragged.SetHighlight(true);
            _lastDraggedCard = currentlyDragged;
        }
    }

    private void SnapTorchToTable()
    {
        if (Table == null) return;
        UVLight[] allUV = Object.FindObjectsOfType<UVLight>();
        if (allUV.Length == 0) return;

        Vector3 tableCenter = Table.transform.position;
        if (Table.TryGetSurfacePose(out Vector3 surfPos, out Quaternion _)) tableCenter = surfPos;

        foreach (var uv in allUV)
        {
            if (Vector3.Distance(uv.transform.position, tableCenter) > TorchSnapMaxDistance)
            {
                var draggable = uv.GetComponent<TableDraggable>();
                float lightDropHeight = draggable != null ? draggable.LightDropHeight : 0.2f;

                Vector3 snapPos = tableCenter + Table.transform.right * 0.3f;
                snapPos.y = tableCenter.y + lightDropHeight;
                uv.transform.position = snapPos;
                uv.transform.rotation = Quaternion.Euler(180f, Table.transform.eulerAngles.y, 0f);

                if (draggable == null) uv.gameObject.AddComponent<TableDraggable>();

                var rb = uv.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
                uv.gameObject.layer = 6;
            }
        }
    }

    // NEW LOGIC: Connect to GameManager instead of PuzzleBase
    public void OnPuzzleSolved()
    {
        if (IsSolved) return;
        IsSolved = true;

        GameManager.Instance.AddScore(ScoreAward, PuzzleID);
        Debug.Log($"[ShapeMatch] Correct! Puzzle Solved: {PuzzleID}");

        foreach (var card in Cards)
        {
            if (card == null) continue;
            var draggable = card.GetComponent<TableDraggable>();
            if (draggable != null) draggable.enabled = false;
        }

        if (LinkedXORPuzzle != null && !LinkedXORPuzzle.IsSolved)
        {
            LinkedXORPuzzle.OnPuzzleSolved();
        }
    }

    public void ResetPuzzle()
    {
        IsSolved = false;
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
}