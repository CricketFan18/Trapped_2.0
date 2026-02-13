using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Item",menuName = "newItem")]
public class KS_Item :ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject itemPrefab;

    public int stealthValue;
    public int bulkValue;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
