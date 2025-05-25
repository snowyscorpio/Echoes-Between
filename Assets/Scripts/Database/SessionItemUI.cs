using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class SessionItemUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI sessionNameText;
    public Toggle selectionToggle;

    public int SessionId { get; private set; }

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;

    public Action<int> OnSessionDoubleClick;

    public void Setup(int sessionId, string sessionName)
    {
        SessionId = sessionId;
        sessionNameText.text = sessionName;
        selectionToggle.isOn = false;
        selectionToggle.gameObject.SetActive(false); // מוסתר כברירת מחדל
    }

    public bool IsSelected()
    {
        return selectionToggle != null && selectionToggle.isOn;
    }

    public void SetToggleVisible(bool visible)
    {
        selectionToggle.gameObject.SetActive(visible);
    }

    public void SetToggleState(bool state)
    {
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
}
