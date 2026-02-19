using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// 1. Inherit from BasePuzzleUI
public class Puzzle_MSTNetwork : BasePuzzleUI
{
    [Header("UI References")]
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button backButton;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pruneSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip successSound;

    // Graph Data
    private List<NodeUI> allNodes = new();
    private List<EdgeUI> activeEdges = new();
    private List<EdgeUI> allInitialEdges = new();
    private List<EdgeUI> removedEdgeHistory = new();
    private float playerTotalCost;
    private int trueMinimumWeight;
    private bool isSolved = false;

    // 2. Override OnSetup (Replaces Awake/Start)
    protected override void OnSetup()
    {
        SetupNodesAndGraph();
        trueMinimumWeight = CalculateTrueMSTWeight();

        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        UpdateCostUI();

        // Hook up buttons
        confirmButton.onClick.AddListener(OnConfirm);
        resetButton.onClick.AddListener(OnReset);
        backButton.onClick.AddListener(OnBack);
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
        GameObject obj = new GameObject("Edge", typeof(Image), typeof(Button));
        obj.transform.SetParent(lineContainer, false);

        EdgeUI e = obj.AddComponent<EdgeUI>();
        e.Initialize(a, b, w);
        obj.GetComponent<Button>().onClick.AddListener(() => RemoveEdge(e));

        RectTransform rt = obj.GetComponent<RectTransform>();
        Vector2 dir = b.RectTransform.anchoredPosition - a.RectTransform.anchoredPosition;
        rt.sizeDelta = new Vector2(dir.magnitude, 10f);
        rt.anchoredPosition = a.RectTransform.anchoredPosition + dir / 2f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0, 0, angle);
        obj.GetComponent<Image>().color = new Color(1, 1, 1, 0.45f);

        GameObject txtObj = new GameObject("Label");
        txtObj.transform.SetParent(obj.transform, false);

        TextMeshProUGUI t = txtObj.AddComponent<TextMeshProUGUI>();
        t.text = w.ToString();
        t.fontSize = 28;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;

        activeEdges.Add(e);
        allInitialEdges.Add(e);
        playerTotalCost += w;
    }

    private void RemoveEdge(EdgeUI e)
    {
        if (audioSource != null && pruneSound != null)
        {
            audioSource.PlayOneShot(pruneSound);
        }
        if (isSolved) return; // Prevent interaction if already won

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
        if (isSolved) return;
        if (feedbackText != null) feedbackText.gameObject.SetActive(true);

        if (!CheckConnectivity() || activeEdges.Count != 7 || (int)playerTotalCost > trueMinimumWeight)
        {
            if (audioSource != null && errorSound != null)
            {
                audioSource.PlayOneShot(errorSound);
            }
        }

        if (!CheckConnectivity()) ShowFeedback("<color=red>Error: Network Disconnected</color>");
        else if (activeEdges.Count != 7) ShowFeedback("<color=red>Error: Cycles Detected</color>");
        else if ((int)playerTotalCost > trueMinimumWeight) ShowFeedback($"<color=orange>Efficiency Low. Current: {(int)playerTotalCost}</color>");
        else
        {
            // --- NEW: Play the success sound ---
            if (audioSource != null && successSound != null)
            {
                audioSource.PlayOneShot(successSound);
            }
            // -----------------------------------

            ShowFeedback("<color=green>SUCCESS! System Stabilized.</color>");
            isSolved = true;

            CompletePuzzle();

            StartCoroutine(CloseDelay());
        }
    }

    public void OnBack()
    {
        if (isSolved) return;
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

    public void OnReset()
    {
        if (isSolved) return;
        SetupNodesAndGraph();
        UpdateCostUI();
    }

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

    // Uses the UIManager to close safely
    private IEnumerator CloseDelay()
    {
        yield return new WaitForSeconds(2f);
        UIManager.Instance.CloseActivePuzzle();
    }

    // 4. Implement the required "Solved" visuals
    protected override void OnShowSolvedState()
    {
        isSolved = true;
        confirmButton.interactable = false;
        resetButton.interactable = false;
        backButton.interactable = false;
        ShowFeedback("<color=green>SYSTEM STABILIZED (SOLVED)</color>");
        if (feedbackText != null) feedbackText.gameObject.SetActive(true);
    }
}