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

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    public bool grappling;

    // Update is called once per frame
    void Update()
    {
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;

    }

    private void LateUpdate()
    {
        if(grappling == true)
            lr.SetPosition(0, gunTip.position);
    }

    private void OnEnable()
    {
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
    }

    private void HandleGrappleStop(InputAction.CallbackContext ctx)
    {
        StopGrapple();
    }

    private void StartGrapple()
    {
        Debug.Log("Started Grapple");
        if (grapplingCdTimer > 0) return;

        grappling = true;

        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, grappleable))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        pm.JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    private void StopGrapple()
    {
        grappling = false;

        grapplingCdTimer = grapplingCd;

        lr.enabled = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(cam.position, grapplePoint);
    }
}
