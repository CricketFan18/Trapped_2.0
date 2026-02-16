using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Main_Game");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit"); // works only in build
    }
}
