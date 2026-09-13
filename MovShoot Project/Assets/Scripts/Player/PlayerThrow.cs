using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using CameraShake;
using System;
public class PlayerThrow : MonoBehaviour
{
    [SerializeField] KickShake.Params shakeParams;
    [SerializeField] Displacement displacement;

    [Header("Reference")]
    public Transform camTransform;
    public PlayerCam cam;
    public Transform attackPoint;
    public GameObject objectToThrow;
    public UserInputs Controls;
    public PlayerMovementData data;

    [Header("Settings")]
    public int totalThrows;
    public float throwCooldown;
    public float raycastRange;
    public float throwZoomOut;
    public float throwZoomCooldown;

    [Header("Throwing")]
    public float throwForce;
    public float throwUpwardForce;

    bool readyToThrow;
    public bool active;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readyToThrow = true;    
    }
    private void OnEnable()
    {
        Controls = UserInputManager.Instance.Controls;
        Controls.Player.RangedAttack.performed += HandleAttackStart;
    }
    private void OnDisable()
    {
        Controls.Player.RangedAttack.performed -= HandleAttackStart;
    }

    private void HandleAttackStart(InputAction.CallbackContext ctx)
    {
        if (readyToThrow && totalThrows > 0)
        {
            Throw();
        }

    }
 

    // Update is called once per frame
    void Update()
    {
    }


    void Throw()
    {
        if (!active) return;
        readyToThrow = false;

        cam.DoFov(data.defaultFov - throwZoomOut);
        CameraShaker.Shake(new KickShake(shakeParams, displacement));

        // clone object to throw
        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, camTransform.rotation);

        // get rigidbody component
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        projectile.transform.rotation *= Quaternion.Euler(90f, 0, 0);

        // calculate direction
        Vector3 forceDirection = cam.transform.forward;

        RaycastHit hit;

        if(Physics.Raycast(camTransform.position, camTransform.forward, out hit, raycastRange))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        // add force
        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        totalThrows--;

        //implement throwCooldown
        Invoke(nameof(FovCooldown), throwZoomCooldown);
        Invoke(nameof(ResetThrow), throwCooldown);

    }

    void FovCooldown()
    {
        cam.DoFov(data.defaultFov + throwZoomOut);
    }

    void ResetThrow()
    {
        readyToThrow = true;

    }
}
