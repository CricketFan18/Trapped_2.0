using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Puzzle_MSTNetwork : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI instructionText;

    private List<NodeUI> allNodes = new();
    private List<EdgeUI> activeEdges = new();
    private List<EdgeUI> allInitialEdges = new();
    private List<EdgeUI> removedEdgeHistory = new();

    private float playerTotalCost;
    private int targetMSTWeight;

    private void Start()
    {
        SetupUI();
        SpawnUltraSpacedNodes(); // Maximum spacing while holding the structure
        GenerateFixedGraph();
        CalculateHiddenTarget();
        feedbackText.gameObject.SetActive(false);
    }

    private void SetupUI()
    {
        instructionText.text = "<b>SYSTEM STATUS:</b> Redundant Connections Detected.\n" +
                               "<size=22>Prune the network to stabilize the signal.</size>";
    }

    private void SpawnUltraSpacedNodes()
    {
        foreach (Transform child in nodeContainer) Destroy(child.gameObject);
        allNodes.Clear();

        // Structural coordinates from your image
        Vector2[] basePositions = new Vector2[]
        {
            new Vector2(-3.5f, -1.5f), // Node 0
            new Vector2(-3.5f, 1.0f),  // Node 1
            new Vector2(-1.5f, 0.5f),  // Node 2
            new Vector2(-1.0f, 2.0f),  // Node 3
            new Vector2(0.0f, -0.5f),  // Node 4
            new Vector2(1.0f, 1.8f),   // Node 5
            new Vector2(1.2f, 0.5f),   // Node 6
            new Vector2(4.0f, -1.5f)   // Node 7
        };

        // Spacing factor increased to 180 for maximum spread
        float spacingFactor = 180f;

        for (int i = 0; i < basePositions.Length; i++)
        {
            Vector2 spacedPos = basePositions[i] * spacingFactor;

            GameObject obj = Instantiate(nodePrefab, nodeContainer);
            NodeUI node = obj.GetComponent<NodeUI>();
            node.Initialize(i);
            node.RectTransform.anchoredPosition = spacedPos;
            allNodes.Add(node);
        }
    }

    private void GenerateFixedGraph()
    {
        // Connectivity exactly matches your provided graph image
        int[,] edges = new int[,] {
            {0, 1, 25}, {0, 2, 30}, {0, 4, 50}, {0, 7, 95},
            {1, 2, 20}, {1, 3, 35},
            {2, 3, 20}, {2, 4, 25}, {2, 6, 40},
            {3, 5, 30},
            {4, 6, 30}, {4, 7, 60},
            {5, 6, 20}, {5, 7, 70},
            {6, 7, 45}
        };

        for (int i = 0; i < edges.GetLength(0); i++)
        {
            CreateEdge(allNodes[edges[i, 0]], allNodes[edges[i, 1]], edges[i, 2]);
        }
    }

    private void CreateEdge(NodeUI a, NodeUI b, float weight)
    {
        GameObject edgeObj = new GameObject("Edge", typeof(Image), typeof(Button));
        edgeObj.transform.SetParent(lineContainer, false);

        EdgeUI edge = edgeObj.AddComponent<EdgeUI>();
        edge.Initialize(a, b, weight);
        edgeObj.GetComponent<Button>().onClick.AddListener(() => RemoveEdge(edge));

        DrawLine(edgeObj.GetComponent<RectTransform>(), a, b);
        CreateWeightLabel(edgeObj.transform, weight);

        activeEdges.Add(edge);
        allInitialEdges.Add(edge);
        playerTotalCost += weight;
        UpdateCostUI();
    }

    private void CreateWeightLabel(Transform parent, float weight)
    {
        GameObject bg = new GameObject("BG", typeof(Image));
        bg.transform.SetParent(parent, false);
        bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
        bg.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 50);

        GameObject txt = new GameObject("Txt", typeof(RectTransform));
        txt.transform.SetParent(bg.transform, false);
        var t = txt.AddComponent<TextMeshProUGUI>();
        t.text = weight.ToString();
        t.fontSize = 28;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        bg.GetComponent<RectTransform>().rotation = Quaternion.identity;
    }

    private void DrawLine(RectTransform rect, NodeUI a, NodeUI b)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        Vector2 dir = b.RectTransform.anchoredPosition - a.RectTransform.anchoredPosition;
        rect.sizeDelta = new Vector2(dir.magnitude, 10f);
        rect.anchoredPosition = a.RectTransform.anchoredPosition + dir / 2f;
        rect.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        rect.GetComponent<Image>().color = new Color(1, 1, 1, 0.45f);
    }

    private void RemoveEdge(EdgeUI edge)
    {
        if (activeEdges.Contains(edge))
        {
            activeEdges.Remove(edge);
            removedEdgeHistory.Add(edge);
            edge.gameObject.SetActive(false); // Entirely hidden when pruned
            playerTotalCost -= edge.Cost;
            UpdateCostUI();
        }
    }

    public void OnBackPressed()
    {
        if (removedEdgeHistory.Count == 0) return;
        EdgeUI lastRemoved = removedEdgeHistory[removedEdgeHistory.Count - 1];
        removedEdgeHistory.RemoveAt(removedEdgeHistory.Count - 1);

        lastRemoved.gameObject.SetActive(true);
        activeEdges.Add(lastRemoved);
        playerTotalCost += lastRemoved.Cost;
        UpdateCostUI();
    }

    public void OnResetPressed()
    {
        foreach (var edge in allInitialEdges)
        {
            edge.gameObject.SetActive(true);
            if (!activeEdges.Contains(edge)) activeEdges.Add(edge);
        }
        removedEdgeHistory.Clear();
        playerTotalCost = allInitialEdges.Sum(e => e.Cost);
        UpdateCostUI();
    }

    private void CalculateHiddenTarget()
    {
        // 180 is the MST sum for your specific answer image
        targetMSTWeight = 180;
    }

    public void OnConfirmPressed()
    {
        feedbackText.gameObject.SetActive(true);
        bool connected = CheckConnectivity();
        bool hasCycles = activeEdges.Count != 7;
        int currentWeight = (int)playerTotalCost;

        // Vague feedback to keep the "MST" goal a puzzle
        if (!connected)
            ShowFeedback("<b>STABILIZATION FAILED:</b> Network is fragmented.");
        else if (hasCycles)
            ShowFeedback("<b>STABILIZATION FAILED:</b> Loops detected in the signal path.");
        else if (currentWeight > targetMSTWeight)
            ShowFeedback($"<b>STABILIZATION FAILED:</b> Signal efficiency is too low.");
        else
            ShowFeedback("<color=#00FF88>SYSTEM STABILIZED:</color>\nOptimal Efficiency Reached.");
    }

    private bool CheckConnectivity()
    {
        if (activeEdges.Count == 0) return false;
        HashSet<NodeUI> visited = new();
        Queue<NodeUI> q = new Queue<NodeUI>();
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
        return visited.Count == 8;
    }

    private void UpdateCostUI() => costText.text = $"Energy Load: <color=white>{(int)playerTotalCost}</color>";
    private void ShowFeedback(string m) { feedbackText.text = m; StopAllCoroutines(); StartCoroutine(HideFeedback()); }
    private IEnumerator HideFeedback() { yield return new WaitForSeconds(5f); feedbackText.gameObject.SetActive(false); }
}