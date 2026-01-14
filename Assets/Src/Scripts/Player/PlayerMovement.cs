using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Collections.Generic;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;

    public float grappleSpeed;
    public MovementState state;

    [Header("Jumping")]
    public float jumpForce;
    public float airMultiplier;
    public int jumpCount;
    public int extraAirJumps;

    public bool canJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchScale;
    private float startScale;

    [Header("Sliding")]
    public float slideMinSpeed;
    public float slideForce;
    public float slideDrag;
    public float slideExitSpeed;

    public float slideImpulse;
    public float maxSlideSpeed;

    bool slideJustStarted;

    [Header("Ground Check")]
    public float maxSlopeAngle;
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Obj References")]
    public GameObject playerObj;
    public Transform camPos;
    public Transform orientation;
    public Rigidbody rb;

    //private misc
    private RaycastHit slopeHit;
    private bool exitingSlope;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public enum MovementState
    {
        none,
        walking,
        crouching,
        sliding,
        sprinting,
        grappling,
        air
    }

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        startScale = transform.localScale.y;

        if (orientation == null)
            orientation = transform.Find("Orientation");

        if (!IsOwner)
            enabled = false;
    }

    private void Update()
    {
        Vector3 rayOrigin = new Vector3(playerObj.transform.position.x, playerObj.transform.position.y, playerObj.transform.position.z);
        grounded = Physics.Raycast(rayOrigin, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (state == MovementState.sliding)
        {
            rb.linearDamping = slideDrag;
            jumpCount = 0;
        }
        else if (grounded)
        {
            rb.linearDamping = groundDrag;
            jumpCount = 0;
        }
        else
            rb.linearDamping = 0;

        if (state == MovementState.sliding && rb.linearVelocity.magnitude < slideExitSpeed)
            state = MovementState.crouching;
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

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, startScale, transform.localScale.z);
        }

    }

    private void StateHandler()
    {
        bool crouchInput = Input.GetKey(KeyCode.LeftControl);
        bool sprintInput = Input.GetKey(KeyCode.LeftShift);
        bool grappleInput = Input.GetKey(KeyCode.Mouse1);

        if (grounded && crouchInput && rb.linearVelocity.magnitude > slideMinSpeed)
        {
            if (state != MovementState.sliding)
                slideJustStarted = true;

            state = MovementState.sliding;
        }
        else if (grappleInput && gameObject.TryGetComponent<SpringJoint>(out SpringJoint sj))
            state = MovementState.grappling;
        else if (!grounded)
            state = MovementState.air;
        else if (crouchInput)
            state = MovementState.crouching;
        else if (sprintInput)
            state = MovementState.sprinting;
        else if (rb.linearVelocity.magnitude != 0)
            state = MovementState.walking;
        else
            state = MovementState.none;

        switch (state)
        {
            case MovementState.crouching:
                moveSpeed = crouchSpeed;
                break;
            case MovementState.grappling:
                moveSpeed = grappleSpeed;
                break;
            case MovementState.sprinting:
                moveSpeed = sprintSpeed;
                break;
            case MovementState.walking:
                moveSpeed = walkSpeed;
                break;
            case MovementState.air:
                break;
        }
    }

    private void MovePlayer()
    {
        if (state == MovementState.sliding)
        {
            if (slideJustStarted)
            {
                rb.AddForce(rb.linearVelocity.normalized * slideImpulse, ForceMode.Impulse);
                slideJustStarted = false;
            }

            if (OnSlope())
            {
                Vector3 slopeDir = GetSlopeMoveDirection();
                rb.AddForce(slopeDir * slideForce, ForceMode.Force);
            }

            return;
        }

        moveDirection = orientation.forward * verticalInput + orientation.right * (horizontalInput * .5f);

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier * 10, ForceMode.Force);

        rb.useGravity = !OnSlope() || state == MovementState.grappling;
    }

    private void SpeedControl()
    {
        if (state == MovementState.sliding)
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            if (flatVel.magnitude > maxSlideSpeed)
            {
                Vector3 limited = flatVel.normalized * maxSlideSpeed;
                rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
            }
            return;
        }
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        jumpCount++;
        canJump = false;
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        Invoke(nameof(ResetJump), .2f);
    }
    private void ResetJump()
    {
        canJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(playerObj.transform.position, Vector3.down, out slopeHit, playerHeight * .5f + .1f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}