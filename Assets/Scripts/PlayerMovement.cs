using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls player movement, jumping, crouching, climbing, and respawning behavior.
/// Also manages animation states and spawn position handling based on GameManager session data.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller; // Reference to the character controller script
    public Animator animator; // Reference to the animator for updating animations
    private float vertical; // Vertical input (used when climbing)
    public float runSpeed = 40f; // Movement speed multiplier

    public GameObject fallDetector; // Object that detects when the player falls off the level

    float horizontalMove = 0f; // Current horizontal input
    bool jump = false; // Whether the player should jump
    bool crouch = false; // Whether the player is crouching

    public bool isClimbing = false; // Whether the player is currently climbing

    private Vector2 levelStartPosition; // The default spawn position for the level

    void Start()
    {
        // Default start position
        levelStartPosition = new Vector2(-4.75f, -2.04f);

        // Try to load last saved position from GameManager
        if (GameManager.Instance != null && GameManager.Instance.LastSavedPositionForSession.HasValue)
        {
            transform.position = new Vector3(GameManager.Instance.LastSavedPositionForSession.Value.x,
                                             GameManager.Instance.LastSavedPositionForSession.Value.y, 0f);
            Debug.Log("[PlayerMovement] Loaded from saved session position: " + GameManager.Instance.LastSavedPositionForSession.Value);
        }
        else
        {
            // If no saved position, use default or GameManager's start position
            levelStartPosition = GameManager.Instance != null ? GameManager.Instance.GetStartPosition() : new Vector2(-4.75f, -2.04f);
            transform.position = new Vector3(levelStartPosition.x, levelStartPosition.y, 0f);
            Debug.Log("[PlayerMovement] Spawned at: " + levelStartPosition);
        }

        // Subscribe to OnLand event to update jumping animation
        controller.OnLandEvent.AddListener(OnLanding);
    }

    void Update()
    {
        // Disable movement if dialogue is active or level 4 is active
        if (DialogueManager.IsDialogueActive || GameManager.Instance.LevelDifficulty == 4)
        {
            horizontalMove = 0f;
            jump = false;
            crouch = false;
            animator.SetFloat("Speed", 0f);
            return;
        }

        // Get horizontal input
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        // Update animation based on whether climbing or walking
        if (!isClimbing)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
        }
        else
        {
            animator.SetFloat("Speed", Mathf.Abs(vertical));
        }

        // Detect jump press (only if not climbing)
        if (Input.GetButtonDown("Jump") && !isClimbing)
        {
            jump = true;
        }

        // Set "IsJumping" animation if not climbing
        if (!isClimbing)
        {
            animator.SetBool("IsJumping", !controller.IsGrounded());
        }

        // Detect crouch press
        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }

        // Update fall detector's position to stay under the player
        if (fallDetector != null)
            fallDetector.transform.position = new Vector2(transform.position.x, fallDetector.transform.position.y);
    }

    /// <summary>
    /// Called when the player lands after jumping.
    /// Updates the jumping animation state.
    /// </summary>
    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    /// <summary>
    /// Called externally to update crouching animation state.
    /// </summary>
    public void OnCrouching(bool isCrouching)
    {
        animator.SetBool("IsCrouching", isCrouching);
    }

    /// <summary>
    /// Called when the player collides with a trigger.
    /// If it's the fall detector, resets the player's position and saves it.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FallDetector"))
        {
            transform.position = new Vector3(levelStartPosition.x, levelStartPosition.y, 0f);
            SaveManager.SaveLevelAuto(levelStartPosition);
            Debug.Log("[PlayerMovement] Fell and auto-saved at spawn point: " + levelStartPosition);
        }
    }

    void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics consistency
        if (!isClimbing)
        {
            controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        }

        // Reset jump flag after applying
        jump = false;
    }

    /// <summary>
    /// Returns the player's spawn point for this level.
    /// </summary>
    public Vector2 GetSpawnPoint()
    {
        return levelStartPosition;
    }
}
