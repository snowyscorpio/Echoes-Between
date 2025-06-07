using System.IO;
using UnityEngine;
using System.Data;
using System.Data.SQLite;

public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager instance;
    private string dbPath;

    public static DatabaseManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, "GameDatabase.db");

        if (!File.Exists(fullPath))
        {
            Debug.LogError("GameDatabase.db not found in StreamingAssets! Path: " + fullPath);
            return;
        }

        dbPath = "Data Source=" + fullPath + ";Version=3;";
        Debug.Log("Database initialized: " + dbPath);
    }

    public IDbConnection GetConnection()
    {
        IDbConnection connection = new SQLiteConnection(dbPath);
        connection.Open();
        Debug.Log("Database connection opened.");
        return connection;
    }

    public DataTable GetAllSessions()
    {
        Debug.Log("Fetching all sessions from database...");
        using (IDbConnection connection = GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Sessions";
            IDataReader reader = command.ExecuteReader();

            DataTable table = new DataTable();
            table.Load(reader);
            Debug.Log("Loaded " + table.Rows.Count + " session(s) from database.");
            return table;
        }
    }

    public void AddSession(string name)
    {
        Debug.Log("Adding new session: " + name);
        using (IDbConnection connection = GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Sessions (sessionName, dateOfLastSave) VALUES (@name, datetime('now'))";

            var nameParam = command.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.Value = name;
            command.Parameters.Add(nameParam);

            command.ExecuteNonQuery();
            Debug.Log("Session added to database.");
        }
    }

    public void DeleteSession(int sessionId)
    {
        Debug.Log("Deleting session ID: " + sessionId);
        using (IDbConnection connection = GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Sessions WHERE sessionID = @id";

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = sessionId;
            command.Parameters.Add(idParam);

            command.ExecuteNonQuery();
            Debug.Log("Session deleted from database.");
        }
    }

    public void UpdateSession(int sessionId, string newName)
    {
        Debug.Log("Updating session ID " + sessionId + " with new name: " + newName);
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
            Debug.Log("Session name updated in database.");
        }
    }

    public void SaveSettings(string resolution, string graphics, int volume)
    {
        Debug.Log("Saving settings: Resolution=" + resolution + ", Graphics=" + graphics + ", Volume=" + volume);
        using (IDbConnection connection = GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = @"
            DELETE FROM Settings;
            INSERT INTO Settings (resolution, graphics, volume)
            VALUES (@resolution, @graphics, @volume);";

            var resolutionParam = command.CreateParameter();
            resolutionParam.ParameterName = "@resolution";
            resolutionParam.Value = resolution;
            command.Parameters.Add(resolutionParam);

            var graphicsParam = command.CreateParameter();
            graphicsParam.ParameterName = "@graphics";
            graphicsParam.Value = graphics;
            command.Parameters.Add(graphicsParam);

            var volumeParam = command.CreateParameter();
            volumeParam.ParameterName = "@volume";
            volumeParam.Value = volume;
            command.Parameters.Add(volumeParam);

            command.ExecuteNonQuery();
            Debug.Log("Settings saved to database.");
        }
    }

    public (string resolution, string graphics, int volume)? LoadSettings()
    {
        Debug.Log("Loading settings from database...");
        using (IDbConnection connection = GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT resolution, graphics, volume FROM Settings LIMIT 1";

            using (IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    string resolution = reader.GetString(0);
                    string graphics = reader.GetString(1);
                    int volume = reader.GetInt32(2);

                    Debug.Log("Settings loaded: Resolution=" + resolution + ", Graphics=" + graphics + ", Volume=" + volume);
                    return (resolution, graphics, volume);
                }
                else
                {
                    Debug.LogWarning("No settings found in database.");
                }
            }
        }
        return null;
    }
}
