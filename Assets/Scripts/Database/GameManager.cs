using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Session currentSession;
    public Level currentLevel;
    public InterfaceSettings currentSettings;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInitialData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadInitialData()
    {
        currentSession = Session.LoadAllSessions().Count > 0 ? Session.LoadAllSessions()[0] : null;
        currentLevel = Level.LoadAllLevels().Count > 0 ? Level.LoadAllLevels()[0] : null;
        currentSettings = InterfaceSettings.LoadSettings();

        Debug.Log("Loaded: " + currentSession?.sessionsName + ", Level: " + currentLevel?.levelDifficulty);
    }
}