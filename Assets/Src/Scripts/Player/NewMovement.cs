using Unity.Netcode;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed, sprintSpeed, crouchSpeed, slideSpeed, grappleSpeed;
    public float groundDrag;
    public MS state;

    [Header("Jumping")]
    public float jumpForce;
    public float airMultiplier;
    public int jumpCount, maxJumpCount;
    public bool canJump;

    [Header("Crouching")]
    public float crouchScale;
    public float startScale;

    [Header("Sliding")]
    public float minSlideSpeed;
    public float slideStartImpulse;
    public float slideDrag;
    public bool slideStarted;

    [Header("Ground Check")]
    public bool grounded;
    public float maxSlopeAngle;
    public float playerHeight;
    public LayerMask groundMask;

    [Header("Obj Refs")]
    public Rigidbody rb;
    public Transform playerObj, orientation;

    [Header("Keybinds")]
    public bool placeHolder;
    public KeyCode jumpBind, grappleBind, crouchBind, sprintBind;

    //PRIVATE MISC =======================================================
    private RaycastHit slopeHit;
    private bool wasGrounded;

    private float horizontalInput, verticalInput;
    Vector3 moveDirection;

    public enum MS
    {
        idle, walking, sprinting, crouching, sliding, grappling, air
    }

    // Functions =========================================================

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        startScale = transform.localScale.y;
    }

    private void Update()
    {
        if (!IsOwner) return;

        grounded = Physics.Raycast(playerObj.position, Vector3.down, playerHeight * .5f + .2f, groundMask);

        // Reset jump count when landing
        if (grounded && !wasGrounded)
            jumpCount = 0;

        wasGrounded = grounded;


        HandleInput();
        HandleState();
        HandleSpeedAndDrag();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        MovePlayer();
    }

    // HANDLERS ==========================================================

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpBind) && jumpCount < maxJumpCount && canJump)
            Jump();

        if (Input.GetKeyDown(crouchBind))
            StartCrouch();
        if (Input.GetKeyUp(crouchBind))
            EndCrouch();
    }

    private void HandleState()
    {
        bool crouchInp = Input.GetKey(crouchBind);
        bool sprintInp = Input.GetKey(sprintBind);
        bool grappleInp = Input.GetKey(grappleBind);

        if (grounded && crouchInp && rb.linearVelocity.magnitude > minSlideSpeed) state = MS.sliding;
        else if (grappleInp && TryGetComponent<SpringJoint>(out _)) state = MS.grappling;
        else if (!grounded) state = MS.air;
        else if (crouchInp) state = MS.crouching;
        else if (sprintInp) state = MS.sprinting;
        else if (rb.linearVelocity.magnitude > 0.1f) state = MS.walking;
        else state = MS.idle;
    }

    private void HandleSpeedAndDrag()
    {
        moveSpeed = state switch
        {
            MS.walking => walkSpeed,
            MS.sliding => slideSpeed,
            MS.sprinting => sprintSpeed,
            MS.crouching => crouchSpeed,
            MS.grappling => grappleSpeed,

            _ => moveSpeed,
        };

        //set drag to slide drag if sliding, or add ground drag if grounded
        rb.linearDamping =
            state == MS.sliding ? slideDrag :
            grounded ? groundDrag : 0f;

        // add an extra speed check for sliding to ensure its smooth
        if (state == MS.sliding && rb.linearVelocity.magnitude < minSlideSpeed)
        {
            state = MS.crouching;
        }
    }

    // PHYSICS ===========================================================

    private void MovePlayer()
    {
        // Build move direction
        Vector3 targetDir = orientation.forward * verticalInput + orientation.right * (horizontalInput * 0.5f);
        moveDirection = Vector3.Lerp(moveDirection, targetDir.normalized, 0.2f);

        if (state == MS.sliding)
        {
            if (slideStarted)
            {
                rb.AddForce(rb.linearVelocity.normalized * slideStartImpulse, ForceMode.Impulse);
                slideStarted = false;            
            }

            if (OnSlope())
                rb.AddForce(GetSlopeMoveDirection() * 10f, ForceMode.Force);

            return;
        }

        Vector3 appliedVelocity = moveDirection * moveSpeed * 10f;
        if (grounded)
            rb.AddForce(appliedVelocity, ForceMode.Force);
        else
            rb.AddForce(appliedVelocity * airMultiplier, ForceMode.Force);

        //limit velocity smoothly
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }

        rb.useGravity = !(OnSlope() && state != MS.grappling);
    }

    private void Jump()
    {
        canJump = false;
        jumpCount++;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        Invoke(nameof(ResetJump), 0.2f);
    }
    private void ResetJump()
    {
        canJump = true;
    }

    private void StartCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideStarted = true;
    }
    private void EndCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, startScale, transform.localScale.z);
        slideStarted = false;
    }

    // MISC FUNCTIONS ====================================================

    private bool OnSlope()
    {
        if (Physics.Raycast(playerObj.transform.position, Vector3.down, out slopeHit, playerHeight * .5f + .1f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection() => Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
}
