using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Reference")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;
    public UserInputs Controls;

    [Header("Settings")]
    public int totalThrows;
    public float throwCooldown;
    public float raycastRange;

    [Header("Throwing")]
    public float throwForce;
    public float throwUpwardForce;

    bool readyToThrow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readyToThrow = true;    
    }
    private void OnEnable()
    {
        Controls = UserInputManager.Instance.Controls;
        Controls.Player.Attack.performed += HandleAttackStart;
    }
    private void OnDisable()
    {
        Controls.Player.Attack.performed -= HandleAttackStart;
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
        readyToThrow = false;

        // clone object to throw
        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        // get rigidbody component
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        // calculate direction
        Vector3 forceDirection = cam.transform.forward;

        RaycastHit hit;

        if(Physics.Raycast(cam.position, cam.forward, out hit, raycastRange))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        // add force
        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        totalThrows--;

        //implement throwCooldown
        Invoke(nameof(ResetThrow), throwCooldown);

    }

    void ResetThrow()
    {
        readyToThrow = true;

    }
}
