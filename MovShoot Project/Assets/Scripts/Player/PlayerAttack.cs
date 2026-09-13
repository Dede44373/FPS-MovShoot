using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using CameraShake;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] BounceShake.Params shakeParams;

    [Header("Attacking Stats")]
    private WaitForSeconds ad;
    public float attackDelay = 0.5f;
    [SerializeField] int damage;
    private bool targetHit;
    private bool inAttack;

    [Header("References")]
    public UserInputs Controls;
    [SerializeField] private Collider coll;
    private Animator anim;
    public Transform player;
    public Camera cam;
    public Mouse mouse { get; private set; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mouse = Mouse.current;
        anim = GetComponent<Animator>();
        ad = new WaitForSeconds(attackDelay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        Controls = UserInputManager.Instance.Controls;
        Controls.Player.Attack.performed += AttackStart;
        Controls.Player.Attack.canceled += AttackStop;
    }
    private void OnDisable()
    {
        Controls.Player.Attack.performed -= AttackStart;
        Controls.Player.Attack.canceled -= AttackStop;
    }

    private void AttackStart(InputAction.CallbackContext ctx)
    {
        if (inAttack == false) 
        StartCoroutine(Attacking());
    }
    // Walking
    private void AttackStop(InputAction.CallbackContext ctx)
    {
       
    }

     private IEnumerator Attacking()
    {
        inAttack = true;
        anim.SetTrigger("Attack");
        yield return ad;
        inAttack = false;
    }

    public void EnableWeaponCollider()
    {
        coll.enabled = true;
    }

    public void DisableWeaponCollider()
    {
        coll.enabled = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        print("detected a collision");
        //checks if you hit an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            print("has an enemy tag");
            EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
            enemy.TakeDamage(damage);
            print($"does enemy exist: {enemy != null}, if so it should've taken damage");

            CameraShaker.Presets.ShortShake3D();
            //CameraShaker.Shake(new BounceShake(shakeParams));

            Ray ray = cam.ScreenPointToRay(mouse.position.value);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                print("raycast hit something");
                IKnockable knockback = enemy.transform.GetComponent<IKnockable>();
                if (knockback != null)
                {
                    print("has a knockback script");
                    knockback.Knockback(player);
                }
            }
            else
            {
                print("raycast didnt hit shit");
            }
        }

    }
}
