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
    public float tapThreshold;
    public bool heavyAttack;
    public float slamSpeed;
    public bool heavyCharged;

    public float stepDistance;

    [Header("References")]
    public UserInputs Controls;
    [SerializeField] private Collider coll;
    private Animator anim;
    public Transform player;
    public PlayerMovement pm;
    public Camera cam;
    public Rigidbody rb;
    public Mouse mouse { get; private set; }
    public LayerMask ground;


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
        Debug.DrawRay(player.transform.position, cam.transform.forward * 100f, Color.rebeccaPurple);

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

    private async void AttackStart(InputAction.CallbackContext ctx)
    {
        if (inAttack == false)
        {
                print("<color=blue>GROundSLAMMIN</color>");
            RaycastHit hit;
            if (!pm.grounded && Physics.Raycast(player.transform.position, cam.transform.forward, out hit, 100f, ground))
            {
                print(hit.transform.name);
                print($"Raycast hit{hit.point}");
                inAttack = true;
                while (!pm.grounded)
                {
                    rb.AddForce(-player.transform.up * slamSpeed, ForceMode.Force);
                    await Awaitable.NextFrameAsync(destroyCancellationToken);
                }
                inAttack = false;
            }
            else
            {
                float Elapsed = 0f;
                var Control = ctx.control;
                while (Control.IsPressed())
                {
                    await Awaitable.NextFrameAsync(destroyCancellationToken);
                    Elapsed += Time.deltaTime;

                    if (Elapsed > tapThreshold && !heavyAttack)
                    {
                        damage = 3;
                        inAttack = true;
                        heavyAttack = true;
                        anim.Play("Armature_Punch_Heavy_Charge_1");
                    }
                }

                if (Elapsed <= tapThreshold)
                {
                    StartCoroutine(LightAttack());

                }

            }

        }
    }
    // Walking
    private void AttackStop(InputAction.CallbackContext ctx)
    {
        if (heavyAttack && heavyCharged)
        {
            anim.Play("Armature_Punch_Heavy_Attack_1");

            StartCoroutine(HeavyAttack());
        }
        //else 
        //{
        //    StartCoroutine(LightAttack());
        //}
    }

    public void HeavyCharged()
    {
        heavyCharged = true;
    }
    private IEnumerator HeavyAttack()
    {

        SoundManager.PlaySound(SoundType.Fist_Heavy);
        yield return ad;
        inAttack = false;
        heavyAttack = false;
        heavyCharged = false;
    }
     private IEnumerator LightAttack()
    {
        damage = 2;
        print("ATTACCCCCCCK");
        inAttack = true;
        anim.Play("Armature_Punch_Light_1");
        anim.SetTrigger("Attack");
        SoundManager.PlaySound(SoundType.Fist_Melee);
        yield return ad;
        inAttack = false;
    }

    public void MoveForwards()
    {
        if (pm.currentState == PlayerMovement.MovementState.air)
        {
            return;
        }
        else
        {
            pm.freeze = true;
        }
        
        rb.AddForce(transform.forward * stepDistance, ForceMode.Impulse);
        // Rigidbody rb = GetComponent<Rigidbody>();
    }

    public void EnableWeaponCollider()
    {
        coll.enabled = true;
    }

    public void DisableWeaponCollider()
    {
        coll.enabled = false;
    }

    public void UnfreezePlayer()
    {
        pm.freeze = false;
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
                //if (heavyAttack)
                //    collision.gameObject.GetComponent<EnemyHealth>().knockForce = 100f;
                //else
                //    collision.gameObject.GetComponent<EnemyHealth>().knockForce = 40f;
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
