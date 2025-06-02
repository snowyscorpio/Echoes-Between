using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeleteSessionButtonController : MonoBehaviour
{
    public Button deleteSessionButton;
    public TextMeshProUGUI deleteButtonText;

    public DeleteSessionPopupController popupController; 

    private readonly string activeTextColor = "#FFAC66";
    private readonly string activeUnderlayColor = "#612B00";

    private readonly string disabledTextColor = "#B653A2";
    private readonly string disabledUnderlayColor = "#4D2D46";

    void Start()
    {
        SetButtonEnabled(false);

        if (deleteSessionButton != null)
        {
            deleteSessionButton.onClick.RemoveAllListeners();
            deleteSessionButton.onClick.AddListener(() =>
            {
                if (popupController != null)
                    popupController.ShowDeleteSessionPopup();
            });
        }
    }

    public void SetButtonEnabled(bool isEnabled)
    {
        deleteSessionButton.interactable = isEnabled;

        if (deleteButtonText == null || deleteButtonText.fontSharedMaterial == null)
            return;

        if (isEnabled)
        {
            deleteButtonText.color = HexToColor(activeTextColor);
            deleteButtonText.fontSharedMaterial.SetColor("_UnderlayColor", HexToColor(activeUnderlayColor));
        }
        else
        {
            deleteButtonText.color = HexToColor(disabledTextColor);
            deleteButtonText.fontSharedMaterial.SetColor("_UnderlayColor", HexToColor(disabledUnderlayColor));
        }

        deleteButtonText.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 1f);
        deleteButtonText.fontSharedMaterial.SetFloat("_UnderlayOffsetY", -1f);
        deleteButtonText.fontSharedMaterial.SetFloat("_UnderlaySoftness", 0.6f);
    }

    private Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}
