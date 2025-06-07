using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    private bool alreadySaved = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!alreadySaved && other.CompareTag("Player"))
        {
            Vector2 playerPosition = other.transform.position;
            SaveManager.SaveLevelAuto(playerPosition);
            alreadySaved = true;

            Debug.Log("Auto save at portal!");
        }
    }
}
