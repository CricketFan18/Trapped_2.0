using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CarsManager : MonoBehaviour
{
    public static CarsManager instance;
    public RectTransform[] carPivots; //Cars stored in RGBY sequence
    private Quaternion[] originalRot = new Quaternion[4];
    public float rotationSpeed;
    public GameObject UICanvas;
    public Button retryButton;
    public TextMeshProUGUI countdownText;
    private Coroutine[] rotationRoutine = new Coroutine[4];
    private int rotatingCars = 0;
    private float countdown = 10;
    private bool gameOver = false;
    
    private void Start()
    {
        if (instance == null) instance = this;
        for (int i = 0; i < carPivots.Length; i++)
        {
            originalRot[i] = carPivots[i].rotation;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UICanvas.SetActive(true);
        }
        
        if (countdown <= 0)
        {
            StopCars();
            countdownText.text = "Success";
        }
        
        if(gameOver) return;
        
        if (rotatingCars == 4)
        {
            countdown -= Time.deltaTime;
        }
        
        countdownText.text = countdown.ToString("F2");
    }

    public void StartCar(int index)
    {
        if (gameOver) return;
        Debug.Log(index + " Started");
        if (rotationRoutine[index] != null) return;
        rotationRoutine[index] = StartCoroutine(RotateCar(index));
        rotatingCars++;
    }

    IEnumerator RotateCar(int i)
    {
        while (true)
        {
            carPivots[i].Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void StopCars()
    {
        gameOver = true;
        StopAllCoroutines();
        rotatingCars = 0;
        for(int i=0; i< rotationRoutine.Length; i++)
        {
            rotationRoutine[i] = null;
        }
        
        retryButton.gameObject.SetActive(true);
        retryButton.onClick.AddListener(() =>
        {
            for(int i=0; i < carPivots.Length; i++)
            {
                carPivots[i].rotation = originalRot[i];
            }
            countdown = 10;
            retryButton.gameObject.SetActive(false);
            gameOver = false;
        });
    }
}
