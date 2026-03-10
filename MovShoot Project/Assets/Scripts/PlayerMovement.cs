using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;


    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Controls")]
    public UserInputs Controls;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    Vector2 moveDirection;
    Vector3 calculatedMoveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Controls = UserInputManager.Instance.Controls;
        Controls.Player.Move.performed += ChangePlayerMovement;
        Controls.Player.Move.canceled += StopPlayerMovement;
        Controls.Player.Jump.performed += PlayerJump;
        Controls.Enable();

    }
    private void OnDisable()
    {
        Controls.Player.Move.performed -= ChangePlayerMovement;
        Controls.Player.Move.canceled -= StopPlayerMovement;
        Controls.Player.Jump.performed -= PlayerJump;
    }   
    private void ChangePlayerMovement(InputAction.CallbackContext ctx)
    {
        moveDirection = ctx.ReadValue<Vector2>();
        print($"Movement direction: {moveDirection}");

    }

    private void StopPlayerMovement(InputAction.CallbackContext ctx)
    {
        calculatedMoveDirection = Vector3.zero;
        moveDirection = Vector2.zero;

    }

    private void PlayerJump(InputAction.CallbackContext ctx)
    {
        
        if(readyToJump && grounded)
        {
            readyToJump = false;

            Jump();
            Debug.Log("I hate this shitty input system");
            Invoke(nameof(ResetJump), jumpCooldown);
        }

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Update()
    {
        SpeedControl();
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        Debug.DrawRay(transform.position, Vector3.down,Color.red);
        //handle drag
        if (grounded)   
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }
    void MovePlayer()
    {
        calculatedMoveDirection = orientation.forward * moveDirection.y + orientation.right * moveDirection.x;
        rb.AddForce(calculatedMoveDirection * moveSpeed * 10f, ForceMode.Force);

        if (grounded)
            rb.AddForce(calculatedMoveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        else if (!grounded)
            rb.AddForce(calculatedMoveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Force);
     

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

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        Debug.Log("reset jump");

        readyToJump = true;
    }
    
}
    
