using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

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
        Controls.Enable();

    }
    private void OnDisable()
    {
        Controls.Player.Move.performed -= ChangePlayerMovement;
        Controls.Player.Move.canceled -= StopPlayerMovement;
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
    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Update()
    {
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


        // calculatedMoveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //  rb.AddForce(calculatedMoveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // calculate movement direction
    }
}