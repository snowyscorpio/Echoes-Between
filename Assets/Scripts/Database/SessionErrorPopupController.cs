using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Handles displaying an error popup panel with a countdown timer,
/// commonly used when something critical fails (e.g., database or save error).
/// </summary>
public class SessionErrorPopupController : MonoBehaviour
{
    // Reference to the popup panel GameObject (assigned in the Inspector)
    public GameObject popupPanel;

    // Reference to the TextMeshPro component showing the countdown message
    public TMP_Text countdownText;

    // Static instance for singleton-style access
    private static SessionErrorPopupController instance;

    void Awake()
    {
        // Set the singleton instance and disable the popup panel initially
        instance = this;
        popupPanel.SetActive(false);
    }

    /// <summary>
    /// Static method to trigger the error popup with a given message.
    /// It starts the countdown coroutine on the singleton instance.
    /// </summary>
    /// <param name="message">The message to show above the countdown</param>
    public static void Show(string message)
    {
        // Make sure the instance exists before trying to start the coroutine
        if (instance != null)
        {
            instance.StartCoroutine(instance.ShowCountdownPopup(message));
        }
    }

    /// <summary>
    /// Displays the popup panel with a countdown that updates every second,
    /// then hides the panel and re-enables player movement if found.
    /// </summary>
    /// <param name="message">The error message to display</param>
    private IEnumerator ShowCountdownPopup(string message)
    {
        // Make the popup panel visible
        popupPanel.SetActive(true);

        // Countdown loop: 4, 3, 2, 1
        for (int i = 4; i > 0; i--)
        {
            // Update the message with countdown
            countdownText.text = message + "\nClosing in " + i + "...";
            yield return new WaitForSeconds(1f); // Wait 1 second
        }

        // Hide the popup panel after countdown finishes
        popupPanel.SetActive(false);

        // Try to find the player GameObject and re-enable movement
        GameObject level = GameObject.FindGameObjectWithTag("Player");
        if (level != null)
        {
            level.GetComponent<PlayerMovement>().enabled = true;
        }
    }
}
