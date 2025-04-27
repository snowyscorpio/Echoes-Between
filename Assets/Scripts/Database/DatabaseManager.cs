using System.IO;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;

public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager instance;
    private string dbPath;
    private IDbConnection dbConnection;

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
        OpenConnection();
    }

    private void OpenConnection()
    {
        dbConnection = new SqliteConnection(dbPath);
        dbConnection.Open();
        Debug.Log("Database connection opened.");
    }

    public IDbCommand CreateCommand(string query)
    {
        if (dbConnection == null)
        {
            Debug.LogError("Database connection is not open!");
            return null;
        }

        IDbCommand command = dbConnection.CreateCommand();
        command.CommandText = query;
        return command;
    }

    private void OnApplicationQuit()
    {
        if (dbConnection != null)
        {
            dbConnection.Close();
            dbConnection = null;
            Debug.Log("Database connection closed.");
        }
    }
}
