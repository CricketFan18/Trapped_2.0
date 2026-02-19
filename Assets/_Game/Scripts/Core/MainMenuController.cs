using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject HomePanel;
    public GameObject RegistrationPanel;

    // NEW: Reference for your About screen
    public GameObject AboutPanel;

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

    // NEW: Call this from your "About" button
    public void OpenAbout()
    {
        HomePanel.SetActive(false); // Hides Start, Settings, About, Quit
        AboutPanel.SetActive(true); // Shows your theme text and back button
    }

    // NEW: Call this from the "Back" button inside your AboutPanel
    public void CloseAbout()
    {
        AboutPanel.SetActive(false); // Hides the theme text
        HomePanel.SetActive(true);   // Brings the main menu back
    }
}