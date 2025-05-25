using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SessionPopupController : MonoBehaviour
{
    [Header("Add Session Popup")]
    public GameObject addSessionPanel;
    public TMP_InputField sessionNameInput;
    public Button confirmAddButton;
    public Button cancelAddButton;

    [Header("Delete Confirmation Popup")]
    public GameObject confirmDeletePanel;
    public Button confirmDeleteButton;
    public Button cancelDeleteButton;
    public TMP_Text deleteConfirmationText;

    [Header("Control References")]
    public SessionListController sessionListController;
    public Button deleteSessionButton;
    public Button selectSessionButton;

    private bool selectionMode = false;

    void Start()
    {
        addSessionPanel.SetActive(false);
        confirmDeletePanel.SetActive(false);

        confirmAddButton.onClick.AddListener(OnConfirmAdd);
        cancelAddButton.onClick.AddListener(() => addSessionPanel.SetActive(false));

        confirmDeleteButton.onClick.AddListener(OnConfirmDelete);
        cancelDeleteButton.onClick.AddListener(() => confirmDeletePanel.SetActive(false));

        deleteSessionButton.interactable = false;
        selectSessionButton.onClick.AddListener(ToggleSelectionMode);
        deleteSessionButton.onClick.AddListener(ShowDeleteConfirmation);
    }

    public void ShowAddSessionPopup()
    {
        sessionNameInput.text = "";
        addSessionPanel.SetActive(true);
    }

    private void OnConfirmAdd()
    {
        string sessionName = sessionNameInput.text.Trim();
        if (!string.IsNullOrEmpty(sessionName))
        {
            sessionListController.AddSessionFromPopup(sessionName);
        }
        addSessionPanel.SetActive(false);
    }

    public void ToggleSelectionMode()
    {
        selectionMode = !selectionMode;
        deleteSessionButton.interactable = selectionMode;

        foreach (var item in sessionListController.GetAllSessionItems())
        {
            item.SetToggleVisible(selectionMode);
            if (!selectionMode)
                item.SetToggleState(false);
        }
    }

    private void ShowDeleteConfirmation()
    {
        if (selectionMode && sessionListController.HasSelectedSessions())
        {
            deleteConfirmationText.text = "Are you sure you want to delete the selected session(s)?";
            confirmDeletePanel.SetActive(true);
        }
    }

    private void OnConfirmDelete()
    {
        sessionListController.DeleteSelectedSessions();
        confirmDeletePanel.SetActive(false);
    }
}
