using UnityEngine;

// This script is used to trigger a manual save of the player's position
// Attach this script to a UI button or relevant GameObject
public class ManualSaveButton : MonoBehaviour
{
    // Reference to the player GameObject to get its current position
    public GameObject player;

    // Method to be called when the user clicks the save button
    public void SaveNow()
    {
        // Check if the player GameObject is assigned
        if (player != null)
        {
            // Get the current position of the player in the scene
            Vector2 position = player.transform.position;
            // Call SaveManager to manually save the player's position to the database
            SaveManager.SaveLevelManual(position);
        }
        else
        {
            // Log a warning if the player reference is missing
            Debug.LogWarning("Player not assigned in ManualSaveButton.");
        }
    }
}
