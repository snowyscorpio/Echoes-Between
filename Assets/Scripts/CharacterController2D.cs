using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles 2D character movement, jumping, crouching, flipping direction, and ground detection.
/// Includes events for landing and crouching state changes.
/// </summary>
public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f; // Force applied when jumping
    [Range(0, 1)][SerializeField] private float m_CrouchSpeed = .36f; // Speed multiplier while crouching
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f; // Movement smoothing factor
    [SerializeField] private bool m_AirControl = false; // Allow control while in air
    [SerializeField] private LayerMask m_WhatIsGround; // LayerMask for detecting what counts as ground
    [SerializeField] private Transform m_GroundCheck; // Position used to check if grounded
    [SerializeField] private Transform m_CeilingCheck; // Position used to check if head is hitting ceiling
    [SerializeField] private Collider2D m_CrouchDisableCollider; // Collider to disable when crouching

    const float k_GroundedRadius = .2f; // Radius for ground check
    private bool m_Grounded; // Whether the character is on the ground
    const float k_CeilingRadius = .2f; // Radius for ceiling check
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true; // Direction the character is currently facing
    private Vector3 m_Velocity = Vector3.zero; // Used for smooth movement

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent; // Event triggered when player lands

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent; // Event triggered on crouch start/end
    private bool m_wasCrouching = false; // Track crouch state from previous frame

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes Rigidbody and events if null.
    /// </summary>
    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
    }

    /// <summary>
    /// Called every physics frame to check if the player is grounded.
    /// </summary>
    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // Check if any colliders are touching the ground layer
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;

                // Trigger landing event if previously not grounded
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }

    /// <summary>
    /// Controls character movement including walking, crouching, and jumping.
    /// </summary>
    /// <param name="move">Horizontal input value</param>
    /// <param name="crouch">Whether crouch is active</param>
    /// <param name="jump">Whether jump was requested</param>
    public void Move(float move, bool crouch, bool jump)
    {
        // Prevent standing up if ceiling above
        if (!crouch)
        {
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        // Allow movement only if grounded or air control is enabled
        if (m_Grounded || m_AirControl)
        {
            // Handle crouching state
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true); // Trigger crouch event
                }

                move *= m_CrouchSpeed; // Reduce speed while crouching

                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false; // Disable collider when crouching
            }
            else
            {
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true; // Enable collider when not crouching

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false); // Trigger stand-up event
                }
            }

            // Calculate target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);

            // Smooth and apply velocity
            m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // Flip sprite direction if needed
            if (move > 0 && !m_FacingRight)
            {
                Flip(); // Face right
            }
            else if (move < 0 && m_FacingRight)
            {
                Flip(); // Face left
            }
        }

        // Handle jumping
        if (m_Grounded && jump)
        {
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce)); // Apply upward force
        }
    }

    /// <summary>
    /// Flips the character's facing direction horizontally.
    /// </summary>
    private void Flip()
    {
        m_FacingRight = !m_FacingRight; // Toggle direction

        // Invert local X scale to flip the sprite
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// Returns whether the player is currently grounded.
    /// </summary>
    public bool IsGrounded()
    {
        return m_Grounded;
    }
}
