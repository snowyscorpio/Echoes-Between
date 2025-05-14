using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Level : Session
{
    public int levelID;
    public string positionInLevel;
    public string levelDifficulty;

    public void AutoSave()
    {
        ManualSave();
    }

    public void ManualSave()
    {
        using (var conn = DatabaseManager.Instance.GetConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT OR REPLACE INTO Levels (levelID, positionInLevel, levelDifficulty, dateOfLastSave) " +
                              $"VALUES ({levelID}, '{positionInLevel}', '{levelDifficulty}', '{dateOfLastSave}')";
            cmd.ExecuteNonQuery();
        }
    }

    public void StartAndEnd()
    {
        Debug.Log($"Level Start-End: {levelDifficulty}");
    }

    public void MainCharacterMovement(GameObject player)
    {
        if (player == null)
        {
            Debug.LogWarning("Player object is missing!");
            return;
        }

        // קובע מיקום השחקן לפי המידע מהשלב
        if (!string.IsNullOrEmpty(positionInLevel))
        {
            // מצופה ש-positionInLevel יכיל לדוגמה: "3.5,1.2"
            var split = positionInLevel.Split(',');
            if (split.Length == 2 && float.TryParse(split[0], out var x) && float.TryParse(split[1], out var y))
            {
                player.transform.position = new Vector3(x, y, player.transform.position.z);
            }
            else
            {
                Debug.LogWarning("Invalid position format in Level.positionInLevel");
            }
        }

    }

    public static List<Level> LoadAllLevels()
    {
        var levels = new List<Level>();
        using (var conn = DatabaseManager.Instance.GetConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Levels";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    levels.Add(new Level
                    {
                        levelID = reader.GetInt32(0),
                        positionInLevel = reader.GetString(1),
                        levelDifficulty = reader.GetString(2),
                        dateOfLastSave = reader.GetString(3)
                    });
                }
            }
        }
        return levels;
    }
}
