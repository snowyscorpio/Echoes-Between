using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;
    public static SaveManager Instance => instance;

    private bool hasAutoSavedThisLevel = false;
    private bool hasSavedAtFixedSpawn = false;

    private readonly Vector2 fixedSpawnPosition = new Vector2(-4.75f, -2.04f);
    private const float autoSaveRange = 0.05f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.StartsWith("Level_"))
        {
            hasAutoSavedThisLevel = false;
            hasSavedAtFixedSpawn = false;
            GameManager.Instance.SetLevelDifficultyFromScene();
            StartCoroutine(DelayedAutoSave());
        }
    }

    private IEnumerator DelayedAutoSave()
    {
        yield return new WaitForSeconds(0.5f);
        TryAutoSaveOnce();
    }

    private void TryAutoSaveOnce()
    {
        if (hasAutoSavedThisLevel)
        {
            Debug.Log("Already auto-saved this level, skipping.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 currentPos = player.transform.position;
            float distance = Vector2.Distance(currentPos, fixedSpawnPosition);
            if (distance <= autoSaveRange)
            {
                SaveLevelAuto(fixedSpawnPosition);
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

    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || GameManager.Instance == null) return;

        Vector2 currentPosition = player.transform.position;
        float distance = Vector2.Distance(currentPosition, fixedSpawnPosition);

        if (distance <= autoSaveRange && !hasSavedAtFixedSpawn)
        {
            SaveLevelAuto(fixedSpawnPosition);
            hasSavedAtFixedSpawn = true;
            Debug.Log("[SaveManager] Player at fixed spawn during gameplay. Saving...");
        }
        else if (distance > autoSaveRange)
        {
            hasSavedAtFixedSpawn = false;
        }
    }

    public static void SaveLevelAuto(Vector2 position)
    {
        SaveLevel(position, false);
    }

    public static void SaveLevelManual(Vector2 position)
    {
        SaveLevel(position, true);
    }

    private static void SaveLevel(Vector2 position, bool isManual)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance is null. Cannot save.");
            return;
        }

        try
        {
            GameManager.Instance.SetLevelDifficultyFromScene();
            int sessionId = GameManager.Instance.CurrentSessionID;
            int difficulty = GameManager.Instance.LevelDifficulty;

            if (sessionId <= 0)
            {
                Debug.LogWarning("Invalid session ID: " + sessionId + ". Cannot save.");
                return;
            }

            string positionStr = position.x.ToString("F2") + "," + position.y.ToString("F2");
            int hasSeen = GameManager.Instance.HasSeenDialogue ? 1 : 0;

            using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
            {
                IDbCommand checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Levels WHERE sessionID = @sessionId";
                checkCmd.Parameters.Add(CreateParam(checkCmd, "@sessionId", sessionId));

                int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (existingCount > 0)
                {
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
            Debug.LogError("[SaveManager] Error saving level: " + e.Message);
            SessionErrorPopupController.Show("Storage error. Delete sessions?");

        }
    }


    private static IDbDataParameter CreateParam(IDbCommand command, string name, object value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        return param;
    }
}