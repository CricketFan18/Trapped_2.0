using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Puzzle_MSTNetwork : BasePuzzleUI
{
    [Header("Puzzle Settings")]
    [Tooltip("How many minutes the terminal locks out after a wrong answer.")]
    [SerializeField] private float lockoutMinutes = 5f;

    [Header("UI References")]
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private GameObject instructionText;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button exitButton;

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

    // Bulletproof Lockout Variables
    private bool isLockedOut = false;
    private float lockoutEndTime = -1f;
    private float lockoutDuration;
    private string currentErrorReason = "";

    protected override void OnSetup()
    {
        SetupNodesAndGraph();
        trueMinimumWeight = CalculateTrueMSTWeight();
        UpdateCostUI();

        // Clear listeners to prevent double-clicks if OnSetup runs multiple times
        if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (exitButton != null) exitButton.onClick.RemoveAllListeners();

        // Hook up buttons
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
        if (resetButton != null) resetButton.onClick.AddListener(OnReset);
        if (backButton != null) backButton.onClick.AddListener(OnBack);
        if (exitButton != null) exitButton.onClick.AddListener(OnExit);

        // Check if the player is returning to a puzzle that is still locked
        if (isLockedOut && Time.time < lockoutEndTime)
        {
            if (instructionText != null) instructionText.SetActive(false);
            if (feedbackText != null) feedbackText.gameObject.SetActive(true);
        }
        else
        {
            // Lockout has ended (or never happened)
            isLockedOut = false;
            if (feedbackText != null) feedbackText.gameObject.SetActive(false);
            if (instructionText != null) instructionText.SetActive(true);
        }
    }

    private void Update()
    {
        if (!isLockedOut) return;

        float timeRemaining = lockoutEndTime - Time.time;

        if (timeRemaining > lockoutDuration)
        {
            // First 2.5 seconds: Show them exactly what they did wrong
            ShowFeedback($"<color=red>{currentErrorReason}</color>");
        }
        else if (timeRemaining > 0)
        {
            // Countdown phase
            int mins = Mathf.FloorToInt(timeRemaining / 60);
            int secs = Mathf.FloorToInt(timeRemaining % 60);
            ShowFeedback($"<color=red>SECURITY LOCKOUT: {mins:00}:{secs:00}</color>");
        }
        else
        {
            // The timer reached zero!
            isLockedOut = false;
            ShowFeedback("<color=yellow>SYSTEM READY.</color>");
            if (instructionText != null) instructionText.SetActive(true);

            StartCoroutine(HideFeedbackDelay());
        }
    }

    private IEnumerator HideFeedbackDelay()
    {
        yield return new WaitForSeconds(3f);
        if (feedbackText != null && !isLockedOut) feedbackText.gameObject.SetActive(false);
    }

    private void SetupNodesAndGraph()
    {
        foreach (Transform child in nodeContainer) Destroy(child.gameObject);
        foreach (Transform child in lineContainer) Destroy(child.gameObject);
        allNodes.Clear();
        activeEdges.Clear();
        allInitialEdges.Clear();
        removedEdgeHistory.Clear();
        playerTotalCost = 0;

        Vector2[] positions = new Vector2[] {
            new Vector2(-3.5f, -1.5f), new Vector2(-3.5f, 1.0f),
            new Vector2(-1.5f, 0.5f),  new Vector2(-1.0f, 2.0f),
            new Vector2(0.0f, -0.5f),  new Vector2(1.0f, 1.8f),
            new Vector2(1.2f, 0.5f),   new Vector2(4.0f, -1.5f)
        };

        float spacing = 150f;
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject obj = Instantiate(nodePrefab, nodeContainer);
            NodeUI node = obj.GetComponent<NodeUI>();
            node.Initialize(i);
            node.RectTransform.anchoredPosition = positions[i] * spacing;
            allNodes.Add(node);
        }

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

        // --- NEW: Create a background box for the text ---
        GameObject bgObj = new GameObject("LabelBackground", typeof(Image));
        bgObj.transform.SetParent(obj.transform, false);

        Image bgImage = bgObj.GetComponent<Image>();
        // Sets a dark, slightly transparent background (R, G, B, Alpha)
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(50f, 40f); // Adjust this to make the box bigger/smaller

        // --- UPDATED: Put the text inside the background box ---
        GameObject txtObj = new GameObject("LabelText");
        txtObj.transform.SetParent(bgObj.transform, false);

        TextMeshProUGUI t = txtObj.AddComponent<TextMeshProUGUI>();
        t.text = w.ToString();
        t.fontSize = 24; 
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;

        // This forces the text boundary to perfectly fill the background box
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;
        // --------------------------------------------------------

        activeEdges.Add(e);
        allInitialEdges.Add(e);
        playerTotalCost += w;
    }

    private void RemoveEdge(EdgeUI e)
    {
        if (isSolved || isLockedOut) return;

        if (activeEdges.Contains(e))
        {
            if (audioSource != null && pruneSound != null) audioSource.PlayOneShot(pruneSound);

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
        if (isSolved || isLockedOut) return;
        if (feedbackText != null) feedbackText.gameObject.SetActive(true);

        bool isConnected = CheckConnectivity();
        bool hasNoCycles = activeEdges.Count == 7;
        bool isOptimal = (int)playerTotalCost <= trueMinimumWeight;

        if (!isConnected || !hasNoCycles || !isOptimal)
        {
            if (audioSource != null && errorSound != null) audioSource.PlayOneShot(errorSound);

            if (!isConnected) currentErrorReason = "Error: Network Disconnected";
            else if (!hasNoCycles) currentErrorReason = "Error: Cycles Detected";
            else currentErrorReason = $"Efficiency Low. Current: {(int)playerTotalCost}";

            // Set the exact time in the future this puzzle will unlock using the Inspector variable
            lockoutDuration = lockoutMinutes * 60f;
            lockoutEndTime = Time.time + lockoutDuration + 2.5f;

            isLockedOut = true;
            if (instructionText != null) instructionText.SetActive(false);
        }
        else
        {
            if (audioSource != null && successSound != null) audioSource.PlayOneShot(successSound);
            if (instructionText != null) instructionText.SetActive(false);

            ShowFeedback("<color=green>SUCCESS! System Stabilized.</color>");
            isSolved = true;
            CompletePuzzle();
            StartCoroutine(CloseDelay());
        }
    }

    public void OnBack()
    {
        if (isSolved || isLockedOut) return;
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
        if (isSolved || isLockedOut) return;
        SetupNodesAndGraph();
        UpdateCostUI();
    }

    public void OnExit()
    {
        UIManager.Instance.CloseActivePuzzle();
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

    private IEnumerator CloseDelay()
    {
        yield return new WaitForSeconds(2f);
        UIManager.Instance.CloseActivePuzzle();
    }

    protected override void OnShowSolvedState()
    {
        isSolved = true;
        if (confirmButton != null) confirmButton.interactable = false;
        if (resetButton != null) resetButton.interactable = false;
        if (backButton != null) backButton.interactable = false;
        if (instructionText != null) instructionText.SetActive(false);
        ShowFeedback("<color=green>SYSTEM STABILIZED (SOLVED)</color>");
        if (feedbackText != null) feedbackText.gameObject.SetActive(true);
    }
}