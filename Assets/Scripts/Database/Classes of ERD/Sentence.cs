using System.Collections.Generic;
using System.Data;

public class Sentence
{
    public int conversationID;
    public int ProviderID;
    public int ReceiverID;
    public string sentence; // ùåðä îÎsentenceText

    public static List<Sentence> LoadByProviderID(int providerID)
    {
        var sentences = new List<Sentence>();
        using (var conn = DatabaseManager.Instance.GetConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM Sentences WHERE ProviderID = {providerID}";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    sentences.Add(new Sentence
                    {
                        conversationID = reader.GetInt32(0),
                        ProviderID = reader.GetInt32(1),
                        ReceiverID = reader.GetInt32(2),
                        sentence = reader.GetString(3) // òîåãú "sentence"
                    });
                }
            }
        }
        return sentences;
    }
}
