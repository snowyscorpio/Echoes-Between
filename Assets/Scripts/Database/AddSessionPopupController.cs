using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Data;

/// <summary>
/// Controls the "Add Session" popup: validation, duplicates, disk space check, and forwarding creation to parent controller.
/// </summary>
public class AddSessionPopupController : MonoBehaviour
{
    [Header("Add Session Popup")]
    public GameObject addSessionPanel;              // Panel containing the add session UI
    public TMP_InputField sessionNameInput;         // Input field for entering session name
    public Button confirmAddButton;                 // Button to confirm adding a session
    public Button cancelAddButton;                  // Button to cancel and close popup
    public TMP_Text errorText;                      // Text element for showing validation/errors

    [Header("Control Reference")]
    public SessionListController sessionListController; // Reference to parent list controller to delegate adding

    [Header("No Space Popup")]
    public GameObject noSpacePopup;                 // Popup shown when there's not enough disk space

    private const int maxSessions = 50;             // Maximum number of sessions allowed

    /// <summary>
    /// Initialization: hides UI, clears error, and wires up listeners for input and buttons.
    /// </summary>
    private void Start()
    {
        addSessionPanel.SetActive(false);            // Start with the panel hidden
        errorText.text = "";                         // Clear any initial error message

        sessionNameInput.onValueChanged.AddListener(OnInputChanged); // Validate input as user types
        confirmAddButton.onClick.AddListener(OnConfirmAdd);          // Confirm add session
        cancelAddButton.onClick.AddListener(() =>
        {
            addSessionPanel.SetActive(false);        // Close panel on cancel
            errorText.text = "";                     // Reset error message
        });
    }

    /// <summary>
    /// Displays the add session popup, disabling inputs if max session count reached.
    /// </summary>
    public void ShowAddSessionPopup()
    {
        int currentCount = DatabaseManager.Instance.GetAllSessions().Rows.Count; // Get existing session count
        if (currentCount >= maxSessions)
        {
            errorText.text = "You have reached the limit of sessions you can add."; // Inform user limit hit
            addSessionPanel.SetActive(true);
            confirmAddButton.interactable = false;  // Prevent adding more
            sessionNameInput.interactable = false;  // Disable input
        }
        else
        {
            sessionNameInput.text = "";             // Reset name field
            errorText.text = "";                    // Clear error
            sessionNameInput.interactable = true;   // Enable input
            confirmAddButton.interactable = true;   // Enable confirm
            addSessionPanel.SetActive(true);        // Show popup
        }
    }

    /// <summary>
    /// Handler for confirm button: checks disk space, validates name, checks duplicates, and delegates session creation.
    /// </summary>
    private void OnConfirmAdd()
    {
        // Early exit if disk space insufficient
        if (!SystemSpaceChecker.HasEnoughDiskSpace())
        {
            if (noSpacePopup != null)
                noSpacePopup.SetActive(true);        // Show no-space warning
            return;
        }

        string sessionName = sessionNameInput.text.Trim(); // Trim whitespace from input

        if (string.IsNullOrEmpty(sessionName))
        {
            errorText.text = "Name cannot be empty.";      // Name required
            return;
        }

        try
        {
            DataTable existingSessions = DatabaseManager.Instance.GetAllSessions(); // Load existing sessions
            foreach (DataRow row in existingSessions.Rows)
            {
                string existingName = row["sessionName"].ToString();
                if (string.Equals(existingName, sessionName, System.StringComparison.OrdinalIgnoreCase))
                {
                    // Duplicate (case-insensitive) detected
                    errorText.text = $"Session name already exists. Try adding a number (e.g., '{sessionName}2')";
                    return;
                }
            }

            errorText.text = "";      // Clear error if all good
            sessionListController.AddSessionFromPopup(sessionName); // Delegate creation
            addSessionPanel.SetActive(false);      // Close popup
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[AddSession] Failed to add session: " + ex.Message); // Log failure
            addSessionPanel.SetActive(false);        // Close popup on error
            SessionErrorPopupController.Show("Database is unavailable.\nPlease try again later."); // Inform user
        }
    }

    /// <summary>
    /// Live input change handler: filters invalid characters and enforces length/format constraints.
    /// </summary>
    private void OnInputChanged(string input)
    {
        string filtered = FilterValidCharacters(input); // Strip out disallowed characters
        if (sessionNameInput.text != filtered)
        {
            sessionNameInput.text = filtered;           // Replace with filtered value
            sessionNameInput.caretPosition = filtered.Length; // Keep caret at end
        }

        if (input != filtered)
        {
            // Notify user of invalid input characters
            errorText.text = "Name must be up to 15 letters/numbers (A-Z, a-z, 0-9), no spaces.";
        }
        else
        {
            errorText.text = "";  // Clear error when input is valid
        }
    }

    /// <summary>
    /// Removes any character that is not alphanumeric and truncates to max length (15).
    /// </summary>
    private string FilterValidCharacters(string input)
    {
        string valid = Regex.Replace(input, "[^a-zA-Z0-9]", ""); // Keep only letters and digits
        return valid.Length > 15 ? valid.Substring(0, 15) : valid; // Enforce max length
    }
}
