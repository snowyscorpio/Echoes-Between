using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the behavior and appearance of the Delete Session button,
/// including enabling/disabling it and triggering a confirmation popup.
/// </summary>
public class DeleteSessionButtonController : MonoBehaviour
{
    public Button deleteSessionButton;                   // Reference to the button component
    public TextMeshProUGUI deleteButtonText;             // Reference to the button's text (TMP)

    public DeleteSessionPopupController popupController; // Reference to the popup controller for delete confirmation

    // Colors when the button is active (enabled)
    private readonly string activeTextColor = "#FFAC66";
    private readonly string activeUnderlayColor = "#612B00";

    // Colors when the button is disabled
    private readonly string disabledTextColor = "#B653A2";
    private readonly string disabledUnderlayColor = "#4D2D46";

    /// <summary>
    /// Initializes the button by disabling it and setting up the onClick listener.
    /// </summary>
    void Start()
    {
        SetButtonEnabled(false); // Disable the button initially

        if (deleteSessionButton != null)
        {
            // Remove any previous listeners and add the delete popup action
            deleteSessionButton.onClick.RemoveAllListeners();
            deleteSessionButton.onClick.AddListener(() =>
            {
                if (popupController != null)
                    popupController.ShowDeleteSessionPopup();
            });
        }
    }

    /// <summary>
    /// Enables or disables the delete button and updates its visual style accordingly.
    /// </summary>
    /// <param name="isEnabled">Whether the button should be enabled</param>
    public void SetButtonEnabled(bool isEnabled)
    {
        deleteSessionButton.interactable = isEnabled;

        if (deleteButtonText == null || deleteButtonText.fontSharedMaterial == null)
            return;

        if (isEnabled)
        {
            // Set active colors
            deleteButtonText.color = HexToColor(activeTextColor);
            deleteButtonText.fontSharedMaterial.SetColor("_UnderlayColor", HexToColor(activeUnderlayColor));
        }
        else
        {
            // Set disabled colors
            deleteButtonText.color = HexToColor(disabledTextColor);
            deleteButtonText.fontSharedMaterial.SetColor("_UnderlayColor", HexToColor(disabledUnderlayColor));
        }

        // Set common underlay effects
        deleteButtonText.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 1f);
        deleteButtonText.fontSharedMaterial.SetFloat("_UnderlayOffsetY", -1f);
        deleteButtonText.fontSharedMaterial.SetFloat("_UnderlaySoftness", 0.6f);
    }

    /// <summary>
    /// Converts a hex color string (e.g. "#FFAC66") to a Unity Color.
    /// </summary>
    /// <param name="hex">Hex color string</param>
    /// <returns>Parsed Color</returns>
    private Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}
