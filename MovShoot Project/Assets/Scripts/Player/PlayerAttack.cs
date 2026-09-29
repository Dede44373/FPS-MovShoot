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
    private bool targetHit;
    private bool inAttack;
    public float tapThreshold;
    public bool heavyAttack;
    public float slamSpeed;
    public bool heavyCharged;

    public float slamRange;

    [Header("Damage")]
    private int damage;
    [SerializeField] int lightDamage;
    [SerializeField] int heavyDamage;
    [SerializeField] int slamDamage;


    [Header("Juice")]
    public float stepDistance;
    public float upForce;

    public float tiltDegree;
    public float tiltOriginal;
    public bool tilt;

    [Header("References")]
    public UserInputs Controls;
    [SerializeField] private Collider coll;
    private Animator anim;
    public Transform player;
    public PlayerMovement pm;
    public Camera cam;
    public PlayerCam pc;
    public Rigidbody rb;
    public ParticleSystem slamParticles;
    public Mouse mouse { get; private set; }
    public LayerMask ground, enemyLayer;


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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.transform.position, slamRange);
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
            Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 100f, ground);

            float angle = Vector3.Angle(-player.transform.up, cam.transform.forward);
            print($"<color=purple>Angle: {angle}</color>");
            if (!pm.grounded && angle < 35f)
            {
                print(hit.transform.name);
                print($"Raycast hit{hit.point}");
                inAttack = true;
                //StartCoroutine(LightAttack());
                while (!pm.grounded)    
                {
                    rb.AddForce(-player.transform.up * slamSpeed, ForceMode.Force);
                    await Awaitable.NextFrameAsync(destroyCancellationToken);
                }
                StartCoroutine(Slam());
                SoundManager.PlaySound(SoundType.Ground_Slam);
                //if (pm.grounded == true)
                //{
                //    new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                //}
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
                        damage = heavyDamage;
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
        damage = lightDamage;
        print("ATTACCCCCCCK");
        inAttack = true;
        anim.Play("Armature_Punch_Light_1");
        anim.SetTrigger("Attack");
        SoundManager.PlaySound(SoundType.Fist_Melee);
        yield return ad;
        inAttack = false;
    }
    private IEnumerator Slam()
    {
        damage = slamDamage;
        Instantiate(slamParticles, player.transform.position, Quaternion.identity);
        Collider[] hitEnemies = Physics.OverlapSphere(player.transform.position, slamRange, enemyLayer);
        foreach (Collider Enemy in hitEnemies)
        {
            Enemy.GetComponent<EnemyHealth>().TakeDamage(damage);

        }
        yield return ad;
        inAttack =false;
    }

    //movement/stepping
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
    public void MoveUpwards()
    {
        rb.AddForce(transform.up * upForce, ForceMode.Impulse);
    }

    // Juice
    public void TiltPlayer()
    {
       pc.DoTilt(tiltDegree);
    }
    public void UntiltPlayer()
    {
        pc.DoTilt(tiltOriginal);
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
