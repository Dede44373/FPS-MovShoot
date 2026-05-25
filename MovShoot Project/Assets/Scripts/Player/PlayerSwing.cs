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
        Debug.Log("Swinging");
        StartSwing();
    }

    private void HandleStopSwing(InputAction.CallbackContext ctx)
    {
        StopSwing();
    }

    void StartSwing()
    {
        pm.swinging = true; 

        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, Grappleable))
        {
            swingPoint = hit.point;
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
    }

    void StopSwing()
    {
        pm.swinging = false;

        lr.positionCount = 0;
        Destroy(joint);
    }



    // 1. projectiles not sticking and going through enemies
    // 2. moon jumps with grapple on slope
    // 3. cant cancel grapple with dash

    // if(hit.transform.position.x < transform.position.x)
    // {StopSwinging()}
    //  + rb.linearVelocity
}
