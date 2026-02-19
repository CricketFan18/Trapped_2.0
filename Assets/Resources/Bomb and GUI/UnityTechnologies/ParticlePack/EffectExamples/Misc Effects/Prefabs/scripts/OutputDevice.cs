using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputDevice : MonoBehaviour
{
    public enum DeviceType { DoorLock, Trap }
    public DeviceType type;
    public CircuitNode inputNode;
    public Renderer meshRenderer;
    [Header("Materials")]
    public Material matActive; 
    public Material matInactive;
    public static bool GlobalSystemFailure = false;
    void Update()
    {
        bool hasPower = (inputNode != null) && inputNode.IsActive();

        if (type == DeviceType.Trap)
        {
            if (hasPower)
            {
                GlobalSystemFailure = true; 
                meshRenderer.material = matActive;
            }
            else
            {
                GlobalSystemFailure = false;
                meshRenderer.material = matInactive;
            }
        }
        else if (type == DeviceType.DoorLock)
        {
            if (hasPower && !GlobalSystemFailure)
            {
                meshRenderer.material = matActive; 
            }
            else
            {
                meshRenderer.material = matInactive; 
            }
        }
    }
}
