using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // This function loads the next scene in the build index
    public void OptionMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // This function quits the application
    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
