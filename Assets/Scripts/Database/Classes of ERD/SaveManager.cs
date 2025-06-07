using UnityEngine;
using System.Data;
using System.Data.SQLite;

public class SaveManager : MonoBehaviour
{
    private void Start()
    {
        AutoSaveAtStartOfLevel();
    }

    private void AutoSaveAtStartOfLevel()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 position = player.transform.position;
            SaveLevelAuto(position);
            Debug.Log("Auto saved at start of level.");
        }
        else
        {
            Debug.LogWarning("Player not found for auto-save at level start.");
        }
    }

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

        int sessionId = GameManager.Instance.CurrentSessionID;
        int difficulty = GameManager.Instance.LevelDifficulty;
        string positionStr = position.x.ToString("F2") + "," + position.y.ToString("F2");

        using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
        {
            IDbCommand checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT levelID FROM Levels WHERE sessionID = @sessionId";
            var checkParam = checkCmd.CreateParameter();
            checkParam.ParameterName = "@sessionId";
            checkParam.Value = sessionId;
            checkCmd.Parameters.Add(checkParam);

            object result = checkCmd.ExecuteScalar();

            if (result != null)
            {
                IDbCommand updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE Levels 
                    SET positionInLevel = @position, levelDifficulty = @difficulty 
                    WHERE sessionID = @sessionId";

                updateCmd.Parameters.Add(CreateParam(updateCmd, "@position", positionStr));
                updateCmd.Parameters.Add(CreateParam(updateCmd, "@difficulty", difficulty));
                updateCmd.Parameters.Add(CreateParam(updateCmd, "@sessionId", sessionId));

                updateCmd.ExecuteNonQuery();

                Debug.Log((isManual ? "Manual" : "Auto") + " save UPDATED.");
            }
            else
            {
                IDbCommand insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Levels (positionInLevel, levelDifficulty, sessionID)
                    VALUES (@position, @difficulty, @sessionId)";

                insertCmd.Parameters.Add(CreateParam(insertCmd, "@position", positionStr));
                insertCmd.Parameters.Add(CreateParam(insertCmd, "@difficulty", difficulty));
                insertCmd.Parameters.Add(CreateParam(insertCmd, "@sessionId", sessionId));

                insertCmd.ExecuteNonQuery();

                Debug.Log((isManual ? "Manual" : "Auto") + " save INSERTED.");
            }
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
