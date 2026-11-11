using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using CameraShake;
using System;
using UnityEngine.ProBuilder.MeshOperations;
public class PlayerThrow : MonoBehaviour
{
    [SerializeField] KickShake.Params shakeParams;
    [SerializeField] Displacement displacement;

    [Header("Reference")]
    public PlayerHealth health;
    public Transform camTransform;
    public PlayerCam cam;
    public Transform attackPoint;
    public GameObject objectToThrow;
    public UserInputs Controls;
    public PlayerMovementData data;
    public Rigidbody rb;

    [Header("Settings")]
    public int totalThrows;
    public float throwCooldown;
    public float raycastRange;
    public float throwZoomOut;
    public float throwZoomCooldown;
    public float recoilForce;

    [Header("Throwing")]
    public float throwForce;
    public float throwUpwardForce;

    public bool readyToThrow;
    public bool active;

    [Header("Parrying")]
    public bool parryActive;
    public float parryWindow;
    public float parryCooldown = 1f;
    private float parryCooldownTimer;
    private float parryTime;
    public float TapThreshold = 0.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readyToThrow = true;  
        parryActive = false;
        parryTime = parryWindow;
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

    public async void HandleAttackStart(InputAction.CallbackContext ctx)
    {
        print("Throwing");
        float Elapsed = 0f;
        var Control = ctx.control;
        
        //held down (shooting)
        while (Control.IsPressed())
        {
            await Awaitable.NextFrameAsync(destroyCancellationToken);
            Elapsed += Time.deltaTime;

            if (Elapsed > TapThreshold)
            {
                if (readyToThrow && totalThrows > 0)
                {
                    Throw();
                }
            }
        }

        //Tapped button (parry)
        if (Elapsed <= TapThreshold)
        {
            Parry();
            print("Huge");
        }
    }

    public async void Parry()
    {
        if (parryActive) return;
        //if (parryTime >= 0)
        //{
        //    parryTime -= Time.deltaTime;
        //    return;
        //}
        parryActive = true;
        while (parryTime >= 0)
        {
            print("Huge Parrying");
            await Awaitable.NextFrameAsync(destroyCancellationToken);
            parryTime -= Time.deltaTime;

            if (parryActive && parryTime <= 0)
            {
                ParryReset();
            }
        }
    }
    public async void ParryReset()
    {
        parryTime = parryWindow;
        parryActive = false;

        //cooldown Timer;
        float Elapsed = 0f;

        while (Elapsed < parryCooldownTimer && !parryActive)
        {
            await Awaitable.NextFrameAsync(destroyCancellationToken);
            Elapsed += Time.deltaTime;

            if (Elapsed > parryCooldownTimer)
            {
                print("Huge Parry Reset");
                parryCooldownTimer = parryCooldown;
            }
        }
    }
    public void Throw()
    {
        if (!active || !readyToThrow) return;
        readyToThrow = false;

        rb.AddForce(-camTransform.forward * recoilForce, ForceMode.Impulse);
        
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

        SoundManager.PlaySound(SoundType.Fire_Projectile);
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
