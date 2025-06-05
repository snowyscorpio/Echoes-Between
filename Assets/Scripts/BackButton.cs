using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    public Button backButton; 

    void Start()
    {
        backButton.onClick.AddListener(GoBack);
    }

    public void GoBack()
    {
        SceneManager.LoadScene(0);
    }
}
