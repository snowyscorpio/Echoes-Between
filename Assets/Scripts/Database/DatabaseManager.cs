using System.IO;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;

public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager instance;
    private string dbPath;

    public static DatabaseManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("DatabaseManager");
                instance = go.AddComponent<DatabaseManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        string persistentPath = Path.Combine(Application.persistentDataPath, "GameDatabase.db");

        if (!File.Exists(persistentPath))
        {
            Debug.Log("Database not found in PersistentDataPath. Copying from Resources...");
            TextAsset dbAsset = Resources.Load<TextAsset>("GameDatabase");

            if (dbAsset != null)
            {
                File.WriteAllBytes(persistentPath, dbAsset.bytes);
                Debug.Log("Database copied successfully.");
            }
            else
            {
                Debug.LogError("GameDatabase.db not found in Resources!");
                return;
            }
        }

        dbPath = "URI=file:" + persistentPath;
    }

    public IDbConnection GetConnection()
    {
        IDbConnection connection = new SqliteConnection(dbPath);
        connection.Open();
        return connection;
    }
}