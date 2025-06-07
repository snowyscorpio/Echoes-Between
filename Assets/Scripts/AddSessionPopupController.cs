using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Data;

public class AddSessionPopupController : MonoBehaviour
{
    [Header("Add Session Popup")]
    public GameObject addSessionPanel;
    public TMP_InputField sessionNameInput;
    public Button confirmAddButton;
    public Button cancelAddButton;
    public TMP_Text errorText;

    [Header("Control Reference")]
    public SessionListController sessionListController;

    [Header("No Space Popup")]
    public GameObject noSpacePopup;

    private const int maxSessions = 50;

    private void Start()
    {
        addSessionPanel.SetActive(false);
        errorText.text = "";

        sessionNameInput.onValueChanged.AddListener(OnInputChanged);
        confirmAddButton.onClick.AddListener(OnConfirmAdd);
        cancelAddButton.onClick.AddListener(() =>
        {
            addSessionPanel.SetActive(false);
            errorText.text = "";
        });
    }

    public void ShowAddSessionPopup()
    {
        int currentCount = DatabaseManager.Instance.GetAllSessions().Rows.Count;
        if (currentCount >= maxSessions)
        {
            errorText.text = "You have reached the limit of sessions you can add.";
            addSessionPanel.SetActive(true);
            confirmAddButton.interactable = false;
            sessionNameInput.interactable = false;
        }
        else
        {
            sessionNameInput.text = "";
            errorText.text = "";
            sessionNameInput.interactable = true;
            confirmAddButton.interactable = true;
            addSessionPanel.SetActive(true);
        }
    }

    private void OnConfirmAdd()
    {
        if (!SystemSpaceChecker.HasEnoughDiskSpace())
        {
            if (noSpacePopup != null)
                noSpacePopup.SetActive(true);
            return;
        }

        string sessionName = sessionNameInput.text.Trim();

        if (string.IsNullOrEmpty(sessionName))
        {
            errorText.text = "Name cannot be empty.";
            return;
        }

        DataTable existingSessions = DatabaseManager.Instance.GetAllSessions();
        foreach (DataRow row in existingSessions.Rows)
        {
            string existingName = row["sessionName"].ToString();
            if (string.Equals(existingName, sessionName, System.StringComparison.OrdinalIgnoreCase))
            {
                errorText.text = $"Session name already exists. Try adding a number (e.g., '{sessionName}2')";
                return;
            }
        }

        errorText.text = "";
        sessionListController.AddSessionFromPopup(sessionName);
        addSessionPanel.SetActive(false);
    }

    private void OnInputChanged(string input)
    {
        string filtered = FilterValidCharacters(input);
        if (sessionNameInput.text != filtered)
        {
            sessionNameInput.text = filtered;
            sessionNameInput.caretPosition = filtered.Length;
        }

        if (input != filtered)
        {
            errorText.text = "Name must be up to 15 letters/numbers (A-Z, a-z, 0-9), no spaces.";
        }
        else
        {
            errorText.text = "";
        }
    }

    private string FilterValidCharacters(string input)
    {
        string valid = Regex.Replace(input, "[^a-zA-Z0-9]", "");
        return valid.Length > 15 ? valid.Substring(0, 15) : valid;
    }
}
