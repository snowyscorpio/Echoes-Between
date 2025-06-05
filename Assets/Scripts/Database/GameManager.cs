using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameManager");
                instance = go.AddComponent<GameManager>();
                DontDestroyOnLoad(go);
                Debug.Log("GameManager Instance created dynamically");
            }
            return instance;
        }
    }

    public int CurrentSessionID { get; set; }
    public int CurrentLevelID { get; set; }
    public int LevelDifficulty { get; set; }
    public string PendingStartPosition { get; set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager initialized from scene");
        }
        else if (instance != this)
        {
            Debug.LogWarning("Duplicate GameManager detected and destroyed");
            Destroy(gameObject);
        }
    }

    public Vector2 GetStartPosition()
    {
        if (string.IsNullOrEmpty(PendingStartPosition))
        {
            Debug.LogWarning("PendingStartPosition is null or empty. Defaulting to Vector2.zero");
            return Vector2.zero;
        }

        string[] parts = PendingStartPosition.Split(',');
        if (parts.Length == 2 &&
            float.TryParse(parts[0], out float x) &&
            float.TryParse(parts[1], out float y))
        {
            Vector2 result = new Vector2(x, y);
            Debug.Log($"Start position parsed: {result}");
            return result;
        }

        Debug.LogError($"Failed to parse PendingStartPosition: {PendingStartPosition}. Defaulting to Vector2.zero");
        return Vector2.zero;
    }

    public void ClearPendingState()
    {
        Debug.Log("Clearing pending state (position, level ID, difficulty)");
        PendingStartPosition = null;
        CurrentLevelID = 0;
        LevelDifficulty = 0;
    }

    public void LoadNextLevel()
    {
        if (CurrentLevelID >= 3)
        {
            Debug.Log("Loading LastLevel scene");
            SceneManager.LoadScene("LastLevel");
        }
        else
        {
            int nextLevel = CurrentLevelID + 1;
            Debug.Log($"Loading next level: Level_{nextLevel}");
            CurrentLevelID = nextLevel;
            SceneManager.LoadScene("Level_" + nextLevel);
        }
    }
}
