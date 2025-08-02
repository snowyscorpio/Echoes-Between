using UnityEngine;

/// <summary>
/// Triggers a manual save of the player's position.
/// Attach to a UI button or relevant GameObject.
/// </summary>
public class ManualSaveButton : MonoBehaviour
{
    public GameObject player; // Reference to the player GameObject

    /// <summary>
    /// Called when the user clicks the manual save button.
    /// Saves the current player position.
    /// </summary>
    public void SaveNow()
    {
        if (player != null)
        {
            Vector2 position = player.transform.position; // Get current position
            SaveManager.SaveLevelManual(position); // Save position manually
        }
        else
        {
            Debug.LogWarning("Player not assigned in ManualSaveButton."); // Missing reference warning
        }
    }
}
