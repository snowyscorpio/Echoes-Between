using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PortalTrigger : MonoBehaviour
{
    private bool alreadyTriggered = false;

    private readonly List<string> levelSceneNames = new List<string>
    {
        "Level_1",
        "Level_2",
        "Level_3",
        "Level_4"
    };

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!alreadyTriggered && other.CompareTag("Player"))
        {
            alreadyTriggered = true;

            string currentScene = SceneManager.GetActiveScene().name;
            int currentIndex = levelSceneNames.IndexOf(currentScene);

            if (currentIndex >= 0 && currentIndex < levelSceneNames.Count - 1)
            {
                string nextScene = levelSceneNames[currentIndex + 1];
                LoadingManager.SceneToLoad = nextScene;
                SceneManager.LoadScene("Loading"); // מסך טעינה
            }
            else
            {
                Debug.LogWarning("No next scene found or this is the last level.");
            }
        }
    }
}
