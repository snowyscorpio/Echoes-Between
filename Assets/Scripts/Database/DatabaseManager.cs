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

    public DataTable GetAllSessions()
    {
        using (IDbConnection connection = GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Sessions";
            IDataReader reader = command.ExecuteReader();

            DataTable table = new DataTable();
            table.Load(reader);
            return table;
        }
    }

    public void AddSession(string name)
    {
        using (IDbConnection connection = GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Sessions (sessionName, dateOfLastSave) VALUES (@name, datetime('now'))";

            var nameParam = command.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.Value = name;
            command.Parameters.Add(nameParam);

            command.ExecuteNonQuery();
        }
    }

    public void DeleteSession(int sessionId)
    {
        using (IDbConnection connection = GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Sessions WHERE sessionID = @id";

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = sessionId;
            command.Parameters.Add(idParam);

            command.ExecuteNonQuery();
        }
    }

    public void UpdateSession(int sessionId, string newName)
    {
        using (IDbConnection connection = GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE Sessions SET sessionName = @name WHERE sessionID = @id";

            var nameParam = command.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.Value = newName;
            command.Parameters.Add(nameParam);

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = sessionId;
            command.Parameters.Add(idParam);

            command.ExecuteNonQuery();
        }
    }
}
