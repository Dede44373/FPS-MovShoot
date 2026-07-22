using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwing : MonoBehaviour
{
    [Header("References")]
    public UserInputs controls;
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask Grappleable;
    public PlayerMovement pm;

    [Header("Swinging")]
    public float maxSwingDistance =25f;
    private Vector3 currentGrapplePosition;
    private Vector3 swingPoint;
    private SpringJoint joint;

    [SerializeField] private float grappleSpeed;

    [Header("Swing Movement")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;

    private void OnEnable()
    {
        controls = UserInputManager.Instance.Controls;
        controls.Player.Interact.started += HandleStartSwing;
        controls.Player.Interact.canceled += HandleStopSwing;
    }

    private void OnDisable()
    {
        controls.Player.Interact.started -= HandleStartSwing;
        controls.Player.Interact.canceled -= HandleStopSwing;
    }

    private void Update()
    {
        CheckForSwingPoints();
        //if (joint != null) SwingMovement();
    }
    private void LateUpdate()
    {
        DrawRope();

    }
    private void DrawRope()
    {
        //if not grappling, dont draw rope
        if (!joint)
        {
            lr.enabled = false;
            return;
        }
        lr.enabled = true;
       currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }
    private void HandleStartSwing(InputAction.CallbackContext ctx)
    {

        //Grappling combining function
        Debug.Log("Swinging");
        StartSwing();
    }

    private void HandleStopSwing(InputAction.CallbackContext ctx)
    {
        StopSwing();
    }

    void StartSwing()
    {
        if (predictionHit.point == Vector3.zero) return;

        if (TryGetComponent<PlayerGrapple>(out PlayerGrapple GrappleScript))
        {
            GrappleScript.StopGrapple();
        }

        // pm.ResetRestrictions();
        pm.swinging = true;
        swingPoint = predictionHit.point;
        currentGrapplePosition = gunTip.position;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        // the distance grapple will try to keep from grapple point.
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        //customize these values

        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        lr.positionCount = 2;
    }

    public void StopSwing()
    {
        pm.swinging = false;

        lr.positionCount = 0;
        Destroy(joint);
    }
    private void CheckForSwingPoints()
    {
        if (joint != null) return;
        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward, 
                            out sphereCastHit, maxSwingDistance, Grappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward,
                            out raycastHit, maxSwingDistance, Grappleable);

        Vector3 realHitPoint;
        //Option 1 - Direct Hit
        if(raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;

        //Option 2 - Indirect (predicted) Hit
        else if (sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;

        //Option 3 - Miss
        else realHitPoint = Vector3.zero;

        //realHitPoint found
        if(realHitPoint != Vector3.zero)
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
        Gizmos.DrawLine(cam.position, swingPoint);
    }

    // 1. projectiles not sticking and going through enemies
    // 2. moon jumps with grapple on slope
    // 3. cant cancel grapple with dash
    // 4. After sliding wont return to walking if not holding sprint, just defaults to sprinting

    // if(hit.transform.position.x < transform.position.x)
    // {StopSwinging()}
    //  + rb.linearVelocity
}
