using System.Collections;
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

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    public float grappleCount;

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
    }

    private void HandleGrappleStop(InputAction.CallbackContext ctx)
    {
        if(grappling == true)
        StopGrapple();
    }

    private void StartGrapple()
    {
        
        if (grapplingCdTimer > 0) return;
        if (grappleCount >= 1)
        {
            grappleCount--;
            Debug.Log("Check Grapple");

            grappling = true;
            pm.freeze = true;

            RaycastHit hit;
            if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance))
            {
                detectedGO = hit.transform.gameObject;
                grapplePoint = hit.point;

                Invoke(nameof(ExecuteGrapple), grappleDelayTime);
                Debug.Log("Start Grapple");
            }
            else
            {
                grapplePoint = cam.position + cam.forward * maxGrappleDistance;

                Invoke(nameof(StopGrapple), grappleDelayTime);
                Debug.Log("Grapple Fail");
            }

            lr.enabled = true;
            lr.SetPosition(1, grapplePoint);
        }
    }

    private bool HasLayerMask(GameObject RequestingObject, LayerMask RequestingMask) => (RequestingMask.value & (1 << RequestingObject.layer)) != 0;
    private void ExecuteGrapple()
    {
        pm.freeze = false;

        if (pm.isDashing == true) return;
        else
        {
            if (HasLayerMask(detectedGO, grappleable))
            {
                
                Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y -1, transform.position.z);

                float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
                float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

                if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;
                //pm.JumpToPosition(grapplePoint, highestPointOnArc);
                MoveToDestination(grapplePoint);

                Invoke(nameof(StopGrapple), 1f);
            }
            else
            {
                StopGrapple();
            }
        }
    }

    private IEnumerator ApplyForceUntilDestinationReached(Vector3 Destination)
    {
        pm.rb.useGravity = false;
       
        float Distance = Vector3.Distance(transform.position, Destination);

        while (Distance > 5f && grappling)
        {
            Vector3 Direction = (Destination - pm.transform.position).normalized;
            pm.rb.AddForce(Direction * 100f, ForceMode.Force);
            pm.rb.AddForce(-Physics.gravity/2 * pm.rb.mass, ForceMode.Force);
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
        Debug.Log("Stop Grapple");
        pm.freeze = false;
        grappling = false;

        grapplingCdTimer = grapplingCd;

        lr.enabled = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(cam.position, grapplePoint);
    }
}
