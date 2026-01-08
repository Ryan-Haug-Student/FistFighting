using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;


public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintSpeed;
    public bool sprinting;
    public bool canSprint;
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

        if(Input.GetKeyDown(KeyCode.Space) && jumpCount < extraAirJumps && canJump)
            Jump();

        if (Input.GetKeyDown(KeyCode.LeftControl))
            Crouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            UnCrouch();

            sprinting = Input.GetKey(KeyCode.LeftShift) && canSprint ? true : false;
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

        if (!sprinting)
        {
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
        else
        {
            if (flatVel.magnitude > sprintSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * sprintSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
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

    private void Crouch()
    {
        canSprint = false;

        transform.position = new Vector3(transform.position.x, transform.position.y - .25f, transform.position.z);
        playerObj.transform.localScale = new Vector3(1, .75f, 1);
        camPos.localPosition = Vector3.up * 1.25f;

    }
    private void UnCrouch()
    {
        canSprint = true;

        transform.position = new Vector3(transform.position.x, transform.position.y + .25f, transform.position.z);
        playerObj.transform.localScale = Vector3.one;
        camPos.localPosition = Vector3.up * 1.5f;
    }
}
