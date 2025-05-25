using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int CurrentSessionID { get; set; }
    public int CurrentLevelID { get; set; }
    public int LevelDifficulty { get; set; }
    public string PendingStartPosition { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector2 GetStartPosition()
    {
        if (string.IsNullOrEmpty(PendingStartPosition)) return Vector2.zero;

        string[] parts = PendingStartPosition.Split(',');
        if (parts.Length == 2 &&
            float.TryParse(parts[0], out float x) &&
            float.TryParse(parts[1], out float y))
        {
            return new Vector2(x, y);
        }

        return Vector2.zero;
    }

    public void ClearPendingState()
    {
        PendingStartPosition = null;
        CurrentLevelID = 0;
        LevelDifficulty = 0;
    }
}
