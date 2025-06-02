using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DeleteSessionPopupController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text deleteConfirmationText;
    public Button confirmDeleteButton;
    public Button cancelDeleteButton;

    [Header("Control Reference")]
    public SessionListController sessionListController;

    void Start()
    {
        gameObject.SetActive(false);

        if (confirmDeleteButton != null)
            confirmDeleteButton.onClick.AddListener(OnConfirmDelete);

        if (cancelDeleteButton != null)
            cancelDeleteButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
    }

    public void ShowDeleteSessionPopup()
    {
        if (sessionListController != null && sessionListController.HasSelectedSessions())
        {
            if (deleteConfirmationText != null)
                deleteConfirmationText.text = "ARE YOU SURE YOU\nWANT TO DELETE ?";
            gameObject.SetActive(true);
        }
    }

    private void OnConfirmDelete()
    {
        if (sessionListController != null)
            sessionListController.DeleteSelectedSessions();

        gameObject.SetActive(false);
    }
}
