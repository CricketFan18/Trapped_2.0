using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Puzzle_MSTNetwork : MonoBehaviour
{
    public static Puzzle_MSTNetwork Instance;

    [Header("UI References")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    // Unused but kept to avoid breaking inspector links
    [SerializeField] private GameObject helpPanel;

    private PuzzleTrigger currentTrigger;

    // Graph Data
    private List<NodeUI> allNodes = new();
    private List<EdgeUI> activeEdges = new();
    private List<EdgeUI> allInitialEdges = new();
    private List<EdgeUI> removedEdgeHistory = new();
    private float playerTotalCost;
    private int trueMinimumWeight;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (puzzlePanel != null) puzzlePanel.SetActive(false);
    }

    public void OpenPuzzle(PuzzleTrigger trigger)
    {
        currentTrigger = trigger;
        if (puzzlePanel != null) puzzlePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartGame();
    }

    public void ClosePuzzle()
    {
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (currentTrigger != null) currentTrigger.OnPuzzleClosed();
    }

    private void StartGame()
    {
        SetupNodesAndGraph();
        trueMinimumWeight = CalculateTrueMSTWeight();
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        UpdateCostUI();
    }

    private void SetupNodesAndGraph()
    {
        // Clear Old Objects
        foreach (Transform child in nodeContainer) Destroy(child.gameObject);
        foreach (Transform child in lineContainer) Destroy(child.gameObject);
        allNodes.Clear();
        activeEdges.Clear();
        allInitialEdges.Clear();
        removedEdgeHistory.Clear();
        playerTotalCost = 0;

        // Spawn Nodes
        Vector2[] positions = new Vector2[] {
            new Vector2(-3.5f, -1.5f), new Vector2(-3.5f, 1.0f),
            new Vector2(-1.5f, 0.5f),  new Vector2(-1.0f, 2.0f),
            new Vector2(0.0f, -0.5f),  new Vector2(1.0f, 1.8f),
            new Vector2(1.2f, 0.5f),   new Vector2(4.0f, -1.5f)
        };

        float spacing = 180f;
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject obj = Instantiate(nodePrefab, nodeContainer);
            NodeUI node = obj.GetComponent<NodeUI>();
            node.Initialize(i);
            node.RectTransform.anchoredPosition = positions[i] * spacing;
            allNodes.Add(node);
        }

        // Generate Edges
        int[,] edgesData = new int[,] {
            {0, 1, 25}, {0, 2, 30}, {0, 4, 50}, {0, 7, 95},
            {1, 2, 20}, {1, 3, 35}, {2, 3, 20}, {2, 4, 25},
            {2, 6, 40}, {3, 5, 30}, {4, 6, 30}, {4, 7, 60},
            {5, 6, 20}, {5, 7, 70}, {6, 7, 45}
        };

        for (int i = 0; i < edgesData.GetLength(0); i++)
        {
            CreateEdge(allNodes[edgesData[i, 0]], allNodes[edgesData[i, 1]], edgesData[i, 2]);
        }
    }

    private void CreateEdge(NodeUI a, NodeUI b, float w)
    {
        // 1. Create the Edge Object
        GameObject obj = new GameObject("Edge", typeof(Image), typeof(Button));
        obj.transform.SetParent(lineContainer, false);

        EdgeUI e = obj.AddComponent<EdgeUI>();
        e.Initialize(a, b, w);
        obj.GetComponent<Button>().onClick.AddListener(() => RemoveEdge(e));

        // 2. Draw the Line
        RectTransform rt = obj.GetComponent<RectTransform>();
        Vector2 dir = b.RectTransform.anchoredPosition - a.RectTransform.anchoredPosition;
        rt.sizeDelta = new Vector2(dir.magnitude, 10f); // Line thickness
        rt.anchoredPosition = a.RectTransform.anchoredPosition + dir / 2f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0, 0, angle);
        obj.GetComponent<Image>().color = new Color(1, 1, 1, 0.45f);

        // 3. Create the Text Label (CRASH FIXED HERE)
        GameObject txtObj = new GameObject("Label"); // Create empty first
        txtObj.transform.SetParent(obj.transform, false);

        TextMeshProUGUI t = txtObj.AddComponent<TextMeshProUGUI>(); // Add component once
        t.text = w.ToString();
        t.fontSize = 28;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;

        // 4. Register
        activeEdges.Add(e);
        allInitialEdges.Add(e);
        playerTotalCost += w;
    }

    private void RemoveEdge(EdgeUI e)
    {
        if (activeEdges.Contains(e))
        {
            activeEdges.Remove(e);
            removedEdgeHistory.Add(e);
            e.gameObject.SetActive(false);
            playerTotalCost -= e.Cost;
            UpdateCostUI();
        }
    }

    private int CalculateTrueMSTWeight()
    {
        List<EdgeUI> sorted = allInitialEdges.OrderBy(e => e.Cost).ToList();
        List<HashSet<int>> sets = new();
        for (int i = 0; i < allNodes.Count; i++) sets.Add(new HashSet<int> { i });

        float sum = 0;
        foreach (var e in sorted)
        {
            var setA = sets.Find(s => s.Contains(e.NodeA.NodeIndex));
            var setB = sets.Find(s => s.Contains(e.NodeB.NodeIndex));
            if (setA != setB)
            {
                sum += e.Cost;
                setA.UnionWith(setB);
                sets.Remove(setB);
            }
        }
        return Mathf.RoundToInt(sum);
    }

    public void OnConfirm()
    {
        if (feedbackText != null) feedbackText.gameObject.SetActive(true);

        if (!CheckConnectivity()) ShowFeedback("Error: Network Disconnected");
        else if (activeEdges.Count != 7) ShowFeedback("Error: Cycles Detected");
        else if ((int)playerTotalCost > trueMinimumWeight) ShowFeedback($"Efficiency Low. Current: {(int)playerTotalCost}");
        else
        {
            ShowFeedback("SUCCESS! System Stabilized.");
            StartCoroutine(CloseDelay());
        }
    }

    public void OnBack()
    {
        if (removedEdgeHistory.Count > 0)
        {
            EdgeUI e = removedEdgeHistory[removedEdgeHistory.Count - 1];
            removedEdgeHistory.RemoveAt(removedEdgeHistory.Count - 1);
            e.gameObject.SetActive(true);
            activeEdges.Add(e);
            playerTotalCost += e.Cost;
            UpdateCostUI();
        }
    }

    public void OnReset() => StartGame();
    private bool CheckConnectivity()
    {
        if (activeEdges.Count == 0) return false;
        HashSet<NodeUI> visited = new();
        Queue<NodeUI> q = new();
        q.Enqueue(allNodes[0]);
        visited.Add(allNodes[0]);
        while (q.Count > 0)
        {
            NodeUI curr = q.Dequeue();
            foreach (var e in activeEdges)
            {
                NodeUI n = (e.NodeA == curr) ? e.NodeB : (e.NodeB == curr) ? e.NodeA : null;
                if (n != null && !visited.Contains(n)) { visited.Add(n); q.Enqueue(n); }
            }
        }
        return visited.Count == allNodes.Count;
    }

    private void UpdateCostUI() { if (costText != null) costText.text = $"Load: {(int)playerTotalCost}"; }
    private void ShowFeedback(string m) { if (feedbackText != null) feedbackText.text = m; }
    private IEnumerator CloseDelay() { yield return new WaitForSeconds(2f); ClosePuzzle(); }
}