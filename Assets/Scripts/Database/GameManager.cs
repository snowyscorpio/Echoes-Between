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
        Debug.Log("GameManager: Awake");

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager initialized");
        }
        else if (instance != this)
        {
            Debug.LogWarning("Duplicate GameManager destroyed");
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

    public void LoadNextLevel()
    {
        if (CurrentLevelID >= 3)
        {
            SceneManager.LoadScene("LastLevel");
        }
        else
        {
            int nextLevel = CurrentLevelID + 1;
            CurrentLevelID = nextLevel;
            SceneManager.LoadScene("Level_" + nextLevel);
        }
    }
}
