using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Handles confirmation and error popups for interface settings changes.
/// Triggers save/apply logic and handles error display with countdown.
/// </summary>
public class InterfaceSettingsPopupController : MonoBehaviour
{
    [Header("Popup UI")]
    public GameObject confirmationPopup;       // The confirmation popup window
    public TMP_Text confirmationText;          // Text displayed in the confirmation popup
    public Button confirmButton;               // Button to confirm changes
    public Button cancelButton;                // Button to cancel and close the popup

    [Header("Error Popup")]
    public GameObject errorPopup;              // Popup displayed when an error occurs
    public TMP_Text errorText;                 // Text area in the error popup

    [Header("Target Script")]
    public OptionMenu optionMenu;              // Reference to the OptionMenu script to save/apply settings

    // Called once on initialization
    private void Start()
    {
        confirmationPopup.SetActive(false);    // Hide confirmation popup on start
        if (errorPopup != null) errorPopup.SetActive(false); // Hide error popup on start (if assigned)

        // Add listener to confirmation button
        confirmButton.onClick.AddListener(OnConfirm);

        // Add listener to cancel button to close the confirmation popup
        cancelButton.onClick.AddListener(() =>
        {
            confirmationPopup.SetActive(false);
        });
    }

    /// <summary>
    /// Shows the confirmation popup with predefined text.
    /// </summary>
    public void ShowPopup()
    {
        if (confirmationText != null)
            confirmationText.text = "ARE YOU SURE YOU WANT TO CHANGE THE INTERFACE?";

        confirmationPopup.SetActive(true); // Display the confirmation popup
    }

    /// <summary>
    /// Shows the error popup and starts a countdown before hiding it.
    /// </summary>
    private void ShowErrorPopup(string message)
    {
        if (errorPopup != null && errorText != null)
        {
            StartCoroutine(ShowErrorPopupWithCountdown(message, 4)); // Show for 4 seconds
        }
    }

    /// <summary>
    /// Coroutine that displays an error message and counts down before hiding it.
    /// </summary>
    private IEnumerator ShowErrorPopupWithCountdown(string message, int seconds)
    {
        errorPopup.SetActive(true); // Show error popup

        for (int i = seconds; i > 0; i--)
        {
            errorText.text = $"{message}\nRetrying in {i}..."; // Update text with countdown
            yield return new WaitForSeconds(1f); // Wait 1 second
        }

        errorPopup.SetActive(false); // Hide popup after countdown
    }

    /// <summary>
    /// Optional coroutine to hide error popup after fixed delay.
    /// </summary>
    private IEnumerator HideErrorPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait specified time
        errorPopup.SetActive(false); // Then hide error popup
    }

    /// <summary>
    /// Called when the user confirms the interface change.
    /// Attempts to save and apply settings, shows error popup on failure.
    /// </summary>
    private void OnConfirm()
    {
        if (optionMenu != null)
        {
            try
            {
                optionMenu.SaveSettingsToDB();       // Save current UI settings to DB
                optionMenu.ApplyCurrentSettings();   // Apply changes to UI
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error saving settings: " + ex.Message); // Log the error
                ShowErrorPopup("Saving failed, try again later"); // Show user-facing error
            }
        }

        confirmationPopup.SetActive(false); // Close the confirmation popup
    }
}
