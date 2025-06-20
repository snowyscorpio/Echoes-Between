using UnityEngine;
using System.Data;
using System.Data.SQLite;
using System;
using System.IO;

public struct SessionData
{
    public Vector2 position;
    public int levelDifficulty;
}

public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager instance;
    private string dbPath;

    public static DatabaseManager Instance
    {
        get { return instance; }
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

    public SQLiteConnection GetConnection()
    {
        SQLiteConnection connection = new SQLiteConnection(dbPath);
        connection.Open();
        Debug.Log("Database connection opened.");
        return connection;
    }

    public SessionData? LoadSavedSessionData(int sessionId)
    {
        Debug.Log("Loading saved session data for session ID: " + sessionId);
        using (var connection = GetConnection())
        {
            // SQL query to get the last saved level position and difficulty for given session ID
            string query = "SELECT positionInLevel, levelDifficulty FROM levels WHERE sessionId = @id ORDER BY levelID DESC LIMIT 1";


            using (var cmd = new SQLiteCommand(query, connection))
            {
                // Bind session ID parameter to SQL command to prevent SQL injection
                cmd.Parameters.AddWithValue("@id", sessionId);

                using (var reader = cmd.ExecuteReader())
                {
                    // Check if the query returned a row
                    if (reader.Read())
                    {
                        // Split the position string (e.g., '12.34,56.78') into x and y coordinates
                        string[] posParts = reader.GetString(0).Split(',');
                        // Validate that the split array has exactly 2 parts (x and y)
                        if (posParts.Length == 2 &&
                            // Convert x part to float
                            float.TryParse(posParts[0], out float x) &&
                            // Convert y part to float
                            float.TryParse(posParts[1], out float y))
                        {
                            // Read level difficulty from second column in result
                            int level = reader.GetInt32(1);
                            // Return the session data with parsed position and level difficulty
                            return new SessionData
                            {
                                position = new Vector2(x, y),
                                levelDifficulty = level
                            };
                        }

                    }
                }
            }
        }
        return null;
    }

    public DataTable GetAllSessions()
    {
        Debug.Log("Fetching all sessions from database...");
        using (IDbConnection connection = GetConnection())
        {
            // Create command object to hold SQL query
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Sessions";
            // Execute the query and obtain a reader to fetch results
            IDataReader reader = command.ExecuteReader();

            DataTable table = new DataTable();
            // Load all result rows into a DataTable for use in UI or logic
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
            // Create command object to hold SQL query
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Sessions (sessionName, dateOfLastSave) VALUES (@name, datetime('now'))";

            // Create a parameter to bind the session name into the SQL insert query
            var nameParam = command.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.Value = name;
            command.Parameters.Add(nameParam);

            // Execute the insert/update/delete command that does not return results
            command.ExecuteNonQuery();
            Debug.Log("Session added to database.");
        }
    }


    // Deletes a session and its related level saves from the database.
    public void DeleteSession(int sessionId)
    {
        Debug.Log("Deleting session ID: " + sessionId);

        // Open a connection to the database
        using (IDbConnection connection = GetConnection())
        {
            //Delete related level saves

            using (IDbCommand deleteLevelsCommand = connection.CreateCommand())
            {
                deleteLevelsCommand.CommandText = "DELETE FROM Levels WHERE sessionID = @id";

                var levelsIdParam = deleteLevelsCommand.CreateParameter();
                levelsIdParam.ParameterName = "@id";
                levelsIdParam.Value = sessionId;
                deleteLevelsCommand.Parameters.Add(levelsIdParam);

                deleteLevelsCommand.ExecuteNonQuery();
                Debug.Log("Related levels deleted.");
            }

            //Delete the session itself

            using (IDbCommand deleteSessionCommand = connection.CreateCommand())
            {
                deleteSessionCommand.CommandText = "DELETE FROM Sessions WHERE sessionID = @id";

                var sessionIdParam = deleteSessionCommand.CreateParameter();
                sessionIdParam.ParameterName = "@id";
                sessionIdParam.Value = sessionId;
                deleteSessionCommand.Parameters.Add(sessionIdParam);

                deleteSessionCommand.ExecuteNonQuery();
                Debug.Log("Session deleted.");
            }
        }
    }



    public void UpdateSession(int sessionId, string newName)
    {
        Debug.Log("Updating session ID " + sessionId + " with new name: " + newName);
        using (IDbConnection connection = GetConnection())
        {
            // Create command object to hold SQL query
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE Sessions SET sessionName = @name WHERE sessionID = @id";

            // Create a parameter to bind the session name into the SQL insert query
            var nameParam = command.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.Value = newName;
            command.Parameters.Add(nameParam);

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = sessionId;
            command.Parameters.Add(idParam);

            // Execute the insert/update/delete command that does not return results
            command.ExecuteNonQuery();
            Debug.Log("Session name updated in database.");
        }
    }

    public void SaveSettings(string resolution, string graphics, int volume)
    {
        Debug.Log("Saving settings: Resolution=" + resolution + ", Graphics=" + graphics + ", Volume=" + volume);
        using (IDbConnection connection = GetConnection())
        {
            // Create command object to hold SQL query
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

            // Execute the insert/update/delete command that does not return results
            command.ExecuteNonQuery();
            Debug.Log("Settings saved to database.");
        }
    }

    public (string resolution, string graphics, int volume)? LoadSettings()
    {
        Debug.Log("Loading settings from database...");
        using (IDbConnection connection = GetConnection())
        {
            // Create command object to hold SQL query
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT resolution, graphics, volume FROM Settings LIMIT 1";

            using (IDataReader reader = command.ExecuteReader())
            {
                // Check if the query returned a row
                if (reader.Read())
                {
                    string resolution = reader.GetString(0);
                    string graphics = reader.GetString(1);
                    int volume = reader.GetInt32(2);

                    Debug.Log("Settings loaded: Resolution=" + resolution + ", Graphics=" + graphics + ", Volume=" + volume);
                    // Return the loaded settings as a tuple
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