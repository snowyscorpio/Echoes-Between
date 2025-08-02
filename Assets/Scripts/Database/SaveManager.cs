using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;

/// <summary>
/// Handles automatic and manual saving of level-related data (position, difficulty, dialogue flag).
/// Maintains singleton instance, listens for scene loads, and manages auto-save logic for fixed spawn.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private static SaveManager instance; // Singleton instance
    public static SaveManager Instance => instance; // Public accessor

    private bool hasAutoSavedThisLevel = false; // Tracks whether auto-save on scene load already occurred
    private bool hasSavedAtFixedSpawn = false;  // Tracks whether fixed spawn save has occurred during gameplay

    private readonly Vector2 fixedSpawnPosition = new Vector2(-4.75f, -2.04f); // Predefined fixed spawn
    private const float autoSaveRange = 0.05f; // Distance threshold for considering player at fixed spawn

    /// <summary>
    /// Unity Awake lifecycle: initialize singleton and persist across scenes.
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this; // Set this as the global instance
            DontDestroyOnLoad(gameObject); // Keep across scene transitions
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Enforce singleton uniqueness
            return;
        }
    }

    /// <summary>
    /// Subscribe to sceneLoaded event when enabled.
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Listen for new scene loads
    }

    /// <summary>
    /// Unsubscribe from sceneLoaded event when disabled.
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called when a scene is loaded. Resets state and triggers delayed auto-save if appropriate.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.StartsWith("Level_")) // Only handle level scenes
        {
            hasAutoSavedThisLevel = false; // Reset flags
            hasSavedAtFixedSpawn = false;
            GameManager.Instance.SetLevelDifficultyFromScene(); // Update difficulty from scene name
            StartCoroutine(DelayedAutoSave()); // Attempt auto-save shortly after load
        }
    }

    /// <summary>
    /// Coroutine to delay auto-save slightly to allow scene stabilization.
    /// </summary>
    private IEnumerator DelayedAutoSave()
    {
        yield return new WaitForSeconds(0.5f); // Small wait so player object likely exists
        TryAutoSaveOnce(); // Attempt auto-save based on spawn position
    }

    /// <summary>
    /// Attempts one-time auto-save if the player is at the fixed spawn position after scene load.
    /// </summary>
    private void TryAutoSaveOnce()
    {
        if (hasAutoSavedThisLevel)
        {
            Debug.Log("Already auto-saved this level, skipping.");
            return; // Avoid duplicate auto-save for same level load
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player"); // Find player
        if (player != null)
        {
            Vector2 currentPos = player.transform.position;
            float distance = Vector2.Distance(currentPos, fixedSpawnPosition);
            if (distance <= autoSaveRange) // Player is close enough to fixed spawn
            {
                SaveLevelAuto(fixedSpawnPosition); // Auto-save at fixed spawn
                hasAutoSavedThisLevel = true;
                hasSavedAtFixedSpawn = true;
                Debug.Log("[SaveManager] Auto-saved from spawn (scene load).");
            }
        }
        else
        {
            Debug.LogWarning("Player not found. Cannot auto-save.");
        }
    }

    /// <summary>
    /// Regular update loop: monitors if player remains at fixed spawn during gameplay and auto-saves if necessary.
    /// </summary>
    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // Find player object
        if (player == null || GameManager.Instance == null) return; // Early exit if missing

        Vector2 currentPosition = player.transform.position;
        float distance = Vector2.Distance(currentPosition, fixedSpawnPosition);

        if (distance <= autoSaveRange && !hasSavedAtFixedSpawn)
        {
            SaveLevelAuto(fixedSpawnPosition); // Save when player returns to fixed spawn
            hasSavedAtFixedSpawn = true;
            Debug.Log("[SaveManager] Player at fixed spawn during gameplay. Saving...");
        }
        else if (distance > autoSaveRange)
        {
            hasSavedAtFixedSpawn = false; // Reset flag if player leaves fixed spawn
        }
    }

    /// <summary>
    /// Convenience wrapper for auto-save (non-user initiated).
    /// </summary>
    /// <param name="position">Position to save</param>
    public static void SaveLevelAuto(Vector2 position)
    {
        SaveLevel(position, false);
    }

    /// <summary>
    /// Convenience wrapper for manual save (user initiated).
    /// </summary>
    /// <param name="position">Position to save</param>
    public static void SaveLevelManual(Vector2 position)
    {
        SaveLevel(position, true);
    }

    /// <summary>
    /// Core save logic; handles inserting or updating a level record with position, difficulty, and dialogue flag.
    /// </summary>
    /// <param name="position">Player position to store</param>
    /// <param name="isManual">Whether the save was manual (true) or automatic (false)</param>
    private static void SaveLevel(Vector2 position, bool isManual)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance is null. Cannot save.");
            return; // Cannot proceed without game manager context
        }

        try
        {
            GameManager.Instance.SetLevelDifficultyFromScene(); // Ensure difficulty is current based on active scene
            int sessionId = GameManager.Instance.CurrentSessionID;
            int difficulty = GameManager.Instance.LevelDifficulty;

            if (sessionId <= 0)
            {
                Debug.LogWarning("Invalid session ID: " + sessionId + ". Cannot save.");
                return; // Invalid session guard
            }

            string positionStr = position.x.ToString("F2") + "," + position.y.ToString("F2"); // Format position as string with 2 decimals
            int hasSeen = GameManager.Instance.HasSeenDialogue ? 1 : 0; // Convert flag to integer

            using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
            {
                // Check whether a record already exists for this session
                IDbCommand checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Levels WHERE sessionID = @sessionId";
                checkCmd.Parameters.Add(CreateParam(checkCmd, "@sessionId", sessionId));

                int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (existingCount > 0)
                {
                    // Update existing row with new data
                    IDbCommand updateCmd = connection.CreateCommand();
                    updateCmd.CommandText = @"
                    UPDATE Levels 
                    SET positionInLevel = @position, levelDifficulty = @difficulty, hasSeenDialogue = @hasSeen
                    WHERE sessionID = @sessionId";

                    updateCmd.Parameters.Add(CreateParam(updateCmd, "@position", positionStr));
                    updateCmd.Parameters.Add(CreateParam(updateCmd, "@difficulty", difficulty));
                    updateCmd.Parameters.Add(CreateParam(updateCmd, "@hasSeen", hasSeen));
                    updateCmd.Parameters.Add(CreateParam(updateCmd, "@sessionId", sessionId));

                    updateCmd.ExecuteNonQuery();
                    Debug.Log($"[SaveManager] {(isManual ? "Manual" : "Auto")} save UPDATED row.");
                }
                else
                {
                    // Insert new level record
                    IDbCommand insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = @"
                    INSERT INTO Levels (positionInLevel, levelDifficulty, sessionID, hasSeenDialogue)
                    VALUES (@position, @difficulty, @sessionId, @hasSeen)";

                    insertCmd.Parameters.Add(CreateParam(insertCmd, "@position", positionStr));
                    insertCmd.Parameters.Add(CreateParam(insertCmd, "@difficulty", difficulty));
                    insertCmd.Parameters.Add(CreateParam(insertCmd, "@sessionId", sessionId));
                    insertCmd.Parameters.Add(CreateParam(insertCmd, "@hasSeen", hasSeen));

                    insertCmd.ExecuteNonQuery();
                    Debug.Log($"[SaveManager] {(isManual ? "Manual" : "Auto")} save INSERTED row.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveManager] Error saving level: " + e.Message); // Log error detail
            SessionErrorPopupController.Show("Storage error. Delete sessions?"); // Notify user of storage issue
        }
    }

    /// <summary>
    /// Helper to create and configure a parameter for IDbCommands.
    /// </summary>
    /// <param name="command">Command to attach parameter to</param>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Value to assign</param>
    /// <returns>Configured parameter</returns>
    private static IDbDataParameter CreateParam(IDbCommand command, string name, object value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name; // Set parameter placeholder
        param.Value = value;        // Assign value
        return param;
    }
}
