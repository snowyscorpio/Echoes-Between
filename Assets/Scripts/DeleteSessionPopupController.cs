using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DeleteSessionPopupController : MonoBehaviour
{
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
        confirmDeletePanel.SetActive(false);

        confirmDeleteButton.onClick.AddListener(OnConfirmDelete);
        cancelDeleteButton.onClick.AddListener(() => confirmDeletePanel.SetActive(false));

        deleteSessionButton.interactable = false;
        selectSessionButton.onClick.AddListener(ToggleSelectionMode);
        deleteSessionButton.onClick.AddListener(ShowDeleteConfirmation);
    }

    public void ToggleSelectionMode()
    {
        selectionMode = !selectionMode;
        deleteSessionButton.interactable = selectionMode;

        foreach (var item in sessionListController.GetAllSessionItems())
        {
            item.SetSelectionVisible(selectionMode); 
            if (!selectionMode)
                item.SetSelected(false);              
        }
    }

    public void ShowDeleteConfirmation()
    {
        if (selectionMode && sessionListController.HasSelectedSessions())
        {
            confirmDeletePanel.SetActive(true);
        }
    }

    private void OnConfirmDelete()
    {
        sessionListController.DeleteSelectedSessions();
        confirmDeletePanel.SetActive(false);
    }
}
