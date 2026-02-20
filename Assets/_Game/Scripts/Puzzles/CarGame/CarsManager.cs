using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 1. Inherit from BasePuzzleUI, not MonoBehaviour
public class CarsManager : BasePuzzleUI
{
    [Header("Car Setup")]
    public RectTransform[] carPivots;
    public float rotationSpeed;

    [Header("UI Setup")]
    public Button retryButton;
    public TextMeshProUGUI countdownText;
    public GameObject SuccessOverlay; // Optional: A panel showing "ACCESS GRANTED"

    private Quaternion[] originalRot = new Quaternion[4];
    private Coroutine[] rotationRoutine = new Coroutine[4];
    private int rotatingCars = 0;
    private float countdown = 10;
    private bool gameOver = false;

    // 2. Override OnSetup instead of Start
    protected override void OnSetup()
    {
        countdownText.text = "10.00";
        for (int i = 0; i < carPivots.Length; i++)
        {
            originalRot[i] = carPivots[i].rotation;
        }
        retryButton.gameObject.SetActive(false);
        if (SuccessOverlay) SuccessOverlay.SetActive(false);
    }

    private void Update()
    {
        if (gameOver) return;

        if (rotatingCars == 4)
        {
            countdown -= Time.deltaTime;
            countdownText.text = countdown.ToString("F2");

            if (countdown <= 0)
            {
                Success();
            }
        }
    }

    public void StartCar(int index)
    {
        if (gameOver) return;
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

    void Success()
    {
        gameOver = true;
        StopAllCoroutines();

        countdownText.text = "Success!";

        // 3. Tell the core system we won!
        CompletePuzzle();
    }

    // 4. Implement the required "Solved" visuals
    protected override void OnShowSolvedState()
    {
        gameOver = true;
        countdownText.text = "SOLVED";
        retryButton.gameObject.SetActive(false);
        if (SuccessOverlay) SuccessOverlay.SetActive(true); // Lock the UI
    }

    public void StopCars()
    {
        if (gameOver) return;

        gameOver = true;
        StopAllCoroutines();
        rotatingCars = 0;

        for (int i = 0; i < rotationRoutine.Length; i++) { rotationRoutine[i] = null; }

        countdownText.text = "CRASHED!";
        retryButton.GetComponentInChildren<TextMeshProUGUI>().text = "Retry";
        retryButton.gameObject.SetActive(true);
        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(ResetGame);
    }

    private void ResetGame()
    {
        for (int i = 0; i < carPivots.Length; i++)
        {
            carPivots[i].rotation = originalRot[i];
        }
        countdown = 10;
        countdownText.text = "10.00";
        retryButton.gameObject.SetActive(false);
        gameOver = false;
    }
}