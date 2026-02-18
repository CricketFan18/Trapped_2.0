using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpObject : MonoBehaviour, IInteractable
{
    public float pickupRange = 3f;
    public string InteractionPrompt => "E to pick up";

    public string ItemName;
    public Sprite itemIcon;
    public bool Interact(Interactor interactor)
    {
        GetComponent<Rigidbody>().isKinematic = true;
        this.transform.SetParent(interactor.transform);
        transform.localPosition = new Vector3(0.5f, -0.3f, 1f);
        transform.localRotation = Quaternion.identity;

        Debug.Log("PickUp called on " + gameObject.name);
        InventorySystem.Instance.AddToInventory(ItemName, itemIcon);
        Destroy(gameObject);
        return true;
    }
}
