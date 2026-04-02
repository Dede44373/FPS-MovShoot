using System.Collections;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerMovementData data;

    [Header("Movement")]
    bool isDashing;
    private float dashTimer;
    public float slideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    bool readyToJump = true;
    int currentJump = 0;

    [Header("Controls")]
    public UserInputs Controls;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    public bool sliding = false;
    public Transform playerObj;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopAngle;
    private bool exitingSlope;
    private RaycastHit slopeHit;

    public Camera cam;
    public float sprintFOV = 70;
    public Transform orientation;

    Vector2 moveDirection;
    Vector3 calculatedMoveDirection;

    Rigidbody rb;
    
    public MovementState currentState;
    public MovementState oldState;
    public enum MovementState
    {
        walking, 
        sprinting,
        sliding,
        air
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        desiredMoveSpeed = data.walkSpeed;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
       // rb.AddForce(Vector3.down * data.addGravity, ForceMode.Acceleration);
        SpeedControl();
        GroundDetection();

    }
    private void FixedUpdate()
    {
        MovePlayer();
    }

    #region Input Subscribing

    private void OnEnable()
    {
        Controls = UserInputManager.Instance.Controls;
        Controls.Player.Move.performed += HandleMoveStart;
        Controls.Player.Move.canceled += HandleMoveStop;
        Controls.Player.Sprint.performed += PlayerSprint;
        Controls.Player.Sprint.canceled += StopPlayerSprint;
        Controls.Player.Crouch.performed += HandleSlideStart;
        Controls.Player.Crouch.canceled += HandleSlideStop;
        Controls.Player.Jump.performed += PlayerJump;
    }
    private void OnDisable()
    {
        Controls.Player.Move.performed -= HandleMoveStart;
        Controls.Player.Move.canceled -= HandleMoveStop;
        Controls.Player.Sprint.performed -= PlayerSprint;
        Controls.Player.Sprint.canceled -= StopPlayerSprint;
        Controls.Player.Crouch.performed -= HandleSlideStart;
        Controls.Player.Crouch.canceled -= HandleSlideStop;
        Controls.Player.Jump.performed -= PlayerJump;
    }

    #endregion

    private void ChangeState(MovementState newState)
    {
        oldState = currentState;
        currentState = newState;
        print($"State changed from {oldState} to {newState}");
    }


    // Calculating walking direction
    private void HandleMoveStart(InputAction.CallbackContext ctx)
    {
        moveDirection = ctx.ReadValue<Vector2>();
        //calculatedMoveDirection = (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;
    }
    // Walking
    private void HandleMoveStop(InputAction.CallbackContext ctx)
    {
        //calculatedMoveDirection = Vector3.zero;
        moveDirection = Vector2.zero;
    }

    //Sprinting
    private void PlayerSprint(InputAction.CallbackContext ctx)
    {
        if (grounded)
        {
            ChangeState(MovementState.sprinting);
            
            desiredMoveSpeed = data.sprintSpeed;
        }
        if (isDashing == false)
        {
            // cam.fieldOfView = sprintFOV;
            Dash();
        }
    }

    private void Dash()
    {
        isDashing = true;
        dashTimer = data.dashTime;
        StartCoroutine(DashRoutine());
    }



    IEnumerator DashRoutine()
    {
        while (dashTimer > 0)
        {
            rb.AddForce(GetMoveDirection() * data.dashSpeed, ForceMode.Force);
            dashTimer -= Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity *= data.dashExitSpeed;
        isDashing = false;
    }

    private void StopPlayerSprint(InputAction.CallbackContext ctx)
    {
        //Mode - Walking
        if (grounded)
        {
            ChangeState(MovementState.walking);
            desiredMoveSpeed = data.walkSpeed;
            //cam.fieldOfView = 60f;
        }
    }

    //Sliding
    private void HandleSlideStart(InputAction.CallbackContext ctx)
    {
        if (GetMoveDirection() != Vector3.zero)
            StartSlide();
    }
    private void HandleSlideStop(InputAction.CallbackContext ctx)
    {
        StopSlide();
    }

    private void StartSlide()
    {
        sliding = true;

        calculatedMoveDirection = (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;
        ChangeState(MovementState.sliding);
        transform.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);    
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        rb.linearDamping = data.slideDrag;

        slideTimer = maxSlideTime;

    }

    private void SlidingMovement()
    {
        //normal sliding
        if(!OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(GetMoveDirection() * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;

        }

        //sliding down a slope
        else
        {
            rb.AddForce(GetSlopeMoveDirection(GetMoveDirection()) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        sliding = false;
        ChangeState(MovementState.walking);
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        rb.linearDamping = data.groundDrag;
        // moveSpeed = data.walkSpeed;

    }

    private void PlayerJump(InputAction.CallbackContext ctx)
    {
  
        if(readyToJump && currentJump < data.baseJumpUses)
        {
            readyToJump = false;
            currentJump++;

            Jump();
            Debug.Log("jumping");
            Invoke(nameof(ResetJump), data.jumpCooldown);
        }

    }

    private void GroundDetection()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        Debug.DrawRay(transform.position, Vector3.down, Color.red);
        //handle drag
        if (grounded && currentState == MovementState.air) // Runs upon landing from air
        {
            currentJump = 0;
            rb.linearDamping = data.groundDrag;
            currentState = MovementState.walking;
        }
        else if(!grounded && currentState != MovementState.air) // Runs upon leaving the ground 
        {
            rb.linearDamping = data.airDrag;
            currentState = MovementState.air;
        }
    }

    void MovePlayer()
    {
        if (isDashing) return;

        calculatedMoveDirection = GetMoveDirection();

        switch (currentState)
        {
            case MovementState.walking:
                break;
        }
        if (sliding)
            SlidingMovement();

        if (OnSlope())
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * desiredMoveSpeed * data.groundControlModifier, ForceMode.Force);

        if (grounded)
            rb.AddForce(calculatedMoveDirection * desiredMoveSpeed * data.groundControlModifier, ForceMode.Force);

        else
        {

            calculatedMoveDirection = (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;

            if (OnSlope() && !exitingSlope)
            {
                rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

                if (rb.linearVelocity.y > 0)
                    rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }

            else if (grounded)
                rb.AddForce(calculatedMoveDirection * moveSpeed * data.groundControlModifier, ForceMode.Force);

            else if(!grounded)
                rb.AddForce(calculatedMoveDirection * moveSpeed * data.airControlModifier, ForceMode.Force);

            //turn gravity off while on slope
            rb.useGravity = !OnSlope();


        // calculatedMoveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //  rb.AddForce(calculatedMoveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // calculate movement direction

    }
    private void SpeedControl() 
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x , 0f, rb.linearVelocity.z);

        //limit velocity when needed
        if(flatVel.magnitude > desiredMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * desiredMoveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }

        //limiting speed on ground or in air
        else 
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            //limit velocity when needed
            if(flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

    }
    private void Jump()
    {
        exitingSlope = true;
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * data.jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        Debug.Log("reset jump");

        exitingSlope = false;

        readyToJump = true;
    }
        
    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f ))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopAngle && angle != 0;

        }

        return false;

    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction) 
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    
    private Vector3 GetMoveDirection()
    {
        return (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;
    }
}
    
