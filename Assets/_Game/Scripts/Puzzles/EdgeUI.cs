using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class EdgeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public NodeUI NodeA { get; private set; }
    public NodeUI NodeB { get; private set; }
    public float Cost { get; private set; }

    private Image image;

    public void Initialize(NodeUI a, NodeUI b, float cost)
    {
        NodeA = a;
        NodeB = b;
        Cost = cost;
        image = GetComponent<Image>();
    }

    // Highlighting for better user feedback
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (image != null)
            image.color = new Color(1, 0.92f, 0.016f, 0.8f); // Soft Yellow
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (image != null)
            image.color = new Color(1, 1, 1, 0.45f); // Back to transparent white
    }
}