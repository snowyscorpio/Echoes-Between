using System.Data;

public class InterfaceSettings
{
    public int savedOptionsID;
    public string graphics;
    public string resolution;
    public float volume;

    public void UpdateInterface()
    {
        // עדכון הגדרות במסד נתונים
    }

    public static InterfaceSettings LoadSettings()
    {
        using (var conn = DatabaseManager.Instance.GetConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM InterfaceSettings LIMIT 1";
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new InterfaceSettings
                    {
                        savedOptionsID = reader.GetInt32(0),
                        graphics = reader.GetString(1),
                        resolution = reader.GetString(2),
                        volume = float.Parse(reader.GetValue(3).ToString())
                    };
                }
            }
        }
        return null;
    }
}
