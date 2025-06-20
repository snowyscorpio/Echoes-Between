using UnityEngine;
using UnityEngine.SceneManagement;
using System.Data;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int currentSessionId;
    public int CurrentSessionID
    {
        get => currentSessionId;
        set => currentSessionId = value;
    }

    public Vector2? LastSavedPositionForSession { get; private set; }
    public int LevelDifficulty { get; private set; }
    public string LastSceneBeforeOptions { get; set; }
    public Vector2? PendingStartPosition { get; set; }
    public bool HasSeenDialogue { get; set; }

    // Unity's Awake is called when the GameManager object is created
    // This sets up the singleton instance and prevents destruction across scenes
    void Awake()
    {
        // If there is no existing instance, set this as the instance
        if (Instance == null)
        {
            Instance = this;
            // Prevent GameManager from being destroyed when loading a new scene
            DontDestroyOnLoad(gameObject);
        }
        // If no data was found, log an error message
        else
        {
            // Destroy duplicate GameManager instances to enforce singleton
            Destroy(gameObject);
        }
    }

    // Load session data (position and level difficulty) from the database
    public void LoadSession(int sessionId)
    {
        // Query the database for session data using the given sessionId
        var sessionData = DatabaseManager.Instance.LoadSavedSessionData(sessionId);
        // If session data was found, update internal state and load the level scene
        if (sessionData.HasValue)
        {
            // Store loaded player position
            LastSavedPositionForSession = sessionData.Value.position;
            // Store the difficulty of the loaded level
            LevelDifficulty = sessionData.Value.levelDifficulty;
            CurrentSessionID = sessionId;

            string sceneName = $"Level_{LevelDifficulty}";
            Debug.Log($"[GameManager] Loading scene: {sceneName} for session ID: {sessionId}");

            // Load the level scene based on its difficulty
            SceneManager.LoadScene(sceneName);
        }
        // If no data was found, log an error message
        else
        {
            Debug.LogError("No saved data found for session ID: " + sessionId);
        }
    }

    // Get the saved start position if available, or return a default spawn location
    public Vector2 GetStartPosition()
    {
        return LastSavedPositionForSession ?? new Vector2(-4.75f, -2.04f); // Default spawn
    }

    // Derive the difficulty level by parsing the current scene's name
    public void SetLevelDifficultyFromScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        // Check if scene name follows the format 'Level_X'
        if (sceneName.StartsWith("Level_") && int.TryParse(sceneName.Substring(6), out int difficulty))
        {
            LevelDifficulty = difficulty;
            // Log the extracted level difficulty for debugging
            Debug.Log("[GameManager] LevelDifficulty set to: " + LevelDifficulty);
        }
        // If no data was found, log an error message
        else
        {
            Debug.LogWarning("Scene name does not match Level_X format: " + sceneName);
        }
    }

    // Load the 'hasSeenDialogue' flag from the Levels table for this session and level
    public void LoadDialogueFlagFromDB()
    {
        // Default the flag to false until proven otherwise
        HasSeenDialogue = false;

        using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
        {
            IDbCommand cmd = connection.CreateCommand();
            // SQL query to get hasSeenDialogue for this session and level
            cmd.CommandText = @"
            SELECT hasSeenDialogue 
            FROM Levels 
            WHERE sessionID = @sessionId AND levelDifficulty = @difficulty";

            var sessionParam = cmd.CreateParameter();
            sessionParam.ParameterName = "@sessionId";
            sessionParam.Value = CurrentSessionID;
            // Add parameters to prevent SQL injection
            cmd.Parameters.Add(sessionParam);

            var difficultyParam = cmd.CreateParameter();
            difficultyParam.ParameterName = "@difficulty";
            difficultyParam.Value = LevelDifficulty;
            // Add parameters to prevent SQL injection
            cmd.Parameters.Add(difficultyParam);

            using (IDataReader reader = cmd.ExecuteReader())
            {
                // If a matching row is found, read and convert flag value
                if (reader.Read())
                {
                    int flag = reader.GetInt32(0);
                    // Set HasSeenDialogue to true if database value is 1
                    HasSeenDialogue = flag == 1;
                }
            }
        }
    }


    public void ApplyLoadedSessionData(Vector2 position, int difficulty)
    {
        LastSavedPositionForSession = position;
        PendingStartPosition = position;
        LevelDifficulty = difficulty;
    }


    // Retrieve and store position/difficulty for this session without loading a scene
    public void LoadSavedLevelPosition(int sessionId)
    {
        // Store the session ID in the GameManager
        CurrentSessionID = sessionId;

        // Query the database for session data using the given sessionId
        var sessionData = DatabaseManager.Instance.LoadSavedSessionData(sessionId);

        // If we found saved data in the database
        if (sessionData.HasValue)
        {
            int savedDifficulty = sessionData.Value.levelDifficulty;
            string currentSceneName = SceneManager.GetActiveScene().name;

            // Check if the current scene name matches the pattern "Level_X"
            if (currentSceneName.StartsWith("Level_") &&
                int.TryParse(currentSceneName.Substring(6), out int currentDifficulty))
            {
                // If the saved difficulty matches the current scene's difficulty
                if (savedDifficulty == currentDifficulty)
                {
                    // Use the saved position to spawn the player
                    PendingStartPosition = sessionData.Value.position;
                    LastSavedPositionForSession = sessionData.Value.position;
                    LevelDifficulty = savedDifficulty;

                    Debug.Log("[GameManager] Loaded saved position for matching level: " + PendingStartPosition.Value);
                    return;
                }
                else
                {
                    // If difficulties do not match, skip loading the saved position
                    Debug.LogWarning($"[GameManager] Saved data found but difficulty {savedDifficulty} does not match current scene {currentDifficulty}. Ignoring saved position.");
                }
            }
            else
            {
                // If scene name is not in the expected format
                Debug.LogWarning("[GameManager] Current scene name is not in 'Level_X' format: " + currentSceneName);
            }
        }

        // No valid saved data found or mismatch – reset position to null so player spawns at default spawn point
        PendingStartPosition = null;
        LastSavedPositionForSession = null;
        Debug.LogWarning("[GameManager] No valid saved position loaded for this level. Using default spawn.");
    }


}