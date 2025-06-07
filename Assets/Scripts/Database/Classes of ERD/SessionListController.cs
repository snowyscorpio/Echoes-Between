using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;
using UnityEngine.SceneManagement;

public class SessionListController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject sessionItemPrefab;
    public Transform contentPanel;
    public Button addButton;
    public Button deleteButton;
    public Button selectButton;

    [Header("Popup Controllers")]
    public AddSessionPopupController addSessionPopupController;
    public DeleteSessionPopupController deleteSessionPopupController;

    [Header("Other Controllers")]
    public DeleteSessionButtonController deleteSessionButtonController;

    [Header("Session Limit UI")]
    public TextMeshProUGUI sessionCountText;

    [Header("Error Display")]
    public TextMeshProUGUI errorText;

    [Header("No Space Popup")]
    public GameObject noSpacePopup;

    private const int maxSessions = 50;
    private List<SessionItemUI> sessionItems = new List<SessionItemUI>();
    private bool selectionMode = false;

    void Start()
    {
        if (DatabaseManager.Instance == null || GameManager.Instance == null)
        {
            Debug.LogError("Missing managers");
            return;
        }

        if (!SystemSpaceChecker.HasEnoughDiskSpace())
        {
            if (noSpacePopup != null)
                noSpacePopup.SetActive(true);
        }

        RefreshSessionList();

        if (addButton != null)
            addButton.onClick.AddListener(() => addSessionPopupController.ShowAddSessionPopup());

        if (selectButton != null)
            selectButton.onClick.AddListener(ToggleSelectionMode);

        if (deleteButton != null)
            deleteButton.interactable = false;
    }

    public void RefreshSessionList()
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError(" Cannot refresh: DatabaseManager is null.");
            return;
        }

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        sessionItems.Clear();
        DataTable sessions = DatabaseManager.Instance.GetAllSessions();

        for (int i = sessions.Rows.Count - 1; i >= 0; i--)
        {
            DataRow row = sessions.Rows[i];
            GameObject itemObj = Instantiate(sessionItemPrefab, contentPanel);
            SessionItemUI itemUI = itemObj.GetComponent<SessionItemUI>();
            itemUI.Setup((int)(long)row["sessionID"], row["sessionName"].ToString());
            itemUI.SetParentListController(this);
            itemUI.OnSessionDoubleClick = LoadSession;
            itemUI.SetSelectionVisible(selectionMode);
            sessionItems.Add(itemUI);
        }

        if (sessionCountText != null)
        {
            sessionCountText.text = $"Sessions: {sessions.Rows.Count}/{maxSessions}";
        }

        UpdateDeleteButtonState();
    }

    public void AddSessionFromPopup(string sessionName)
    {
        if (!SystemSpaceChecker.HasEnoughDiskSpace())
        {
            if (noSpacePopup != null)
                noSpacePopup.SetActive(true);
            return;
        }

        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("Cannot add session: DatabaseManager is null.");
            if (errorText != null)
                errorText.text = "Database error. Try again later.";
            return;
        }

        int currentSessionCount = DatabaseManager.Instance.GetAllSessions().Rows.Count;

        if (currentSessionCount >= maxSessions)
        {
            Debug.LogWarning("Cannot add more sessions. Maximum limit reached.");
            if (errorText != null)
                errorText.text = $"Cannot add session – limit of {maxSessions} reached.";
            return;
        }

        DatabaseManager.Instance.AddSession(sessionName);
        RefreshSessionList();

        if (errorText != null)
            errorText.text = ""; // Clear error if success
    }

    public void DeleteSelectedSessions()
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError(" Cannot delete sessions: DatabaseManager is null.");
            return;
        }

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

    private void ToggleSelectionMode()
    {
        selectionMode = !selectionMode;

        foreach (var item in sessionItems)
        {
            item.SetSelectionVisible(selectionMode);
            if (!selectionMode)
                item.SetSelected(false);
        }

        UpdateDeleteButtonState();
    }

    public void HandleSessionToggleChanged()
    {
        UpdateDeleteButtonState();
    }

    private void UpdateDeleteButtonState()
    {
        bool shouldEnable = selectionMode && HasSelectedSessions();
        if (deleteSessionButtonController != null)
        {
            deleteSessionButtonController.SetButtonEnabled(shouldEnable);
        }
    }

    public void OnSessionTriggerSelected(int sessionId)
    {
        UpdateDeleteButtonState();
    }

    void LoadSession(int sessionId)
    {
        if (!SystemSpaceChecker.HasEnoughDiskSpace())
        {
            if (noSpacePopup != null)
                noSpacePopup.SetActive(true);
            return;
        }

        if (DatabaseManager.Instance == null || GameManager.Instance == null)
        {
            Debug.LogError(" Cannot load session – missing DatabaseManager or GameManager");
            return;
        }

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
                    string position = reader.IsDBNull(1) ? "0,0" : reader.GetString(1);
                    int difficulty = reader.GetInt32(2);

                    ApplySessionToGameManager(sessionId, levelID, position, difficulty);
                    SceneManager.LoadScene("Level_" + levelID);
                }
                else
                {
                    ApplySessionToGameManager(sessionId, 1, "0,0", 1);
                    SceneManager.LoadScene("Level_1");
                }
            }
        }

        void ApplySessionToGameManager(int sid, int lid, string pos, int diff)
        {
            GameManager.Instance.CurrentSessionID = sid;
            GameManager.Instance.PendingStartPosition = pos;
            GameManager.Instance.CurrentLevelID = lid;
            GameManager.Instance.LevelDifficulty = diff;
        }
    }
}
