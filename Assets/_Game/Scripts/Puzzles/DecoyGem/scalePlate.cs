using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scalePlate : MonoBehaviour
{
    public List<Transform> GemLoc;
    
    public bool PlaceOnPlate(Gem gem)
    {
        int index = CheckEmptyPosition();
        if (index == -1) return false;
        gem.transform.position = GemLoc[index].position;
        gem.transform.SetParent(GemLoc[index]);
        GetComponent<Rigidbody>().isKinematic = true;
        return true;
    }
    
    int CheckEmptyPosition()
    {
        for (int i = 0; i < GemLoc.Count; i++)
        {
            if (GemLoc[i].childCount == 0)
            {
                return i;
            }
        }
        return -1;
    }
}
