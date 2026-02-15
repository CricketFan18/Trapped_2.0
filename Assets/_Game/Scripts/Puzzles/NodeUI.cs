using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NodeUI : MonoBehaviour
{
    public int NodeIndex { get; private set; }
    public RectTransform RectTransform { get; private set; }

    [SerializeField] private TextMeshProUGUI indexLabel; // Optional: To see Node ID

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        if (RectTransform == null)
            Debug.LogError("NodeUI requires RectTransform.");
    }

    public void Initialize(int index)
    {
        NodeIndex = index;

        // Useful for double-checking your 8-node structure
        if (indexLabel != null)
            indexLabel.text = index.ToString();
    }
}