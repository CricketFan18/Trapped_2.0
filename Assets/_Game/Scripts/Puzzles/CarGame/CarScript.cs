using UnityEngine;
using UnityEngine.EventSystems;

public class CarScript : MonoBehaviour, IPointerClickHandler
{
    public int carIndex;
    private CarsManager _manager;

    private void Start()
    {
        // Find the manager on the parent UI
        _manager = GetComponentInParent<CarsManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_manager != null) _manager.StartCar(carIndex);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_manager != null) _manager.StopCars();
    }
}