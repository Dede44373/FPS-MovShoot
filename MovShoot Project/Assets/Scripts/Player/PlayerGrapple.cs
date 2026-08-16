using System.Collections;
using System.Collections.Generic;
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
    public bool grappling;

    [Header("Swinging")]
    float holdTime;
    public float holdActivateTime;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    private bool isApplyingGrappleForce;

    public bool swinging;

    public bool freezePlayer;

    private bool startGrapple;
    private bool startSwing;
    public float TapThreshold = 0.5f;
    public bool grappleActivate;

    [Header("Rope")]
    private float ropeLength;
    private bool isRopeInTension;
    [SerializeField] private float minRopeLength;

    private float reelInSpeed;
    private bool isReelingIn;
    [SerializeField] private float reelInAcceleration;

    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;


    private HashSet<string> KeyboardControls = new()
    {
        "W",
        "A",
        "S",
        "D"
    };


    // Update is called once per frame
    void Update()
    {
        //For Sphere casting
        CheckForSwingPoints();

        if (swinging)
        {
            isApplyingGrappleForce = true;

            isRopeInTension = ropeLength * ropeLength < (grapplePoint - rb.position).sqrMagnitude;
        }
        else
        {
            isApplyingGrappleForce = false;
            //pm.enabled = true;
        }

        if (controls.Player.Move.IsPressed())
        {
            if (KeyboardControls.Contains(controls.Player.Move.activeControl.displayName)) // dont use this if u plan to use all 4 keys (WASD)
            {
                //print("Pressed valid input");
            }
        }

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;


    }

    private void LateUpdate()
    {
        if (swinging == true)
        {
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, grapplePoint);
        }
    }

    private void FixedUpdate()
    {
        if (isApplyingGrappleForce)
        {
            ApplySwingingForces();
            if (Vector3.Dot(rb.linearVelocity, grapplePoint - rb.position) <= 0 && isRopeInTension)
                TugPlayer();

            if (ropeLength > minRopeLength && isReelingIn)
            {
                reelInSpeed += reelInAcceleration * Time.fixedDeltaTime;
                ropeLength -= reelInSpeed * Time.fixedDeltaTime;
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
    private async void HandleGrappleStart(InputAction.CallbackContext ctx)
    {

        float Elapsed = 0f;
        var Control = ctx.control;

        while (Control.IsPressed())
        {
            await Awaitable.NextFrameAsync(destroyCancellationToken);
            Elapsed += Time.deltaTime;

            if (Elapsed > TapThreshold)
            {
                StartSwing();
                //print("Holding");
                //isApplyingGrappleForce = true;
            }
        }
    
        print(Elapsed);

        if (Elapsed <= TapThreshold)
        {
            grappleActivate = true;
            //print("Tapped");
            StartSwing();
        }
    }

    private void HandleGrappleStop(InputAction.CallbackContext ctx)
    {
        if (swinging || grappling == true)
            StopGrapple();
    }

    //maths for calculating swinging velocity
    private void ApplySwingingForces()
    {
        //calculating theta
        Vector3 direction = (grapplePoint - rb.position).normalized;
        float theta = Vector3.Angle(direction, Vector3.up) * Mathf.Deg2Rad;

        float centripetalAcceleration = rb.linearVelocity.sqrMagnitude / ropeLength;
        Vector3 tension = rb.mass * (centripetalAcceleration + Physics.gravity.magnitude * Mathf.Cos(theta)) * direction;

        if (isRopeInTension)
        {
            if (isReelingIn)
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

    private void StartSwing()
    {
        if (predictionHit.point == Vector3.zero) return;
        //pm.enabled = false;
        if (swinging) return;
        // makes sure that if you're grappling or swinging it stops it before starting a new grapple/or prevents it
        if (grapplingCdTimer > 0) return;
        //GetComponent<PlayerSwing>().StopSwing();
        rb.linearDamping = 0;

        //if you have more than 1 grapple count left you can grapple
        if (grappleCount >= 1)
        {
            grappleCount--;
            // freezes all the player's movements and velocities
            // creates a raycast from the camera position, forwards (where you're looking), then stores the value in 'hit', and max distance it can travel.

            Vector3 direction = predictionHit.point - cam.transform.position;
            grapplePoint = predictionHit.point;

            ropeLength = (grapplePoint - rb.position).magnitude;
            isReelingIn = true;
            reelInSpeed = 0;

            pm.activeGrapple = true;
            // Invoke(nameof(ExecuteGrapple), grappleDelayTime);

            if (!grappleActivate)
            {
                StartSwing();
                swinging = true;
                isRopeInTension = ropeLength * ropeLength < (grapplePoint - rb.position).sqrMagnitude;
            }
            else
            {
                Debug.Log("Start Grapple");
                grappling = true;
                ExecuteGrapple();
            }

                #region OldSwing
                /*
                if (Physics.Raycast(cam.position, direction, out hit, maxGrappleDistance))
                {
                detectedGO = hit.transform.gameObject;
                grapplePoint = predictionHit.point;

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
                return;
                }
                }
                else
                { 
                grappleCount = 1;
                //grapplePoint = cam.position + cam.forward * maxGrappleDistance;

                Invoke(nameof(StopGrapple), grappleDelayTime);
                Debug.Log("Grapple Fail");
                return;


                }
                */
                #endregion


            lr.enabled = true;
            lr.positionCount = 2;
            lr.SetPosition(1, grapplePoint);

        }
    }

    private bool HasLayerMask(GameObject RequestingObject, LayerMask RequestingMask) => (RequestingMask.value & (1 << RequestingObject.layer)) != 0;
    private void ExecuteGrapple()
    {
        fovCam.DoFov(grappleFOV);
        pm.col.sharedMaterial = pm.slideMat;
        pm.freeze = false;
        freezePlayer = false;
        pm.isDashing = false; // force end any ongoing dash
        //.activeGrapple = true;
        CancelInvoke(nameof(pm.ResetDash)); // cancel the delayed reset too
        MoveToDestination(grapplePoint);
        Invoke(nameof(StopGrapple), 1f);
    }

    private IEnumerator ApplyForceUntilDestinationReached(Vector3 Destination)
    {
        pm.rb.useGravity = false;

        float Distance = Vector3.Distance(transform.position, Destination);

        while (Distance > 5f && grappling)
        {
            Vector3 Direction = (Destination - pm.transform.position).normalized;
            pm.rb.AddForce(Direction * grappleSpeed, ForceMode.Force);
            //pm.rb.AddForce(-Physics.gravity/1.75f * pm.rb.mass, ForceMode.Force);
            Distance = Vector3.Distance(pm.transform.position, Destination);
            yield return null;
        }
        pm.rb.useGravity = true;
    }

    private void MoveToDestination(Vector3 Destination)
    {
        StartCoroutine(ApplyForceUntilDestinationReached(Destination));
    }
    public void StopGrapple()
    {
        fovCam.DoFov(pm.normalFOV);
        Debug.Log("Stop Grapple");
        pm.col.sharedMaterial = pm.groundMat;
        pm.freeze = false;
        freezePlayer = false;

        swinging = false;
        grappling = false;
        grappleActivate = false;

        pm.activeGrapple = false;
        isApplyingGrappleForce = false;
        grapplingCdTimer = grapplingCd;

        lr.positionCount = 0;
    }

    private void CheckForSwingPoints()
    {
        if (swinging)
        {
            Debug.Log("<color=red>Grappling</color>");
            return;
        }

        //Debug.Log("<color=green>Check for Swing</color>");

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward,
            out sphereCastHit, maxGrappleDistance, grappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward,
            out raycastHit, maxGrappleDistance, grappleable);

        Vector3 realHitPoint;
        //Option 1 - Direct Hit
        if (raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;

        //Option 2 - Indirect (predicted) Hit
        else if (sphereCastHit.point != Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
            Debug.Log("Sphere casted");

        }

        //Option 3 - Miss
        else
        {
            realHitPoint = Vector3.zero;
        }

        //realHitPoint found
        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        //realHitPoint not found
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(cam.position, grapplePoint);
    }
}
