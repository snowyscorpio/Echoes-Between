using System;
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
                errorText.text = $"Cannot add session limit of {maxSessions} reached.";
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
        Debug.Log($"[LoadSession] Loading session {sessionId}");

        if (!SystemSpaceChecker.HasEnoughDiskSpace())
        {
            if (noSpacePopup != null)
                noSpacePopup.SetActive(true);
            return;
        }

        if (DatabaseManager.Instance == null || GameManager.Instance == null)
        {
            Debug.LogError("Cannot load session: missing DatabaseManager or GameManager");
            SessionErrorPopupController.Show("Database is unavailable.");
            return;
        }

        GameManager.Instance.CurrentSessionID = sessionId;

        try
        {
            var sessionData = DatabaseManager.Instance.LoadSavedSessionData(sessionId);

            if (sessionData.HasValue)
            {
                GameManager.Instance.ApplyLoadedSessionData(sessionData.Value.position, sessionData.Value.levelDifficulty);
                Debug.Log("[LoadSession] Loaded saved data. Going to Level_" + sessionData.Value.levelDifficulty);
                LoadingManager.SceneToLoad = "Level_" + sessionData.Value.levelDifficulty;
            }
            else
            {
                Debug.Log("[LoadSession] No saved data found. Starting from Cutscene.");
                LoadingManager.SceneToLoad = "Cutscene";
            }

            SceneManager.LoadScene("Loading");
        }

        catch (Exception ex)
        {
            Debug.LogError("[LoadSession] Error loading session: " + ex.Message);

            if (ex.Message.ToLower().Contains("database") || ex.Message.ToLower().Contains("sqlite"))
            {
                SessionErrorPopupController.Show("Database is unavailable.\nPlease try again later.");
            }
            else
            {
                SessionErrorPopupController.Show("Failed to load session.\nChoose a different session.");
            }
        }
    }

}