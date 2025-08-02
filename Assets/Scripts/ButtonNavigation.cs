using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles button navigation for switching between scenes such as Options, Session List, and Main Menu.
/// </summary>
public class ButtonNavigation : MonoBehaviour
{
    /// <summary>
    /// Navigates to the Options Menu scene.
    /// Stores the current scene name in GameManager so we can return to it later.
    /// </summary>
    public void OptionMenu()
    {
        GameManager.Instance.LastSceneBeforeOptions = SceneManager.GetActiveScene().name; // Save current scene name
        SceneManager.LoadScene("OptionsMenu"); // Load options scene
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("QUIT!"); // Log quit (will only be visible in editor)
        Application.Quit(); // Quit the application
    }

    /// <summary>
    /// Loads the session list scene by build index.
    /// Assumes that the session list is at index 2.
    /// </summary>
    public void SessionList()
    {
        SceneManager.LoadScene(2); // Load scene at index 2 (session list)
    }

    /// <summary>
    /// Returns to the main menu.
    /// Assumes main menu is at build index 0.
    /// </summary>
    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene(0); // Load scene at index 0 (main menu)
    }
}
