using UnityEngine;
using TMPro;

public class PlayerHints : MonoBehaviour
{
    public TextMeshProUGUI climbText;
    public TextMeshProUGUI jumpText;
    public TextMeshProUGUI crouchText;

    private int ladderTriggerCount = 0;
    private int jumpTriggerCount = 0;
    private int crouchTriggerCount = 0;

    private void Start()
    {
        if (climbText != null) climbText.gameObject.SetActive(false);
        if (jumpText != null) jumpText.gameObject.SetActive(false);
        if (crouchText != null) crouchText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            ladderTriggerCount++;
            if (climbText != null) climbText.gameObject.SetActive(true);
        }

        if (other.CompareTag("Jump"))
        {
            jumpTriggerCount++;
            if (jumpText != null) jumpText.gameObject.SetActive(true);
        }

        if (other.CompareTag("Crouch"))
        {
            crouchTriggerCount++;
            if (crouchText != null) crouchText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            ladderTriggerCount--;
            if (ladderTriggerCount <= 0 && climbText != null)
            {
                ladderTriggerCount = 0;
                climbText.gameObject.SetActive(false);
            }
        }

        if (other.CompareTag("Jump"))
        {
            jumpTriggerCount--;
            if (jumpTriggerCount <= 0 && jumpText != null)
            {
                jumpTriggerCount = 0;
                jumpText.gameObject.SetActive(false);
            }
        }

        if (other.CompareTag("Crouch"))
        {
            crouchTriggerCount--;
            if (crouchTriggerCount <= 0 && crouchText != null)
            {
                crouchTriggerCount = 0;
                crouchText.gameObject.SetActive(false);
            }
        }
    }
}
