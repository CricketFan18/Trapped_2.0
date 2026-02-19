using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject HomePanel;
    public GameObject RegistrationPanel;
    public void StartGame()
    {
        HomePanel.SetActive(false);
        RegistrationPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit"); // works only in build
    }
}
