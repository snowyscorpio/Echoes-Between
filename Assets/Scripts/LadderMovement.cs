using UnityEngine;

/// <summary>
/// Handles ladder climbing logic for the player character.
/// Detects interaction with ladders and updates movement and gravity accordingly.
/// Notifies PlayerMovement script when climbing starts or ends.
/// </summary>
public class LadderMovement : MonoBehaviour
{
    private float vertical;              // Vertical input (up/down)
    private float horizontal;            // Horizontal input (left/right)
    private float speed = 8f;            // Climbing speed
    private bool isLadder;               // Whether player is within a ladder collider
    private bool isClimbing;             // Whether player is currently climbing

    [SerializeField] private Rigidbody2D rb; // Reference to Rigidbody2D for movement
    private PlayerMovement playerMovement;  // Reference to PlayerMovement to update climbing state

    /// <summary>
    /// Called on object initialization.
    /// Caches reference to PlayerMovement component.
    /// </summary>
    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    /// <summary>
    /// Handles input detection and climbing logic each frame.
    /// Activates climbing state if vertical input is detected while inside ladder area.
    /// Exits climbing if horizontal movement occurs with no vertical input.
    /// </summary>
    void Update()
    {
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");

        if (isLadder && Mathf.Abs(vertical) > 0f)
        {
            isClimbing = true;
            playerMovement.isClimbing = true;
        }
        else if (!isLadder || (isClimbing && Mathf.Abs(vertical) == 0f && Mathf.Abs(horizontal) > 0f))
        {
            isClimbing = false;
            playerMovement.isClimbing = false;
        }
    }

    /// <summary>
    /// Applies climbing movement and gravity adjustment in fixed update for consistent physics.
    /// </summary>
    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f; // Disable gravity while climbing
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, vertical * speed); // Apply vertical movement
        }
        else
        {
            rb.gravityScale = 4f; // Restore gravity when not climbing
        }
    }

    /// <summary>
    /// Triggered when entering a ladder collider.
    /// Enables ladder interaction and notifies PlayerMovement.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isLadder = true;
            if (playerMovement != null)
                playerMovement.isClimbing = true;
        }
    }

    /// <summary>
    /// Triggered when exiting a ladder collider.
    /// Disables ladder interaction and resets climbing state.
    /// </summary>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isLadder = false;
            isClimbing = false;
            if (playerMovement != null)
                playerMovement.isClimbing = false;
        }
    }
}
