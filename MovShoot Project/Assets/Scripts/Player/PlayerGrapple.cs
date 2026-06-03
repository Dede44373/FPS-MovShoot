using System.Collections;
using DG.Tweening;
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

    public bool grappling;

    public bool freezePlayer;

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
        GetComponent<PlayerSwing>().StopSwing();

        if (grappleCount >= 1)
        {
            grappleCount--;
            Debug.Log("Check Grapple");

            grappling = true;
            pm.freeze = true;
            freezePlayer = true;

            RaycastHit hit;
            if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance))
            {
                detectedGO = hit.transform.gameObject;
                grapplePoint = hit.point;

                if (HasLayerMask(detectedGO, grappleable))
                {
                    Invoke(nameof(ExecuteGrapple), grappleDelayTime);
                    Debug.Log("Start Grapple");
                }
                else
                {
                    grappleCount = 1;
                    grapplePoint = cam.position + cam.forward * maxGrappleDistance;

                    Invoke(nameof(StopGrapple), grappleDelayTime);
                    Debug.Log("Grapple Fail");
                }
            }
            else
            {
                grappleCount = 1;
                grapplePoint = cam.position + cam.forward * maxGrappleDistance;

                Invoke(nameof(StopGrapple), grappleDelayTime);
                Debug.Log("Grapple Fail");
            }

            lr.enabled = true;
            lr.positionCount = 2;
            lr.SetPosition(1, grapplePoint);
        }
    }

    private bool HasLayerMask(GameObject RequestingObject, LayerMask RequestingMask) => (RequestingMask.value & (1 << RequestingObject.layer)) != 0;
    private void ExecuteGrapple()
    {
        fovCam.DoFov(grappleFOV);
        pm.freeze = false;
        freezePlayer = false;
        pm.isDashing = false; // force end any ongoing dash
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
            pm.rb.AddForce(-Physics.gravity/1.75f * pm.rb.mass, ForceMode.Force);
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
        pm.freeze = false;
        freezePlayer = false;
        grappling = false;

        grapplingCdTimer = grapplingCd;

        lr.positionCount = 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(cam.position, grapplePoint);
    }
}
