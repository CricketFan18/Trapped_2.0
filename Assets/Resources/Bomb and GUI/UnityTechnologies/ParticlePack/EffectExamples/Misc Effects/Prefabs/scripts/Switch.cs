using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : CircuitNode
{
    public bool isOn = false;
    public Renderer meshRenderer;
    public Material matOn;
    public Material matOff;
    private void Start()
    {
        UpdateVisuals();
    }
    private void OnMouseDown()
    {
        if (PuzzleManager.Instance.canInteract)
        {


            isOn = !isOn;
            UpdateVisuals();
            if (PuzzleManager.Instance != null)
            {
                PuzzleManager.Instance.CheckSwitchLimit();
            }
        }
    }
    public void TurnOff()
    {
        isOn = false;
        UpdateVisuals();
    }
    public override bool IsActive()
    {
        return isOn;
    }
    void UpdateVisuals()
    {
        if (meshRenderer != null && matOn != null && matOff != null)
        {
            meshRenderer.material = isOn ? matOn : matOff;
        }
    }
}
