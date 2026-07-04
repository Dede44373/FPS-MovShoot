using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerGrapple : MonoBehaviour
{
    [Header("References")]
    public UserInputs controls;
    public Transform cam;
    public Transform gunTip;
    public LayerMask grappleable;
    public LineRenderer lr;
    public PlayerMovement pm;
    private GameObject detectedGO;
    public PlayerCam fovCam;
    public Transform playerBottom;
    [SerializeField] private Rigidbody rb;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    public float grappleSpeed;
    public float grappleCount;

    private Vector3 grapplePoint;

    public float grappleFOV;
    
    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    private bool isApplyingGrappleForce;

    public bool grappling;

    public bool freezePlayer;

    [Header("Rope")]
    private float ropeLength;
    private bool isRopeInTension;
    [SerializeField] private float minRopeLength;

    private float reelInSpeed;
    private bool isReelingIn;
    [SerializeField] private float reelInAcceleration;


    // Update is called once per frame
    void Update()
    {
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;

    }

    private void LateUpdate()
    {
        if (grappling == true)
        {
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, grapplePoint);
        }
    }

    private void FixedUpdate()
    {
        if (isApplyingGrappleForce)
        {
            ApplyGrappleForces();

            // when do you want to apply rope tension or tugging
            if(Vector3.Dot(rb.linearVelocity, grapplePoint - rb.position) <= 0 && isRopeInTension)
            {
                TugPlayer();
            }

            if(ropeLength > minRopeLength && isReelingIn)
            {
                reelInSpeed += reelInAcceleration * Time.fixedDeltaTime;
                ropeLength -= reelInSpeed *Time.fixedDeltaTime;
            }
            else
            {
                isReelingIn = false;
                ropeLength = minRopeLength;
            }
        }

    }
    private void OnEnable()
    {
        lr.enabled = false;
        controls = UserInputManager.Instance.Controls;
        controls.Player.Grapple.performed += HandleGrappleStart;
        controls.Player.Grapple.canceled += HandleGrappleStop;
    }

    private void OnDisable()
    {
        controls.Player.Grapple.performed -= HandleGrappleStart;
        controls.Player.Grapple.canceled -= HandleGrappleStop;
    }
    private void HandleGrappleStart(InputAction.CallbackContext ctx)
    {
        StartGrapple();
        if (grappling)
        {
            isApplyingGrappleForce = true;

            isRopeInTension = ropeLength * ropeLength < (grapplePoint - rb.position).sqrMagnitude;
        }
        else
        {
            grappling = false;
            isApplyingGrappleForce = false;


        }

    }

    private void HandleGrappleStop(InputAction.CallbackContext ctx)
    {
        if(grappling == true)
        StopGrapple();
    }

    //maths for calculating swinging velocity
    private void ApplyGrappleForces()
    {
        //calculating theta
        Vector3 direction = (grapplePoint - rb.position).normalized;
        float theta = Vector3.Angle(direction, Vector3.up) * Mathf.Deg2Rad;

        float centripetalAcceleration = rb.linearVelocity.sqrMagnitude / ropeLength;
        Vector3 tension = rb.mass * (centripetalAcceleration + Physics.gravity.magnitude * Mathf.Cos(theta)) * direction;

        if (isRopeInTension)
        { 
            if(isReelingIn)
            {
                rb.AddForce(rb.mass * reelInAcceleration * direction);
            }

            rb.AddForce(tension, ForceMode.Force);
        }
    }

    private void TugPlayer()
    {
        Vector3 direction = (grapplePoint - rb.position).normalized;

        Vector3 tangentialVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, direction);
        rb.linearVelocity = tangentialVelocity;

        isRopeInTension = true;

        rb.position = grapplePoint - direction * ropeLength;
    }

    private void StartGrapple()
    {
        if (grappling) return;
        // makes sure that if you're grappling or swinging it stops it before starting a new grapple/or prevents it
        if (grapplingCdTimer > 0) return;
        //GetComponent<PlayerSwing>().StopSwing();
        rb.linearDamping = 0;

        //if you have more than 1 grapple count left you can grapple
        if (grappleCount >= 1)
        {
            grappleCount--;
            grappling = true;
            // freezes all the player's movements and velocities
            // creates a raycast from the camera position, forwards (where you're looking), then stores the value in 'hit', and max distance it can travel.
            RaycastHit hit;
            if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance))
            {
                detectedGO = hit.transform.gameObject;
                grapplePoint = hit.point;

                pm.freeze = true;
                freezePlayer = true;

                ropeLength = (grapplePoint - rb.position).magnitude;
                isReelingIn = true;
                reelInSpeed = 0;

                if (HasLayerMask(detectedGO, grappleable))
                {
                    pm.activeGrapple = true;
                   // Invoke(nameof(ExecuteGrapple), grappleDelayTime);
                    Debug.Log("Start Grapple");
                }
                else
                {
                    grappleCount = 1;
                    //grapplePoint = cam.position + cam.forward * maxGrappleDistance;

                    Invoke(nameof(StopGrapple), grappleDelayTime);
                    Debug.Log("Grapple Fail");
                }
            }
            else
            { 
                grappleCount = 1;
                //grapplePoint = cam.position + cam.forward * maxGrappleDistance;

                Invoke(nameof(StopGrapple), grappleDelayTime);
                Debug.Log("Grapple Fail");
            }

            lr.enabled = true;
            lr.positionCount = 2;
            lr.SetPosition(1, grapplePoint);
        }
    }

    private bool HasLayerMask(GameObject RequestingObject, LayerMask RequestingMask) => (RequestingMask.value & (1 << RequestingObject.layer)) != 0;
    //private void ExecuteGrapple()
    //{
    //    fovCam.DoFov(grappleFOV);
    //    pm.freeze = false;
    //    freezePlayer = false;
    //    pm.isDashing = false; // force end any ongoing dash
    //    //.activeGrapple = true;
    //    CancelInvoke(nameof(pm.ResetDash)); // cancel the delayed reset too
    //   // MoveToDestination(grapplePoint);
    //    //Invoke(nameof(StopGrapple), 1f);
    //}

    //private IEnumerator ApplyForceUntilDestinationReached(Vector3 Destination)
    //{
    //    pm.rb.useGravity = false;
       
    //    float Distance = Vector3.Distance(transform.position, Destination);

    //    while (Distance > 5f && grappling)
    //    {
    //        Vector3 Direction = (Destination - pm.transform.position).normalized;
    //        pm.rb.AddForce(Direction * grappleSpeed, ForceMode.Force);
    //        //pm.rb.AddForce(-Physics.gravity/1.75f * pm.rb.mass, ForceMode.Force);
    //        Distance = Vector3.Distance(pm.transform.position, Destination);
    //        yield return null;
    //    }
    ////    pm.rb.useGravity = true;
    ////}

    //private void MoveToDestination(Vector3 Destination)
    //{
    //    StartCoroutine(ApplyForceUntilDestinationReached(Destination));
    //}
    public void StopGrapple()
    {
       // fovCam.DoFov(pm.normalFOV);
        Debug.Log("Stop Grapple");
        pm.freeze = false;
        freezePlayer = false;
        grappling = false;
        pm.activeGrapple = false;
        isApplyingGrappleForce = false;
        grapplingCdTimer = grapplingCd;

        lr.positionCount = 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(cam.position, grapplePoint);
    }
}
