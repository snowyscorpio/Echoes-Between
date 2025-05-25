using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeleteSessionButtonController : MonoBehaviour
{
    public Button deleteSessionButton;
    public TextMeshProUGUI deleteButtonText;

    void Start()
    {
        SetButtonEnabled(false); 
    }

    public void SetButtonEnabled(bool isEnabled)
    {
        deleteSessionButton.interactable = isEnabled;

        if (isEnabled)
        {
            deleteButtonText.color = HexToColor("#E89A53");

            deleteButtonText.fontSharedMaterial.SetColor("_UnderlayColor", HexToColor("#934600"));
        }
        else
        {
            deleteButtonText.color = HexToColor("#981777");

            deleteButtonText.fontSharedMaterial.SetColor("_UnderlayColor", HexToColor("#74125B"));
        }

        deleteButtonText.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 1f);
        deleteButtonText.fontSharedMaterial.SetFloat("_UnderlayOffsetY", -1f);
        deleteButtonText.fontSharedMaterial.SetFloat("_UnderlaySoftness", 0.552f);
    }

    private Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}
