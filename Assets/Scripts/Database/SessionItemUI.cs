using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// Handles the UI logic for a single session item in a list,
/// including click/double-click detection and selection toggling.
/// </summary>
public class SessionItemUI : MonoBehaviour, IPointerClickHandler
{
    public Button sessionNameButton;          // Button displaying the session name
    public Toggle selectionToggle;            // Toggle used to select this session item

    public int SessionId { get; private set; } // Unique ID of the session this item represents

    private SessionListController parentListController; // Reference to the parent controller that manages this list

    private float lastClickTime = 0f;         // Time of last click for double-click detection
    private const float doubleClickThreshold = 0.3f; // Max time interval (in seconds) to count as double-click

    public Action<int> OnSessionDoubleClick;  // Callback invoked when item is double-clicked

    /// <summary>
    /// Initializes the session item UI with given ID and name.
    /// </summary>
    public void Setup(int sessionId, string sessionName)
    {
        SessionId = sessionId; // Store session ID

        TextMeshProUGUI text = sessionNameButton.GetComponentInChildren<TextMeshProUGUI>(); // Get text component inside button
        if (text != null)
            text.text = sessionName; // Set session name as button label

        if (selectionToggle != null)
        {
            selectionToggle.isOn = false; // Deselect by default
            selectionToggle.gameObject.SetActive(false); // Hide toggle initially
            selectionToggle.onValueChanged.RemoveAllListeners(); // Remove previous listeners
            selectionToggle.onValueChanged.AddListener(OnToggleValueChanged); // Register toggle handler
        }

        if (sessionNameButton != null)
        {
            sessionNameButton.onClick.RemoveAllListeners(); // Clear previous button clicks
            sessionNameButton.onClick.AddListener(OnSessionTriggerClicked); // Register click handler
        }
    }

    /// <summary>
    /// Called when the session name button is clicked.
    /// Toggles the selection and simulates a pointer click event.
    /// </summary>
    private void OnSessionTriggerClicked()
    {
        if (selectionToggle != null)
        {
            selectionToggle.isOn = !selectionToggle.isOn; // Toggle selection state
            OnToggleValueChanged(selectionToggle.isOn); // Trigger change handler
        }

        // Simulate a pointer click so double-click works via button click too
        ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }

    /// <summary>
    /// Called when the toggle value is changed.
    /// Notifies the parent list controller of the change.
    /// </summary>
    private void OnToggleValueChanged(bool isOn)
    {
        parentListController?.HandleSessionToggleChanged(); // Inform parent of change
    }

    /// <summary>
    /// Returns true if this session item is currently selected.
    /// </summary>
    public bool IsSelected()
    {
        return selectionToggle != null && selectionToggle.isOn;
    }

    /// <summary>
    /// Shows or hides the selection toggle.
    /// </summary>
    public void SetSelectionVisible(bool visible)
    {
        if (selectionToggle != null)
            selectionToggle.gameObject.SetActive(visible); // Show/hide toggle
    }

    /// <summary>
    /// Sets the selected state of the session item.
    /// </summary>
    public void SetSelected(bool state)
    {
        if (selectionToggle != null)
            selectionToggle.isOn = state; // Set toggle state directly
    }

    /// <summary>
    /// Detects double-clicks using time between clicks.
    /// Invokes double-click event if detected.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= doubleClickThreshold)
        {
            Debug.Log("Double click detected on session ID: " + SessionId);
            OnSessionDoubleClick?.Invoke(SessionId); // Trigger double-click callback
        }

        lastClickTime = Time.time; // Update click time
    }

    /// <summary>
    /// Sets a reference to the parent list controller managing this UI item.
    /// </summary>
    public void SetParentListController(SessionListController controller)
    {
        parentListController = controller;
    }
}
