using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerMovementData data;

    [Header("Movement")]
    private float moveSpeed;
    public float slideSpeed;
    public float firstJump;
   


    public float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    bool readyToJump = true;
    int currentJump = 0;

    public bool freeze;
    private bool enableMovementOnNextTouch;

    [Header("Controls")]
    public UserInputs Controls;

    [Header("Dashing")]
    public bool isDashing;
    public float dashSpeed;
    private float dashTimer;


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
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopAngle;
    private bool exitingSlope;
    private RaycastHit slopeHit;

    public PlayerCam cam;
    public float sprintFOV;
    public float normalFOV;
    public Transform orientation;

    Vector2 moveDirection;
    Vector3 calculatedMoveDirection;

    Rigidbody rb;
    public PlayerGrapple pg;

    public MovementState currentState;
    public MovementState oldState;
    public enum MovementState
    {
        walking,
        sprinting,
        sliding,
        dashing,
        air,
        freeze
    }

    private void Start()
    {
        //pg = GetComponent<PlayerGrapple>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        desiredMoveSpeed = data.walkSpeed;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        SpeedControl();
        GroundDetection();
        stateHandler();
        gravityControl();

       if (grounded && !activeGrapple && !isDashing)
        {
            print("reset to ground drag");
            rb.linearDamping = data.groundDrag;

        }
        else if (activeGrapple == true || isDashing == true)
            rb.linearDamping = 0;

       if(data.dashCdTimer >= 0)
            data.dashCdTimer -= Time.deltaTime;

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
            Dash();
        ChangeState(MovementState.dashing);
      
    }

    private void Dash()
    {
        if (data.dashCdTimer >= 0) return;
        else data.dashCdTimer = data.dashCd;
            Debug.Log("Dash started");

        isDashing = true;
        cam.DoFov(sprintFOV);
        dashTimer = data.dashTime;

        Vector3 forceToApply = orientation.forward * data.dashForce + orientation.up * data.dashUpwardForce;
        rb.AddForce(forceToApply, ForceMode.Impulse);
        Invoke(nameof(DelayedDashForce), 0.025f);
        Invoke(nameof(ResetDash), data.dashTime);

       // StartCoroutine(DashRoutine());
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    { 
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        Debug.Log("dash endeding");
        isDashing = false;
    }


    /* IEnumerator DashRoutine()
     {
         while (dashTimer > 0)
         {
             rb.AddForce(GetMoveDirection() * data.dashSpeed, ForceMode.Force);
             dashTimer -= Time.deltaTime;
             yield return null;
         }

         rb.linearVelocity *= data.dashExitSpeed;
         isDashing = false;
     }*/

    private void StopPlayerSprint(InputAction.CallbackContext ctx)
    {
        //Mode - Walking
        //isDashing = false ;
        ChangeState(MovementState.walking);
        desiredMoveSpeed = data.walkSpeed;
        cam.DoFov(normalFOV);
    }

    //Sliding
    private void HandleSlideStart(InputAction.CallbackContext ctx)
    {
        if (GetMoveDirection() != Vector3.zero)
            StartSlide();
        if (sliding)
        {
            ChangeState(MovementState.sliding);

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = data.sprintSpeed;

        }
    }
    private void HandleSlideStop(InputAction.CallbackContext ctx)
    {
        StopSlide();
    }

    private void StartSlide()
    {
        sliding = true;

        calculatedMoveDirection = (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;
        transform.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 20f, ForceMode.Impulse);
        rb.linearDamping = data.slideDrag;

        slideTimer = maxSlideTime;

    }

    private void SlidingMovement()
    {
        //normal sliding
        if (!OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(GetMoveDirection().normalized * slideForce, ForceMode.Force);

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

    // Jumping
    private void PlayerJump(InputAction.CallbackContext ctx)
    {

        if (readyToJump && currentJump < data.baseJumpUses)
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

            if (!isDashing)
            {
                rb.linearDamping = data.groundDrag;
            }
            currentState = MovementState.walking;
        }
        else if (!grounded && currentState != MovementState.air) // Runs upon leaving the ground 
        {
            rb.linearDamping = data.airDrag;
            currentState = MovementState.air;
        }
    }

    void gravityControl()
    { 
        if (!OnSlope() && !grounded)
            rb.AddForce(Vector3.down * data.addGravity, ForceMode.Acceleration);


    }
    void stateHandler()
    {
        if (isDashing)
        {
            currentState = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;


        }
        // Mode - Freeze 
        if (freeze)
        {
            currentState = MovementState.freeze;
            moveSpeed = 0;
            rb.linearVelocity = Vector3.zero;

        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 8f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed =Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }    
 

    void MovePlayer()
    {
        if (activeGrapple) return;


        calculatedMoveDirection = GetMoveDirection();

        switch (currentState)
        {
            case MovementState.walking:
                break;
        }
        if (sliding)
            SlidingMovement();

        if (OnSlope())
            rb.AddForce(GetSlopeMoveDirection(calculatedMoveDirection) * desiredMoveSpeed * data.groundControlModifier, ForceMode.Force);

        if (grounded)
            rb.AddForce(calculatedMoveDirection * desiredMoveSpeed * data.groundControlModifier, ForceMode.Force);

        else
        {

            calculatedMoveDirection = (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;

            if (OnSlope() && !exitingSlope)
            {
                rb.AddForce(GetSlopeMoveDirection(calculatedMoveDirection) * desiredMoveSpeed * 20f, ForceMode.Force);

              //  if (rb.linearVelocity.y > 0)
                  //  rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }

            else if (grounded && !isDashing)
                rb.AddForce(calculatedMoveDirection * desiredMoveSpeed * data.groundControlModifier, ForceMode.Force);

            else if (!grounded && !isDashing)
                rb.AddForce(calculatedMoveDirection * desiredMoveSpeed * data.airControlModifier, ForceMode.Force);

            //turn gravity off while on slope
            rb.useGravity = !OnSlope();


            // calculatedMoveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
            //  rb.AddForce(calculatedMoveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            // calculate movement direction

        }
    } 
    private void SpeedControl() 
    {
        if (activeGrapple || isDashing) return;

        //Limit velotcity on slope
        if (OnSlope())
        {
            if (rb.linearVelocity.magnitude > desiredMoveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * desiredMoveSpeed;
        }

        //limit velocity when needed
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            
            if (flatVel.magnitude > desiredMoveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * desiredMoveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }

        }

        //limiting speed on ground or in air
    }
    private void Jump()
    {
        exitingSlope = true;
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        // Makes your first jump a bit higher so it feels better 
        if (currentJump == 1)
            rb.AddForce(transform.up * data.jumpForce * firstJump, ForceMode.Impulse);

        else
            rb.AddForce(transform.up * data.jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        Debug.Log("reset jump");

        exitingSlope = false;

        readyToJump = true;
    }
        
    public void JumpToPosition(Vector3 targetPosition,float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition , trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.linearVelocity = velocityToSet;
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            pg.StopGrapple();
        }
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

    public bool activeGrapple;
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
    private Vector3 GetMoveDirection()
    {
        return (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;
    }
}
    
