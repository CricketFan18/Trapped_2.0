using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LogicGate : CircuitNode
{
    public enum GateType { AND, OR, XOR, NOT }
    [Header("Settings")]
    public GateType gateType;
    public CircuitNode inputA;
    public CircuitNode inputB;
    [Header("Visuals")]
    public Renderer gateRenderer; 
    public Material matOn;        
    public Material matOff;      
    [Header("Wire Visuals")]
    public LineRenderer lineToA;
    public LineRenderer lineToB;
    public override bool IsActive()
    {
        bool a = (inputA != null) && inputA.IsActive();
        bool b = (inputB != null) && inputB.IsActive();
        switch (gateType)
        {
            case GateType.AND: return a && b;
            case GateType.OR: return a || b;
            case GateType.XOR: return a != b;
            case GateType.NOT: return !a;
            default: return false;
        }
    }
    void Update()
    {
        bool active = IsActive();
        if (gateRenderer != null && matOn != null && matOff != null)
        {
            gateRenderer.material = active ? matOn : matOff;
        }
        DrawWire(lineToA, inputA);
        DrawWire(lineToB, inputB);
    }
    void DrawWire(LineRenderer line, CircuitNode target)
    {
        if (line == null || target == null) return;
        line.positionCount = 2;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, target.transform.position);
        Color wireColor = target.IsActive() ? Color.green : Color.gray;
        line.startColor = wireColor;
        line.endColor = wireColor;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
    }
}
