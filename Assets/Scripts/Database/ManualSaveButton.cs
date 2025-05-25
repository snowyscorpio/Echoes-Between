using UnityEngine;
using TMPro;

public class ManualSaveButton : MonoBehaviour
{
    public GameObject popupPanel;
    public TMP_Text popupText;
    public float displayTime = 2f;

    private float timer;
    private bool isShowing = false;

    void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    void Update()
    {
        if (isShowing)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                popupPanel.SetActive(false);
                isShowing = false;
            }
        }
    }

    public void OnClickSave()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector2 playerPos = player.transform.position;
            SaveManager.SaveLevelManual(playerPos);
            ShowPopup("SAVED SUCCESSFULLY!");
        }
        else
        {
            Debug.LogWarning("Player object not found.");
        }
    }

    private void ShowPopup(string message)
    {
        if (popupPanel != null && popupText != null)
        {
            popupText.text = message;
            popupPanel.SetActive(true);
            timer = displayTime;
            isShowing = true;
        }
    }
}
