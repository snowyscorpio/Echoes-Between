using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InterfaceSettingsPopupController : MonoBehaviour
{
    [Header("Popup UI")]
    public GameObject confirmationPopup;
    public TMP_Text confirmationText;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Error Popup")]
    public GameObject errorPopup;
    public TMP_Text errorText;

    [Header("Target Script")]
    public OptionMenu optionMenu;

    private void Start()
    {
        confirmationPopup.SetActive(false);
        if (errorPopup != null) errorPopup.SetActive(false);

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

    private void ShowErrorPopup(string message)
    {
        if (errorPopup != null && errorText != null)
        {
            StartCoroutine(ShowErrorPopupWithCountdown(message, 4));
        }
    }
    private IEnumerator ShowErrorPopupWithCountdown(string message, int seconds)
    {
        errorPopup.SetActive(true);

        for (int i = seconds; i > 0; i--)
        {
            errorText.text = $"{message}\nRetrying in {i}...";
            yield return new WaitForSeconds(1f);
        }

        errorPopup.SetActive(false);
    }


    private IEnumerator HideErrorPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        errorPopup.SetActive(false);
    }

    private void OnConfirm()
    {
        if (optionMenu != null)
        {
            try
            {
                optionMenu.SaveSettingsToDB();
                optionMenu.ApplyCurrentSettings();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error saving settings: " + ex.Message);
                ShowErrorPopup("Saving failed, try again later");
            }
        }

        confirmationPopup.SetActive(false);
    }


}
