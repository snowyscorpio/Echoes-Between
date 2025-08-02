using UnityEngine;
using UnityEngine.SceneManagement;
using System.Data;

/// <summary>
/// GameManager controls global game state across scenes.
/// It stores and manages session data, player position, difficulty levels, and dialogue state.
/// Implements Singleton pattern to persist through scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton instance

    private int currentSessionId; // Stores the active session ID

    // Exposes session ID property for external read/write
    public int CurrentSessionID
    {
        get => currentSessionId;
        set => currentSessionId = value;
    }

    public Vector2? LastSavedPositionForSession { get; set; } // Last saved position loaded from DB
    public int LevelDifficulty { get; private set; } // The current difficulty level for the loaded level
    public string LastSceneBeforeOptions { get; set; } // Name of the scene to return to from options
    public Vector2? PendingStartPosition { get; set; } // Temporary spawn point (e.g., from DB)
    public bool HasSeenDialogue { get; set; } // Flag to skip dialogue if already shown

    void Awake()
    {
        // Ensure only one GameManager exists — Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist through scene loads
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
        }
    }

    /// <summary>
    /// Loads saved session data from the database and navigates to the corresponding level scene.
    /// </summary>
    public void LoadSession(int sessionId)
    {
        // Try to get saved position and difficulty for given session
        var sessionData = DatabaseManager.Instance.LoadSavedSessionData(sessionId);
        if (sessionData.HasValue)
        {
            LastSavedPositionForSession = sessionData.Value.position; // Set position
            LevelDifficulty = sessionData.Value.levelDifficulty; // Set difficulty
            CurrentSessionID = sessionId; // Save current session

            // Construct scene name dynamically from difficulty level
            string sceneName = $"Level_{LevelDifficulty}";
            Debug.Log($"[GameManager] Loading scene: {sceneName} for session ID: {sessionId}");

            SceneManager.LoadScene(sceneName); // Load the relevant level
        }
        else
        {
            Debug.LogError("No saved data found for session ID: " + sessionId); // Error if no data
        }
    }

    /// <summary>
    /// Returns a valid start position for the player — either saved or default.
    /// </summary>
    public Vector2 GetStartPosition()
    {
        // Return saved position if exists, else use hardcoded default spawn point
        return LastSavedPositionForSession ?? new Vector2(-4.75f, -2.04f);
    }

    /// <summary>
    /// Extracts level difficulty from the currently active scene name.
    /// Assumes scenes are named using format: Level_1, Level_2, etc.
    /// </summary>
    public void SetLevelDifficultyFromScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Check if scene starts with "Level_" and parse the number
        if (sceneName.StartsWith("Level_") && int.TryParse(sceneName.Substring(6), out int difficulty))
        {
            LevelDifficulty = difficulty;
            Debug.Log("[GameManager] LevelDifficulty set to: " + LevelDifficulty);
        }
        else
        {
            Debug.LogWarning("Scene name does not match Level_X format: " + sceneName);
        }
    }

    /// <summary>
    /// Loads the hasSeenDialogue flag for the current session and difficulty from the database.
    /// </summary>
    public void LoadDialogueFlagFromDB()
    {
        HasSeenDialogue = false; // Default value

        // Open a DB connection
        using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT hasSeenDialogue 
                FROM Levels 
                WHERE sessionID = @sessionId AND levelDifficulty = @difficulty";

            // Add sessionId parameter
            var sessionParam = cmd.CreateParameter();
            sessionParam.ParameterName = "@sessionId";
            sessionParam.Value = CurrentSessionID;
            cmd.Parameters.Add(sessionParam);

            // Add difficulty parameter
            var difficultyParam = cmd.CreateParameter();
            difficultyParam.ParameterName = "@difficulty";
            difficultyParam.Value = LevelDifficulty;
            cmd.Parameters.Add(difficultyParam);

            // Execute and read result
            using (IDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    int flag = reader.GetInt32(0); // 0 or 1
                    HasSeenDialogue = flag == 1; // Convert to bool
                }
            }
        }
    }

    /// <summary>
    /// Saves loaded session info such as player position and level difficulty.
    /// Used before scene transitions or after loading DB data.
    /// </summary>
    public void ApplyLoadedSessionData(Vector2 position, int difficulty)
    {
        LastSavedPositionForSession = position;
        PendingStartPosition = position;
        LevelDifficulty = difficulty;
    }

    /// <summary>
    /// Loads saved position and difficulty from DB but does not load a new scene.
    /// Only applies data if difficulty matches current scene.
    /// </summary>
    public void LoadSavedLevelPosition(int sessionId)
    {
        CurrentSessionID = sessionId; // Set active session

        // Attempt to load session data
        var sessionData = DatabaseManager.Instance.LoadSavedSessionData(sessionId);
        if (sessionData.HasValue)
        {
            int savedDifficulty = sessionData.Value.levelDifficulty;

            // Try to extract difficulty from scene name
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName.StartsWith("Level_") &&
                int.TryParse(currentSceneName.Substring(6), out int currentDifficulty))
            {
                if (savedDifficulty == currentDifficulty)
                {
                    // If difficulty matches, apply saved position
                    PendingStartPosition = sessionData.Value.position;
                    LastSavedPositionForSession = sessionData.Value.position;
                    LevelDifficulty = savedDifficulty;

                    Debug.Log("[GameManager] Loaded saved position for matching level: " + PendingStartPosition.Value);
                    return;
                }
                else
                {
                    // If mismatch, ignore saved data
                    Debug.LogWarning($"[GameManager] Saved data found but difficulty {savedDifficulty} does not match current scene {currentDifficulty}. Ignoring saved position.");
                }
            }
            else
            {
                Debug.LogWarning("[GameManager] Current scene name is not in 'Level_X' format: " + currentSceneName);
            }
        }

        // If no data or mismatch, reset
        PendingStartPosition = null;
        LastSavedPositionForSession = null;

        Debug.LogWarning("[GameManager] No valid saved position loaded for this level. Using default spawn.");
    }
}
