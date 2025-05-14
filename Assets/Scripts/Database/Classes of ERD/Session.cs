using System.Collections.Generic;
using System.Data;

public class Session
{
    public int sessionsID;
    public string sessionsName;
    public string dateOfLastSave;

    public virtual void SaveSession()
    {
        using (var conn = DatabaseManager.Instance.GetConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT OR REPLACE INTO Sessions (sessionsID, sessionsName, dateOfLastSave) " +
                              $"VALUES ({sessionsID}, '{sessionsName}', '{dateOfLastSave}')";
            cmd.ExecuteNonQuery();
        }
    }

    public virtual void LoadSession()
    {
        using (var conn = DatabaseManager.Instance.GetConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM Sessions WHERE sessionsID = {sessionsID}";
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    sessionsName = reader.GetString(1);
                    dateOfLastSave = reader.GetString(2);
                }
            }
        }
    }

    public virtual void DeleteSession()
    {
        using (var conn = DatabaseManager.Instance.GetConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"DELETE FROM Sessions WHERE sessionsID = {sessionsID}";
            cmd.ExecuteNonQuery();
        }
    }

    public static List<Session> LoadAllSessions()
    {
        var sessions = new List<Session>();
        using (var conn = DatabaseManager.Instance.GetConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Sessions";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    sessions.Add(new Session
                    {
                        sessionsID = reader.GetInt32(0),
                        sessionsName = reader.GetString(1),
                        dateOfLastSave = reader.GetString(2)
                    });
                }
            }
        }
        return sessions;
    }
}
