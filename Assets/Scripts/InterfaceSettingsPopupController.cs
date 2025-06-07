using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterfaceSettingsPopupController : MonoBehaviour
{
    [Header("Popup UI")]
    public GameObject confirmationPopup;
    public TMP_Text confirmationText;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Target Script")]
    public OptionMenu optionMenu;

    private void Start()
    {
        confirmationPopup.SetActive(false);

        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(() =>
        {
            confirmationPopup.SetActive(false);
        });
    }

    public void ShowPopup()
    {
        if (confirmationText != null)
            confirmationText.text = "ARE YOU SURE YOU WANT TO CHANGE THE INTERFACE?";
        confirmationPopup.SetActive(true);
    }

    private void OnConfirm()
    {
        if (optionMenu != null)
        {
            optionMenu.SaveSettingsToDB();
            optionMenu.ApplyCurrentSettings();
        }

        confirmationPopup.SetActive(false);
    }
}
