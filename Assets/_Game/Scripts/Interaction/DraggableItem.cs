using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DraggableItem : MonoBehaviour
{
    public Canvas canvas;

    private GameObject selectedItem;
    private Transform originalParent;
    private Vector3 originalPosition;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectItem();
        }

        if (Input.GetMouseButton(0) && selectedItem != null)
        {
            selectedItem.transform.position = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && selectedItem != null)
        {
            TryPlaceItem();
        }
    }

    void TrySelectItem()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (RaycastResult result in results)
        {
            Image img = result.gameObject.GetComponent<Image>();
            if (img != null && result.gameObject.CompareTag("Item"))
            {
                selectedItem = result.gameObject;
                originalParent = selectedItem.transform.parent;
                originalPosition = selectedItem.transform.position;

                selectedItem.transform.SetParent(canvas.transform);
                break;
            }
        }
    }

    void TryPlaceItem()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        bool placed = false;

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("Slot"))
            {
                Transform slot = result.gameObject.transform;

                if (slot.GetComponentInChildren<Image>())
                {
                    Transform otherItem = slot.GetChild(0);
                    otherItem.SetParent(originalParent);
                    otherItem.localPosition = Vector3.zero;

                    selectedItem.transform.SetParent(slot);
                    selectedItem.transform.localPosition = Vector3.zero;
                }

                placed = true;
                break;
            }
        }

        if (!placed)
        {
            selectedItem.transform.SetParent(originalParent);
            selectedItem.transform.position = originalPosition;
        }

        selectedItem = null;
    }
}