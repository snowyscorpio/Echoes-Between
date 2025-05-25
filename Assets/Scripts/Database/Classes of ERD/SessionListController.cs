using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;
using UnityEngine.SceneManagement;

public class SessionListController : MonoBehaviour
{
    public GameObject sessionItemPrefab;
    public Transform contentPanel;
    public TMP_InputField inputField;
    public Button addButton;
    public Button deleteButton;
    public Button selectButton; 

    private List<SessionItemUI> sessionItems = new List<SessionItemUI>();

    void Start()
    {
        RefreshSessionList();
        addButton.onClick.AddListener(AddNewSession);
        deleteButton.onClick.AddListener(DeleteSelectedSessions);

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(() =>
            {
                FindFirstObjectByType<SessionPopupController>()?.ToggleSelectionMode();
            });
        }
    }

    public void RefreshSessionList()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        sessionItems.Clear();
        DataTable sessions = DatabaseManager.Instance.GetAllSessions();

        foreach (DataRow row in sessions.Rows)
        {
            GameObject itemObj = Instantiate(sessionItemPrefab, contentPanel);
            SessionItemUI itemUI = itemObj.GetComponent<SessionItemUI>();
            itemUI.Setup((int)(long)row["sessionID"], row["sessionName"].ToString());
            itemUI.OnSessionDoubleClick = LoadSession;
            sessionItems.Add(itemUI);
        }
    }

    public void AddNewSession()
    {
        string sessionName = inputField.text.Trim();
        if (!string.IsNullOrEmpty(sessionName))
        {
            DatabaseManager.Instance.AddSession(sessionName);
            inputField.text = "";
            RefreshSessionList();
        }
    }

    public void AddSessionFromPopup(string sessionName)
    {
        DatabaseManager.Instance.AddSession(sessionName);
        RefreshSessionList();
    }

    public void DeleteSelectedSessions()
    {
        foreach (var item in sessionItems)
        {
            if (item.IsSelected())
            {
                DatabaseManager.Instance.DeleteSession(item.SessionId);
            }
        }
        RefreshSessionList();
    }

    public bool HasSelectedSessions()
    {
        return sessionItems.Exists(item => item.IsSelected());
    }

    public List<SessionItemUI> GetAllSessionItems()
    {
        return sessionItems;
    }

    void LoadSession(int sessionId)
    {
        Debug.Log("Loading session with ID: " + sessionId);

        using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT levelID, positionInLevel, levelDifficulty FROM Levels WHERE sessionID = @id ORDER BY levelID DESC LIMIT 1";

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = sessionId;
            command.Parameters.Add(idParam);

            using (IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    int levelID = reader.GetInt32(0);
                    string position = reader.GetString(1);
                    int difficulty = reader.GetInt32(2);

                    Debug.Log($"Session {sessionId} will load level {levelID}, position {position}, difficulty {difficulty}");

                    GameManager.Instance.CurrentSessionID = sessionId;
                    GameManager.Instance.PendingStartPosition = position;
                    GameManager.Instance.CurrentLevelID = levelID;
                    GameManager.Instance.LevelDifficulty = difficulty;

                    SceneManager.LoadScene("Level_" + levelID);
                }
                else
                {
                    Debug.LogWarning("No saved level found for this session. Starting from level 1.");
                    GameManager.Instance.CurrentSessionID = sessionId;
                    GameManager.Instance.PendingStartPosition = "0,0";
                    GameManager.Instance.CurrentLevelID = 1;
                    GameManager.Instance.LevelDifficulty = 1;

                    SceneManager.LoadScene("Level_1");
                }
            }
        }
    }
}
