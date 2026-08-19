using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerWallRun : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Walljumping")]
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallJumpUses;
    int currentWallJump;

    [Header("Camera VFX")]
    public float wallJumpFOV;
    public float tiltAmount;
    public float tiltCooldown;

    [Header("References")]
    public UserInputs Controls;
    public Transform orientation;
    private Rigidbody rb;
    private PlayerMovement pm;
    public PlayerCam cam;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    private void OnEnable()
    {
        Controls = UserInputManager.Instance.Controls;
        Controls.Player.Jump.performed += PlayerJump;
    }

    private void OnDisable()
    {
        Controls.Player.Jump.performed -= PlayerJump;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();  
        pm = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWall();

        if (pm.grounded && currentWallJump >= 0)
            currentWallJump = 0;
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
            WallRunningMovement();

        if (pm.moveDirection != Vector2.zero)
        {
            if ((wallLeft || wallRight) && !pm.grounded)
            {
                if (!pm.wallrunning)
                {
                    pm.wallrunning = true;
                }
            }
            else
            {
                if (pm.wallrunning)
                {
                    StopWallRun();
                }
            }
        }
    }

    private void PlayerJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && pm.wallrunning)
        {
            WallJump();
        }
    }
    public void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    } 

    private void StateMachine()
    {
    }

    private void WallRunningMovement()
    {
        //rb.useGravity = false;
        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        ////caluclates if the wall is to the left or right of you (if its not on the right then its on the left)
        //Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        //// calculates forward by using up and away from the wall
        //Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        ////switches orientation around so you arent stuck going in one direction despite facing the other
        //if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        //    wallForward = -wallForward;

        //// forward force
        //rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //// push to wall force
    }
    private void StopWallRun()
    {
        pm.wallrunning = false;
        rb.useGravity = true;

        Debug.Log("Stopping wall run");
    }


    public async void WallJump()
    {

        /*shortened "if" "else" statement with the "?"
        if (wallRight)
        {
            Vector3 wallNormal = rightWallhit.normal;
        }
        else
        {
            Vector3 wallNormal = leftWallhit.normal;
        }
        */

        if (currentWallJump < wallJumpUses)
        {
            currentWallJump++;

            Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

            Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

            //reset y velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            //add force
            rb.AddForce(forceToApply, ForceMode.Impulse);

            cam.DoFov(wallJumpFOV);
            if (wallLeft) cam.DoTilt(-tiltAmount);
            if (wallRight) cam.DoTilt(tiltAmount);

            await Awaitable.WaitForSecondsAsync(tiltCooldown, destroyCancellationToken);

            cam.DoFov(pm.normalFOV);
            cam.DoTilt(0f);
        }
    }
}
