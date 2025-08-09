
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EndlessMario : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Jump Settings")]
    public float jumpForce = 15f;
    public float jumpUpGravity = 0.47f;
    public float jumpDownGravity = 1.64f;

    [Header("Ground Check")]
    public Transform groundCheck1;
    public Transform groundCheck2;
    public LayerMask groundLayers;

    [Header("Run Settings")]
    public float baseSpeed = 6f;      // Starting speed
    public float speedIncreaseRate = 0.1f; // Speed gain per second
    public float maxSpeed = 12f;      // Maximum speed cap

    private float currentSpeed;
    private float elapsedTime;

    private float normalGravity;
    private bool isGrounded;
    private bool isJumping;
    private bool isFalling;
    private bool jumpButtonHeld;
    private bool jumpButtonReleased = true;

    private bool isDying = false;
    private float deadUpTimer = 0.25f;
    private bool inputFreezed = false;
    private int originalLayer;
    private string originalSortingLayer;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        normalGravity = rb.gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalLayer = gameObject.layer;
        if (spriteRenderer != null)
            originalSortingLayer = spriteRenderer.sortingLayerName;

        currentSpeed = baseSpeed;
        elapsedTime = 0f;
    }

    void Update()
    {
        if (isDying || inputFreezed)
        {

            if (isDying)
            {
                deadUpTimer -= Time.deltaTime;
                if (deadUpTimer > 0)
                {

                    transform.position += Vector3.up * 6f * Time.deltaTime;
                }
                else
                {

                    transform.position += Vector3.down * 12f * Time.deltaTime;
                }
            }
            return;
        }

        elapsedTime += Time.deltaTime;
        currentSpeed = baseSpeed + (elapsedTime * speedIncreaseRate);
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

        isGrounded = Physics2D.OverlapPoint(groundCheck1.position, groundLayers) ||
                     Physics2D.OverlapPoint(groundCheck2.position, groundLayers);

        isFalling = rb.linearVelocity.y < 0 && !isGrounded;

        jumpButtonHeld = Input.GetButton("Jump");
        if (Input.GetButtonUp("Jump"))
            jumpButtonReleased = true;


        float cameraBottom = Camera.main.transform.position.y - Camera.main.orthographicSize;
        if (!isDying && transform.position.y < cameraBottom - 5f)
        {
            FreezeAndDie();
        }


        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFallingNotFromJump", isFalling && !isJumping);

        animator.SetFloat("absSpeed", currentSpeed); // Use currentSpeed for animation
    }

    void FixedUpdate()
    {
        if (isDying || inputFreezed) return;

        // Constant forward movement with gradually increasing speed
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        if (isGrounded)
        {
            isJumping = false;
            rb.gravityScale = normalGravity;
        }

        if (!isJumping && isGrounded && jumpButtonHeld && jumpButtonReleased)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            jumpButtonReleased = false;
        }
        else if (isJumping)
        {
            if (rb.linearVelocity.y > 0 && jumpButtonHeld)
                rb.gravityScale = normalGravity * jumpUpGravity;
            else
                rb.gravityScale = normalGravity * jumpDownGravity;
        }
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (isDying || inputFreezed) return;


        if (other.gameObject.CompareTag("Enemy"))
        {

            FreezeAndDie();
        }

    }
    private void FreezeAndDie()
    {
        if (isDying) return;
        inputFreezed = true;
        isDying = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        animator.SetTrigger("respawn");
        gameObject.layer = LayerMask.NameToLayer("Falling to Kill Plane");
        if (spriteRenderer != null)
            spriteRenderer.sortingLayerName = "Foreground Effect";
        deadUpTimer = 0.25f;
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.MarioRespawn();
        }
    }
}
