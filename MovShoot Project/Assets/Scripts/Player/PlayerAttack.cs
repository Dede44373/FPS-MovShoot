  using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using CameraShake;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] BounceShake.Params shakeParams;

    [Header("Attacking Stats")]
    private WaitForSeconds attackDelayWait;
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
        attackDelayWait = new WaitForSeconds(attackDelay);
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
    }
    private void OnDisable()
    {
        Controls.Player.Attack.performed -= AttackStart;
    }

    private async void AttackStart(InputAction.CallbackContext ctx)
    {
        if (inAttack) return;

        print("<color=blue>GROundSLAMMIN</color>");
        
        float angle = Vector3.Angle(-player.transform.up, cam.transform.forward);
        print($"<color=purple>Angle: {angle}</color>");

        // Ground Slam
        if (!pm.grounded && angle < 35f)
        {
            Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, 100f, ground);
            print(hit.transform.name);
            print($"Raycast hit{hit.point}");
            
            StartCoroutine(Slam(cam.transform.forward));
            //if (pm.grounded == true)
            //{
            //    new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            //}
        }

        // Heavy Attack
        else
        {
            float Elapsed = 0f;
            var Control = ctx.control;
            while (Control.IsPressed())
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);
                Elapsed += Time.deltaTime;

                if (Elapsed > tapThreshold && !inAttack)
                {
                    inAttack = true;
                    anim.Play("Armature_Punch_Heavy_Charge_1");
                }
            }

            if (Elapsed > tapThreshold && heavyCharged)
            {
                StartCoroutine(HeavyAttack());
            }

            // Light Attack
            else
            {
                StartCoroutine(LightAttack());
            }

        }
    }

    private IEnumerator HeavyAttack()
    {
        damage = heavyDamage;
        anim.Play("Armature_Punch_Heavy_Attack_1");

        SoundManager.PlaySound(SoundType.Fist_Heavy);
        yield return attackDelayWait;
        inAttack = false;
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
        yield return attackDelayWait;
        inAttack = false;
    }
    private IEnumerator Slam(Vector3 slamDirection)
    {
        inAttack = true;

        while (!pm.grounded)
        {
            rb.AddForce(slamDirection * slamSpeed, ForceMode.Force);
            yield return null;
        }

        SoundManager.PlaySound(SoundType.Ground_Slam);
        CameraShaker.Presets.Explosion3D();

        if (Physics.Raycast(player.transform.position, -player.transform.up, out RaycastHit hit, 2f))
        {
            Vector3 Normal = hit.normal;
            Vector3 Position = hit.point + Normal * 0.01f;

            Instantiate(slamParticles, Position, Quaternion.LookRotation(-Normal * 0.01f, Vector3.up));
        }

        Collider[] hitEnemies = Physics.OverlapSphere(player.transform.position, slamRange, enemyLayer);
        foreach (Collider Enemy in hitEnemies)
        {
            if (!Enemy.TryGetComponent(out EnemyHealth health)) continue;

            health.TakeDamage(slamDamage);
        }
        yield return attackDelayWait;
        inAttack = false;
    }

    #region Animator Methods

    public void HeavyCharged()
    {
        heavyCharged = true;
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

    #endregion

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
