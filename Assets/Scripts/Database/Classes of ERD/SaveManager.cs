using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;

public class SaveManager : MonoBehaviour
{
    public static void SaveLevelAuto(Vector2 position)
    {
        SaveLevel(position, isManual: false);
    }

    public static void SaveLevelManual(Vector2 position)
    {
        SaveLevel(position, isManual: true);
    }

    private static void SaveLevel(Vector2 position, bool isManual)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentSessionID <= 0)
        {
            Debug.LogWarning("No active session. Cannot save.");
            return;
        }

        using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Levels (positionInLevel, levelDifficulty, sessionID)
                VALUES (@position, @difficulty, @sessionId)
            ";

            IDbDataParameter posParam = command.CreateParameter();
            posParam.ParameterName = "@position";
            posParam.Value = position.x.ToString("F2") + "," + position.y.ToString("F2");
            command.Parameters.Add(posParam);

            IDbDataParameter diffParam = command.CreateParameter();
            diffParam.ParameterName = "@difficulty";
            diffParam.Value = GameManager.Instance.LevelDifficulty;
            command.Parameters.Add(diffParam);

            IDbDataParameter sessionParam = command.CreateParameter();
            sessionParam.ParameterName = "@sessionId";
            sessionParam.Value = GameManager.Instance.CurrentSessionID;
            command.Parameters.Add(sessionParam);

            command.ExecuteNonQuery();
        }

        Debug.Log((isManual ? "Manual" : "Auto") + " save completed.");
    }
}
