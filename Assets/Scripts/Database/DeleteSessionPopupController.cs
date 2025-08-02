using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Controls the delete session confirmation popup, wiring up buttons and invoking the deletion on confirm.
/// </summary>
public class DeleteSessionPopupController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text deleteConfirmationText; // Text showing confirmation message
    public Button confirmDeleteButton;      // Button to confirm deletion
    public Button cancelDeleteButton;       // Button to cancel and close popup

    [Header("Control Reference")]
    public SessionListController sessionListController; // Reference to parent controller managing sessions

    /// <summary>
    /// Initialization: hide popup, set up button listeners.
    /// </summary>
    void Start()
    {
        gameObject.SetActive(false); // Start hidden

        if (confirmDeleteButton != null)
            confirmDeleteButton.onClick.AddListener(OnConfirmDelete); // Hook up confirm action

        if (cancelDeleteButton != null)
            cancelDeleteButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false); // Close popup on cancel
            });
    }

    /// <summary>
    /// Displays the delete confirmation popup if there are selected sessions.
    /// </summary>
    public void ShowDeleteSessionPopup()
    {
        if (sessionListController != null && sessionListController.HasSelectedSessions())
        {
            if (deleteConfirmationText != null)
                deleteConfirmationText.text = "ARE YOU SURE YOU\nWANT TO DELETE ?"; // Set confirmation message
            gameObject.SetActive(true); // Show popup
        }
    }

    /// <summary>
    /// Called when the user confirms deletion; triggers deletion on parent and hides the popup.
    /// </summary>
    private void OnConfirmDelete()
    {
        if (sessionListController != null)
            sessionListController.DeleteSelectedSessions(); // Perform deletion

        gameObject.SetActive(false); // Hide popup after action
    }
}
