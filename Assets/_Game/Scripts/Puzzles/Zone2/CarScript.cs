using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CarScript : MonoBehaviour, IPointerClickHandler
{
    public int carIndex; //Red = 0, Green = 1, Blue = 2, Yellow = 3
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(carIndex + "Clicked");
        CarsManager.instance.StartCar(carIndex);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Collided with " + other.gameObject.name);
        CarsManager.instance.StopCars();
    }
}
