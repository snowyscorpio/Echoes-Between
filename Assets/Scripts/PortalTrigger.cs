using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// This script handles the logic for a portal that triggers a transition to the next level.
/// It checks the current scene, determines the next level from a predefined list,
/// clears the saved player position, and loads a loading screen to transition scenes.
/// </summary>
public class PortalTrigger : MonoBehaviour
{
    // Prevents the trigger logic from running multiple times in a row
    private bool alreadyTriggered = false;

    // List of scene names in order of progression
    private readonly List<string> levelSceneNames = new List<string>
    {
        "Level_1",
        "Level_2",
        "Level_3",
        "Level_4"
    };

    /// <summary>
    /// Called when another collider enters the trigger area of this GameObject.
    /// If the collider belongs to the player and this portal wasn't triggered yet,
    /// it finds the next level and starts loading it.
    /// </summary>
    /// <param name="other">The Collider2D that entered the trigger</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger once, and only if the object is the player
        if (!alreadyTriggered && other.CompareTag("Player"))
        {
            alreadyTriggered = true;

            // Get the name of the current scene
            string currentScene = SceneManager.GetActiveScene().name;

            // Find the index of the current scene in the level list
            int currentIndex = levelSceneNames.IndexOf(currentScene);

            // If we're not at the last level, move to the next one
            if (currentIndex >= 0 && currentIndex < levelSceneNames.Count - 1)
            {
                string nextScene = levelSceneNames[currentIndex + 1];
                GameManager.Instance.LastSavedPositionForSession = null;
                LoadingManager.SceneToLoad = nextScene;
                SceneManager.LoadScene("Loading");
            }
            else
            {
                // If we're already at the last level or the scene is not found in the list
                Debug.LogWarning("No next scene found or this is the last level.");
            }
        }
    }
}
