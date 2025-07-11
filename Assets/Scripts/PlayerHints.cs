using UnityEngine;
using TMPro;

/// <summary>
/// This script manages hint text UI elements for the player,
/// such as "Press up arrow to Climb" or "Press down arrow to Crouch",
/// based on whether the player is inside relevant trigger areas.
/// </summary>
public class PlayerHints : MonoBehaviour
{
    // UI text displayed when the player is near a ladder
    public TextMeshProUGUI climbText;

    // UI text displayed when the player is near a jumpable object or area
    public TextMeshProUGUI jumpText;

    // UI text displayed when the player is in an area where crouching is relevant
    public TextMeshProUGUI crouchText;

    // These counters track how many triggers of each type the player is currently inside.
    // This ensures that the hint is only hidden when the player leaves *all* relevant triggers.
    private int ladderTriggerCount = 0;
    private int jumpTriggerCount = 0;
    private int crouchTriggerCount = 0;

    /// <summary>
    /// On game start, hide all hint texts to ensure the screen is clean
    /// until the player enters relevant trigger zones.
    /// </summary>
    private void Start()
    {
        // Hide climb hint if it's assigned
        if (climbText != null)
            climbText.gameObject.SetActive(false);

        // Hide jump hint if it's assigned
        if (jumpText != null)
            jumpText.gameObject.SetActive(false);

        // Hide crouch hint if it's assigned
        if (crouchText != null)
            crouchText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called automatically by Unity when the player enters a trigger collider.
    /// It checks what type of trigger was entered and displays the appropriate hint.
    /// </summary>
    /// <param name="other">The collider the player has entered.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the trigger has the tag "Ladder"
        if (other.CompareTag("Ladder"))
        {
            // Increment the count of ladder triggers
            ladderTriggerCount++;

            // Show the climb text if assigned
            if (climbText != null)
                climbText.gameObject.SetActive(true);
        }

        // Check if the trigger has the tag "Jump"
        if (other.CompareTag("Jump"))
        {
            // Increment the count of jump triggers
            jumpTriggerCount++;

            // Show the jump text if assigned
            if (jumpText != null)
                jumpText.gameObject.SetActive(true);
        }

        // Check if the trigger has the tag "Crouch"
        if (other.CompareTag("Crouch"))
        {
            // Increment the count of crouch triggers
            crouchTriggerCount++;

            // Show the crouch text if assigned
            if (crouchText != null)
                crouchText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Called automatically by Unity when the player exits a trigger collider.
    /// It decreases the count and hides the hint if the player is no longer in any such area.
    /// </summary>
    /// <param name="other">The collider the player has exited.</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        // Handle exit from "Ladder" area
        if (other.CompareTag("Ladder"))
        {
            // Decrease ladder trigger count
            ladderTriggerCount--;

            // Prevent negative count and hide hint if count is zero
            if (ladderTriggerCount <= 0 && climbText != null)
            {
                ladderTriggerCount = 0;
                climbText.gameObject.SetActive(false);
            }
        }

        // Handle exit from "Jump" area
        if (other.CompareTag("Jump"))
        {
            // Decrease jump trigger count
            jumpTriggerCount--;

            // Prevent negative count and hide hint if count is zero
            if (jumpTriggerCount <= 0 && jumpText != null)
            {
                jumpTriggerCount = 0;
                jumpText.gameObject.SetActive(false);
            }
        }

        // Handle exit from "Crouch" area
        if (other.CompareTag("Crouch"))
        {
            // Decrease crouch trigger count
            crouchTriggerCount--;

            // Prevent negative count and hide hint if count is zero
            if (crouchTriggerCount <= 0 && crouchText != null)
            {
                crouchTriggerCount = 0;
                crouchText.gameObject.SetActive(false);
            }
        }
    }
}
