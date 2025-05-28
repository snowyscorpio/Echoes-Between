using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AddSessionPopupController : MonoBehaviour
{
    [Header("Add Session Popup")]
    public GameObject addSessionPanel;
    public TMP_InputField sessionNameInput;
    public Button confirmAddButton;
    public Button cancelAddButton;

    [Header("Control Reference")]
    public SessionListController sessionListController;

    void Start()
    {

        addSessionPanel.SetActive(false);


        confirmAddButton.onClick.AddListener(OnConfirmAdd);
        cancelAddButton.onClick.AddListener(() => addSessionPanel.SetActive(false));
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
}
