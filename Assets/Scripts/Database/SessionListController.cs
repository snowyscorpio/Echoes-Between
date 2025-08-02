using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the session list UI, including adding, deleting, selecting, and loading sessions.
/// Handles validation (disk space, max count), popup coordination, and session persistence.
/// </summary>
public class SessionListController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject sessionItemPrefab; // Prefab used to instantiate each session entry
    public Transform contentPanel; // Parent transform for list items
    public Button addButton; // Button to open add session popup
    public Button deleteButton; // Button for delete action (interactable controlled)
    public Button selectButton; // Button to toggle selection mode

    [Header("Popup Controllers")]
    public AddSessionPopupController addSessionPopupController; // Controller for adding sessions
    public DeleteSessionPopupController deleteSessionPopupController; // Controller for delete confirmation

    [Header("Other Controllers")]
    public DeleteSessionButtonController deleteSessionButtonController; // Controls delete button enable/disable

    [Header("Session Limit UI")]
    public TextMeshProUGUI sessionCountText; // Displays current session count / limit

    [Header("Error Display")]
    public TextMeshProUGUI errorText; // Shows errors to user

    [Header("No Space Popup")]
    public GameObject noSpacePopup; // Shown when disk space is insufficient

    private const int maxSessions = 50; // Maximum number of sessions allowed
    private List<SessionItemUI> sessionItems = new List<SessionItemUI>(); // Cached UI entries
    private bool selectionMode = false; // Whether multi-select mode is active

    /// <summary>
    /// Unity start lifecycle: verifies dependencies, checks disk space, refreshes list, and wires up buttons.
    /// </summary>
    void Start()
    {
        if (DatabaseManager.Instance == null || GameManager.Instance == null)
        {
            Debug.LogError("Missing managers"); // Required managers are not present
            return;
        }

        if (!SystemSpaceChecker.HasEnoughDiskSpace())
        {
            if (noSpacePopup != null)
                noSpacePopup.SetActive(true); // Warn about disk space constraints early
        }

        RefreshSessionList(); // Populate the UI list

        if (addButton != null)
            addButton.onClick.AddListener(() => addSessionPopupController.ShowAddSessionPopup()); // Show add dialog

        if (selectButton != null)
            selectButton.onClick.AddListener(ToggleSelectionMode); // Toggle selection mode

        if (deleteButton != null)
            deleteButton.interactable = false; // Initialize delete as disabled until selection
    }

    /// <summary>
    /// Clears and repopulates the session list UI from the database.
    /// </summary>
    public void RefreshSessionList()
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError(" Cannot refresh: DatabaseManager is null.");
            return;
        }

        // Remove existing UI entries
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        sessionItems.Clear(); // Clear cached list

        DataTable sessions = DatabaseManager.Instance.GetAllSessions(); // Fetch current sessions

        // Iterate in reverse so newest appears first if that's the intended order
        for (int i = sessions.Rows.Count - 1; i >= 0; i--)
        {
            DataRow row = sessions.Rows[i];
            GameObject itemObj = Instantiate(sessionItemPrefab, contentPanel); // Instantiate UI element
            SessionItemUI itemUI = itemObj.GetComponent<SessionItemUI>();
            itemUI.Setup((int)(long)row["sessionID"], row["sessionName"].ToString()); // Initialize with data
            itemUI.SetParentListController(this); // Provide back-reference
            itemUI.OnSessionDoubleClick = LoadSession; // Double-click loads session
            itemUI.SetSelectionVisible(selectionMode); // Show toggle if in selection mode
            sessionItems.Add(itemUI); // Keep reference for selection logic
        }

        if (sessionCountText != null)
        {
            sessionCountText.text = $"Sessions: {sessions.Rows.Count}/{maxSessions}"; // Update count display
        }

        UpdateDeleteButtonState(); // Reflect current selection state
    }

    /// <summary>
    /// Adds a new session using the name provided from the popup, performing validations.
    /// </summary>
    public void AddSessionFromPopup(string sessionName)
    {
        if (!SystemSpaceChecker.HasEnoughDiskSpace())
        {
            if (noSpacePopup != null)
                noSpacePopup.SetActive(true); // Show no-space warning
            return;
        }

        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("Cannot add session: DatabaseManager is null.");
            if (errorText != null)
                errorText.text = "Database error. Try again later."; // Surface error
            return;
        }

        int currentSessionCount = DatabaseManager.Instance.GetAllSessions().Rows.Count;

        if (currentSessionCount >= maxSessions)
        {
            Debug.LogWarning("Cannot add more sessions. Maximum limit reached.");
            if (errorText != null)
                errorText.text = $"Cannot add session limit of {maxSessions} reached."; // Inform user limit hit
            return;
        }

        DatabaseManager.Instance.AddSession(sessionName); // Create new session record
        RefreshSessionList(); // Refresh UI

        if (errorText != null)
            errorText.text = ""; // Clear any previous error
    }

    /// <summary>
    /// Deletes all currently selected sessions.
    /// </summary>
    public void DeleteSelectedSessions()
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError(" Cannot delete sessions: DatabaseManager is null.");
            return;
        }

        // Iterate through items and delete those marked selected
        foreach (var item in sessionItems)
        {
            if (item.IsSelected())
            {
                DatabaseManager.Instance.DeleteSession(item.SessionId);
            }
        }

        RefreshSessionList(); // Update after deletion
    }

    /// <summary>
    /// Returns whether any session items are currently selected.
    /// </summary>
    public bool HasSelectedSessions()
    {
        return sessionItems.Exists(item => item.IsSelected()); // Check selection state
    }

    /// <summary>
    /// Exposes the internal list of session UI items.
    /// </summary>
    public List<SessionItemUI> GetAllSessionItems()
    {
        return sessionItems;
    }

    /// <summary>
    /// Toggles selection mode on/off, controls visibility of selection toggles, and resets selection when turning off.
    /// </summary>
    private void ToggleSelectionMode()
    {
        selectionMode = !selectionMode; // Flip mode

        foreach (var item in sessionItems)
        {
            item.SetSelectionVisible(selectionMode); // Show/hide toggle
            if (!selectionMode)
                item.SetSelected(false); // Clear selection when leaving mode
        }

        UpdateDeleteButtonState(); // Update delete button enabled state
    }

    /// <summary>
    /// Called when any individual session's toggle changes; updates overall delete button state.
    /// </summary>
    public void HandleSessionToggleChanged()
    {
        UpdateDeleteButtonState();
    }

    /// <summary>
    /// Enables or disables the delete button based on selection mode and selected items.
    /// </summary>
    private void UpdateDeleteButtonState()
    {
        bool shouldEnable = selectionMode && HasSelectedSessions(); // Only active if in selection mode and something is selected
        if (deleteSessionButtonController != null)
        {
            deleteSessionButtonController.SetButtonEnabled(shouldEnable); // Delegate visual enable/disable
        }
    }

    /// <summary>
    /// Called when a session item is explicitly selected (e.g., via UI trigger) to refresh delete control state.
    /// </summary>
    public void OnSessionTriggerSelected(int sessionId)
    {
        UpdateDeleteButtonState();
    }

    /// <summary>
    /// Loads the given session: validates prerequisites, sets up GameManager state, and transitions to the appropriate scene.
    /// </summary>
    void LoadSession(int sessionId)
    {
        Debug.Log($"[LoadSession] Loading session {sessionId}");

        if (!SystemSpaceChecker.HasEnoughDiskSpace())
        {
            if (noSpacePopup != null)
                noSpacePopup.SetActive(true); // Show warning if low disk space
            return;
        }

        if (DatabaseManager.Instance == null || GameManager.Instance == null)
        {
            Debug.LogError("Cannot load session: missing DatabaseManager or GameManager");
            SessionErrorPopupController.Show("Database is unavailable."); // Inform user
            return;
        }

        GameManager.Instance.CurrentSessionID = sessionId; // Set active session for GameManager

        try
        {
            var sessionData = DatabaseManager.Instance.LoadSavedSessionData(sessionId); // Attempt to load saved data

            if (sessionData.HasValue)
            {
                // Apply retrieved session data (position + difficulty) without immediate scene load
                GameManager.Instance.ApplyLoadedSessionData(sessionData.Value.position, sessionData.Value.levelDifficulty);
                Debug.Log("[LoadSession] Loaded saved data. Going to Level_" + sessionData.Value.levelDifficulty);
                LoadingManager.SceneToLoad = "Level_" + sessionData.Value.levelDifficulty; // Prepare appropriate level
            }
            else
            {
                Debug.Log("[LoadSession] No saved data found. Starting from Cutscene.");
                LoadingManager.SceneToLoad = "Cutscene"; // Fallback to cutscene if none
            }

            SceneManager.LoadScene("Loading"); // Transition via loading screen
        }

        catch (Exception ex)
        {
            Debug.LogError("[LoadSession] Error loading session: " + ex.Message);

            // Provide user-friendly error based on exception contents
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
