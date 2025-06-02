using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class SessionItemUI : MonoBehaviour, IPointerClickHandler
{
    public Button sessionNameButton;
    public Toggle selectionToggle;

    public int SessionId { get; private set; }

    private SessionListController parentListController;

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;

    public Action<int> OnSessionDoubleClick;

    public void Setup(int sessionId, string sessionName)
    {
        SessionId = sessionId;

        TextMeshProUGUI text = sessionNameButton.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = sessionName;

        if (selectionToggle != null)
        {
            selectionToggle.isOn = false;
            selectionToggle.gameObject.SetActive(false);
            selectionToggle.onValueChanged.RemoveAllListeners();
            selectionToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        if (sessionNameButton != null)
        {
            sessionNameButton.onClick.RemoveAllListeners();
            sessionNameButton.onClick.AddListener(OnSessionTriggerClicked);
        }
    }

    private void OnSessionTriggerClicked()
    {
        if (selectionToggle != null)
        {
            selectionToggle.isOn = !selectionToggle.isOn;
            OnToggleValueChanged(selectionToggle.isOn);
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        parentListController?.HandleSessionToggleChanged();
    }

    public bool IsSelected()
    {
        return selectionToggle != null && selectionToggle.isOn;
    }

    public void SetSelectionVisible(bool visible)
    {
        if (selectionToggle != null)
            selectionToggle.gameObject.SetActive(visible);
    }

    public void SetSelected(bool state)
    {
        if (selectionToggle != null)
            selectionToggle.isOn = state;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= doubleClickThreshold)
        {
            OnSessionDoubleClick?.Invoke(SessionId);
        }

        lastClickTime = Time.time;
    }

    public void SetParentListController(SessionListController controller)
    {
        parentListController = controller;
    }
}
