using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    public static PlayerMovement instance;

    public float slopeyAngle;
    public bool SlopeIncoming = false;
    private RaycastHit Test2;

    [SerializeField] private PlayerMovementData data;

    [Header("Movement")]
    private float moveSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float firstJump;

    public float swingSpeed;

    public float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public bool readyToJump = true;
    int currentJump = 0;
    public bool inAir;

    public bool freeze;
    private bool enableMovementOnNextTouch;

    [Header("Controls")]
    public UserInputs Controls;

    [Header("Dashing")]
    public bool isDashing;
    public float dashSpeed;
    private float dashTimer;

    private bool keepMomentum;
    public float dashSpeedChangeFactor;

    public bool useCameraForward = true;
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetVel = true;

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

    [Header("Grappling")]

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopAngle;
    private bool exitingSlope;
    private RaycastHit slopeHit;

    [Header("Particles")]
    public ParticleSystem stepPart;
    public ParticleSystem speedLines;

    [Header("References")]
    public PlayerCam cam;
    public float sprintFOV;
    public float normalFOV;
    public Transform playerCam;
    public Transform orientation;

    [HideInInspector] public Vector2 moveDirection;
    public Vector3 calculatedMoveDirection;

    public PhysicsMaterial slideMat;
    public PhysicsMaterial groundMat;
    public Collider col;
    public Rigidbody rb;    
    public PlayerGrapple pg;

    public MovementState currentState;
    public MovementState oldState;
    public enum MovementState
    {
        walking,
        sprinting,
        sliding,
        dashing,
        wallrunning,
        swinging,
        air,
        freeze
    }

    public bool swinging;
    public bool wallrunning;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

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
        PredictSlope();
        SpeedParticleControl();

        if(grounded && currentJump >= 0)
            currentJump = 0;
       if (grounded && !activeGrapple && !isDashing && !swinging)
        {
            //print("reset to ground drag");
            rb.linearDamping = data.groundDrag;

        }
        else if (activeGrapple == true || isDashing == true || swinging == true)
            rb.linearDamping = 0;

       if(data.dashCdTimer >= 0)
            data.dashCdTimer -= Time.deltaTime;

        //if (moveSpeed == 0)
        //{
        //    desiredMoveSpeed = data.walkSpeed;
        //    moveSpeed = desiredMoveSpeed;
        //}
    }
    private void FixedUpdate()
    {
        MovePlayer();
        gravityControl();
    }

    void SpeedParticleControl()
    {
        if (rb.linearVelocity.sqrMagnitude > 0 && !speedLines.isPlaying)
        {
            speedLines.Play();
            print("playing particles");
            //var radius = speedLines.shape.radius;
            //radius -= moveSpeed * Time.deltaTime;
        }
        else if (rb.linearVelocity.sqrMagnitude <= 0 && speedLines.isPlaying)
        {
            speedLines.Stop();  
        }

        if (speedLines.isPlaying)
        {
            var shape = speedLines.shape;
            float SpeedRadius = Mathf.Clamp(rb.linearVelocity.sqrMagnitude * 0.1f,25f, 22f);

            var speed = speedLines.main.startSpeed;
            speed = Mathf.Clamp(rb.linearVelocity.sqrMagnitude * 0.1f, 10f, 20f);

            shape.radius = SpeedRadius;
        }

    }
    void PredictSlope()
    {
        Vector3 ForwardValue = playerCam.transform.forward * moveDirection.y;

        Vector3 RightValue = playerCam.transform.right * moveDirection.x;
        Vector3 Direction3D = ForwardValue + RightValue;

        if (Physics.Raycast(transform.position + Direction3D, Vector3.down, out Test2, playerHeight * 0.5f + 10f, whatIsGround))
        {
            //print(Test2.transform.name);
            float angle = Vector3.Angle(Vector3.up, Test2.normal);
            slopeyAngle = angle;

            SlopeIncoming = slopeyAngle < 60f;
        }
        else
        {
            if (SlopeIncoming)
            {
                SlopeIncoming = false;
            }
        }

        Debug.DrawRay(transform.position + Direction3D, Vector3.down * playerHeight * 0.5f, Color.blue);


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
        if (sliding) return;
        speedLines.Play();
        Dash();
        
      
    }

    #region Dash 
    private void Dash()
    {
        if (data.dashCdTimer >= 0) return;
        data.dashCdTimer = data.dashCd;

        isDashing = true;
        desiredMoveSpeed = dashSpeed;
        keepMomentum = false;
        cam.DoFov(sprintFOV);
        disableGravity = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 forceToApply = calculatedMoveDirection * data.dashForce + orientation.up * data.dashUpwardForce;

        if (disableGravity)
            rb.useGravity = false;

        rb.AddForce(forceToApply, ForceMode.Impulse);
        Invoke(nameof(ResetDash), data.dashTime);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
     
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    public void ResetDash()
    {
        isDashing = false;
        keepMomentum = true;
        cam.DoFov(normalFOV);
        if(Controls.Player.Sprint.IsPressed() && grounded)
        {
            //ChangeState(MovementState.sprinting);
            //desiredMoveSpeed = data.sprintSpeed;
            cam.DoFov(sprintFOV);
        }
        else
        {
            desiredMoveSpeed = data.walkSpeed;
        }
        if (disableGravity)
            rb.useGravity = true;
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

    #endregion  

    private void StopPlayerSprint(InputAction.CallbackContext ctx)
    {
        if (!sliding)
        {
            speedLines.Stop();
            desiredMoveSpeed = data.walkSpeed;
            ChangeState(MovementState.walking);
            cam.DoFov(normalFOV);
        }
    }

    //Sliding
    private void HandleSlideStart(InputAction.CallbackContext ctx)
    {
        gravityControl();
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
        if (Controls.Player.Sprint.IsPressed())
        {
            desiredMoveSpeed = data.sprintSpeed;
        }
        else
        {
            desiredMoveSpeed = data.walkSpeed;
        }

    }

    // Jumping
    private void PlayerJump(InputAction.CallbackContext ctx)
    {
        Debug.Log("spacebar pressed");
        print($"{readyToJump}, {currentJump}, {data.baseJumpUses},is current jump lower than baseJumpUses:{currentJump < data.baseJumpUses}");
        if (readyToJump && currentJump < data.baseJumpUses && !wallrunning)
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
            inAir = false;
            print("GROUNDED LAD");
            if (!isDashing && !activeGrapple)
            {
                col.sharedMaterial = groundMat;
                rb.linearDamping = data.groundDrag;
            }
            ChangeState(MovementState.walking);
        }
        else if (!grounded && currentState != MovementState.air) // Runs upon leaving the ground 
        {
            print("in Air");
            inAir = true;
            col.sharedMaterial = slideMat;
            rb.linearDamping = data.airDrag;
            ChangeState(MovementState.air);
        }

        //reset Grapple
        if (grounded && !activeGrapple)
        {
            pg.grappleCount = 1;
            //col.sharedMaterial = groundMat;
           
        }
        if (pg.swinging == true || swinging == true)
            currentJump = 1;
    }   

    
    void gravityControl()
    {
        if (!grounded && rb.useGravity && !wallrunning && !isDashing && !pg.grappling)
        {
            rb.AddForce(Vector3.down * data.addGravity, ForceMode.Acceleration);
            rb.useGravity = true;
        }

    }
    void stateHandler()
    {
        if(wallrunning)
        {
            ChangeState(MovementState.wallrunning);
            //desiredMoveSpeed = wallrunSpeed;
        }
        else if (!wallrunning)
        {
            if (sliding)
            {
                ChangeState(MovementState.sliding);

                if (OnSlope() && rb.linearVelocity.y < 0.1f)
                    desiredMoveSpeed = slideSpeed;
                else
                    desiredMoveSpeed = slideSpeed;

            }
            else
            {

                if (Controls.Player.Sprint.IsPressed() && grounded)
                {
                    ChangeState(MovementState.sprinting);
                    desiredMoveSpeed = data.sprintSpeed;
                }
                else if (grounded)
                {
                    ChangeState(MovementState.walking);
                    desiredMoveSpeed = data.walkSpeed;
                }
                else if (!isDashing)
                {
                    ChangeState(MovementState.air);
                    desiredMoveSpeed = data.walkSpeed;
                }
            }

        }

        if (isDashing)
        {
            ChangeState(MovementState.dashing);
            desiredMoveSpeed = dashSpeed;
        }

        if (freeze)
        {
            ChangeState(MovementState.freeze);
            //moveSpeed = 0;
            //rb.linearVelocity = Vector3.zero;
        }
        else if (swinging)
        {
            ChangeState(MovementState.swinging);
            swingSpeed = moveSpeed;
        }

        if (!freeze && moveSpeed == 0)
        {
            moveSpeed = data.walkSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (oldState == MovementState.dashing) keepMomentum = true;

        //Debug.Log($"desiredMoveSpeed: {desiredMoveSpeed}, lastDesiredMoveSpeed: {lastDesiredMoveSpeed}, keepMomentum: {keepMomentum}, oldState: {oldState}, changed: {desiredMoveSpeedHasChanged}");

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                Debug.Log("Starting smooth lerp");
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                Debug.Log("Snapping speed");
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        Debug.Log("Momentum activating");
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedIncreaseMultiplier;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                Debug.Log("Slope Speed increasing");
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * boostFactor * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedIncreaseMultiplier = 1f;
        keepMomentum = false;
    }


    void MovePlayer()
    {
        if (activeGrapple) return;
        if (swinging) return;
        //if (freeze) return;

        calculatedMoveDirection = GetMoveDirection();

        if (sliding)
            SlidingMovement();

        if (OnSlope() && !exitingSlope)
            rb.AddForce(GetSlopeMoveDirection(calculatedMoveDirection) * desiredMoveSpeed * data.groundControlModifier, ForceMode.Force);
        else if (grounded)
        {
            rb.AddForce(calculatedMoveDirection * desiredMoveSpeed * data.groundControlModifier, ForceMode.Force);
            //Debug.Log($"applying force: {calculatedMoveDirection * desiredMoveSpeed * data.groundControlModifier}, moveDir: {calculatedMoveDirection}, grounded: {grounded}");
        }
        else if (!isDashing)
            rb.AddForce(calculatedMoveDirection * desiredMoveSpeed * data.airControlModifier, ForceMode.Force);
    }
    private void SpeedControl() 
    {

        if (activeGrapple || isDashing || currentState == MovementState.swinging) return;

        //Limit velotcity on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
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
        if (wallrunning || isDashing) return;

        exitingSlope = true;
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        // Makes your first  jump a bit higher so it feels better 
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
        
    ////public void JumpToPosition(Vector3 targetPosition,float trajectoryHeight)
    ////{
    ////    activeGrapple = true;
    ////    velocityToSet = CalculateJumpVelocity(transform.position, targetPosition , trajectoryHeight);
    ////    Invoke(nameof(SetVelocity), 0f);

    ////    Invoke(nameof(ResetRestrictions), 3f);
    ////}

    //private Vector3 velocityToSet;
    //private void SetVelocity()
    //{
    //    enableMovementOnNextTouch = true;
    //    rb.linearVelocity = velocityToSet;
    //}

    //public void ResetRestrictions()
    //{
    //   // activeGrapple = false;

    //}

    private void OnCollisionEnter(Collision collision)
    {
        if(enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            //ResetRestrictions();

            pg.StopGrapple();
        }
    }

    public bool OnSlope()
    {
        if (SlopeIncoming && grounded)
        {
            return true;
        }

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 10f, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            //slopeyAngle = angle;
            return angle < maxSlopAngle && angle != 0;
        }
        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction) 
    {
        if (SlopeIncoming)
        {
            return Vector3.ProjectOnPlane(direction, Test2.normal).normalized;
        }

        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public bool activeGrapple;
    //public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    //{
    //    float gravity = Physics.gravity.y;
    //    float displacementY = endPoint.y - startPoint.y;
    //    Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

    //    Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
    //    Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
    //        + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

    //    return velocityXZ + velocityY;
    //}
    private Vector3 GetMoveDirection()
    {
        return (orientation.forward * moveDirection.y + orientation.right * moveDirection.x).normalized;
    }
}
    
