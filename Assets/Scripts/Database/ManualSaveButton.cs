using UnityEngine;

public class ManualSaveButton : MonoBehaviour
{
    public GameObject player;

    public void SaveNow()
    {
        if (player != null)
        {
            Vector2 position = player.transform.position;
            SaveManager.SaveLevelManual(position);
        }
        else
        {
            Debug.LogWarning("Player not assigned in ManualSaveButton.");
        }
    }
}
