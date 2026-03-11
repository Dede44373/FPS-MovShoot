using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerMovementData data;

    [Header("Movement")]
    private float moveSpeed;
    

    bool readyToJump = true;
    int currentJump = 0;

    [Header("Controls")]
    public UserInputs Controls;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

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
        crouching,
        air
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        moveSpeed = data.walkSpeed;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        rb.AddForce(Vector3.down * data.addGravity, ForceMode.Acceleration);
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
        Controls.Player.Move.performed += ChangePlayerMovement;
        Controls.Player.Move.canceled += StopPlayerMovement;
        Controls.Player.Sprint.performed += PlayerSprint;
        Controls.Player.Sprint.canceled += StopPlayerSprint;
        Controls.Player.Crouch.performed += PlayerCrouch;
        Controls.Player.Crouch.canceled += StopPlayerCrouch;
        Controls.Player.Jump.performed += PlayerJump;
    }
    private void OnDisable()
    {
        Controls.Player.Move.performed -= ChangePlayerMovement;
        Controls.Player.Move.canceled -= StopPlayerMovement;
        Controls.Player.Sprint.performed -= PlayerSprint;
        Controls.Player.Sprint.canceled -= StopPlayerSprint;
        Controls.Player.Crouch.performed -= PlayerCrouch;
        Controls.Player.Crouch.canceled -= StopPlayerCrouch;
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
    private void ChangePlayerMovement(InputAction.CallbackContext ctx)
    {
        moveDirection = ctx.ReadValue<Vector2>();
        //calculatedMoveDirection = (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;
    }
    // Walking
    private void StopPlayerMovement(InputAction.CallbackContext ctx)
    {
        calculatedMoveDirection = Vector3.zero;
        moveDirection = Vector2.zero;
    }

    //Sprinting
    private void PlayerSprint(InputAction.CallbackContext ctx)
    {
        if (grounded)
        {
            ChangeState(MovementState.sprinting);
            moveSpeed = data.sprintSpeed;
        }
    }

    private void StopPlayerSprint(InputAction.CallbackContext ctx)
    {
        //Mode - Walking
        if (grounded)
        {
            ChangeState(MovementState.walking);
            moveSpeed = data.walkSpeed;
        }
    }

    //Sliding
    private void PlayerCrouch(InputAction.CallbackContext ctx)
    {
        /*moveDirection = ctx.ReadValue<Vector2>();
        calculatedMoveDirection = (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;*/
        calculatedMoveDirection = (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;
        ChangeState(MovementState.crouching);
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        rb.linearDamping = data.slideDrag;
        //moveSpeed = crouchSpeed;
    }
    private void StopPlayerCrouch(InputAction.CallbackContext ctx)
    {
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
        calculatedMoveDirection = (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;

        if (grounded)
            rb.AddForce(calculatedMoveDirection * moveSpeed * data.groundControlModifier, ForceMode.Force);

        else
            rb.AddForce(calculatedMoveDirection * moveSpeed * data.airControlModifier, ForceMode.Force);


        // calculatedMoveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //  rb.AddForce(calculatedMoveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // calculate movement direction
    }
    private void SpeedControl() 
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x , 0f, rb.linearVelocity.z);

        //limit velocity when needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
    private void Jump()
    {
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * data.jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        Debug.Log("reset jump");

        readyToJump = true;
    }
    
}
    
