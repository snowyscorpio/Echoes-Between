using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;
    private float vertical;
    public float runSpeed = 40f;

    private Vector3 respawnPoint;
    public GameObject fallDetector;

    float horizontalMove = 0f;
    bool jump = false;
    bool crouch = false;

    public bool isClimbing = false;

    void Start()
    {
        respawnPoint = transform.position;
        controller.OnLandEvent.AddListener(OnLanding);
    }

    void Update()
    {
        if (DialogueManager.IsDialogueActive || GameManager.Instance.LevelDifficulty == 4)
        {
            horizontalMove = 0f;
            jump = false;
            crouch = false;
            animator.SetFloat("Speed", 0f);
            return;
        }

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (!isClimbing)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
        }
        else
        {
            animator.SetFloat("Speed", Mathf.Abs(vertical));
        }

        if (Input.GetButtonDown("Jump") && !isClimbing)
        {
            jump = true;
        }

        if (!isClimbing)
        {
            animator.SetBool("IsJumping", !controller.IsGrounded());
        }

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }

        fallDetector.transform.position = new Vector2(transform.position.x, fallDetector.transform.position.y);
    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    public void OnCrouching(bool isCrouching)
    {
        animator.SetBool("IsCrouching", isCrouching);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FallDetector"))
        {
            transform.position = respawnPoint;
        }
    }

    void FixedUpdate()
    {
        if (!isClimbing)
        {
            controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        }
        jump = false;
    }
}
