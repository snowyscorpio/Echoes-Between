using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonNavigation : MonoBehaviour
{
    public void OptionMenu()
    {
        GameManager.Instance.LastSceneBeforeOptions = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("OptionsMenu");
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }

    public void SessionList()
    {
        SceneManager.LoadScene(2);
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
