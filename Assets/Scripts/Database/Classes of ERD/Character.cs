using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Character
{
    public int characterID;
    public string characterName;
    public string characterAppearance;
    public int levelID; // מפתח זר לטבלת Levels

    public static Character LoadCharacter(int id)
    {
        using (var conn = DatabaseManager.Instance.GetConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM Characters WHERE characterID = {id}";
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Character
                    {
                        characterID = reader.GetInt32(0),
                        characterName = reader.GetString(1),
                        characterAppearance = reader.GetString(2),
                        levelID = reader.GetInt32(3) // עמודת קישור לרמת קושי
                    };
                }
            }
        }
        return null;
    }

    public static List<Character> LoadAllCharacters()
    {
        var characters = new List<Character>();
        using (var conn = DatabaseManager.Instance.GetConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Characters";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    characters.Add(new Character
                    {
                        characterID = reader.GetInt32(0),
                        characterName = reader.GetString(1),
                        characterAppearance = reader.GetString(2),
                        levelID = reader.GetInt32(3)
                    });
                }
            }
        }
        return characters;
    }
}
