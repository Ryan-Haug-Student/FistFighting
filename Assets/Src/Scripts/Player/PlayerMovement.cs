using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;


public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;


    [Header("Jumping")]
    public float jumpForce;
    public float airMultiplier;
    public int jumpCount;
    public int extraAirJumps;

    public bool canJump;

    [Header("Obj References")]
    public GameObject playerObj;
    public Transform camPos;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Misc")]
    public Transform orientation;
    public Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air
    }

    // Private vars=============================

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;


    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        if (orientation == null)
            orientation = transform.Find("Orientation");

        if (!IsOwner)
            enabled = false;
    }

    private void Update()
    {
        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        grounded = Physics.Raycast(rayOrigin, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded)
        {
            rb.linearDamping = groundDrag;
            jumpCount = 0;
        }
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    // Custom Functions ==============================================================

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < extraAirJumps && canJump)
            Jump();

    }

    private void StateHandler()
    {
        if (grounded && Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = sprintSpeed;
            state = MovementState.sprinting;
        }
        else if (grounded)
        {
            moveSpeed = walkSpeed;
            state = MovementState.walking;
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier * 10, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        jumpCount++;
        canJump = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        Invoke(nameof(ResetJump), .2f);
    }
    private void ResetJump()
    {
        canJump = true;
    }
}
