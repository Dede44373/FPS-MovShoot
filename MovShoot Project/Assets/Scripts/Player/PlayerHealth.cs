using System.Collections;
using CameraShake;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IKnockable
{
    public bool NormalRespawningPos;
    [Header("Stats")]
    public int currentHealth;
    public int maxHealth;
    public float hitstopDuration;
    public float hitstopDeathDuration;

    [Header("iFrames")]
    public LayerMask invulLayer;
    public LayerMask playerLayer;
    public bool isInvul;
    [SerializeField] private float invulDuration;
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;
    public Rigidbody rb;
    public float knockForce;

    [Header("Particles")]
    public ParticleSystem hurtPart;
    public ParticleSystem deathPart;
    public GameObject coll;
    public GameObject player;

    [Header("Audio")]
    [SerializeField] AudioSource hitSFX;
    float pitchVar = 0.05f;

    [SerializeField] private HealthBarUI healthBar;
    public Volume hurtVFX;
    public PlayerRespawn pr;
    public RespawnManager rm;
    public UserInputs Controls;
    private void Start()
    {
        currentHealth = maxHealth;
        if(NormalRespawningPos)
        {
            rb.MovePosition(RespawnManager.instance.spawnPoint);
            print(RespawnManager.instance.spawnPoint);
        }
        //Debug.Log(SceneManager.GetActiveScene().name.ToString());
    }
    private void OnEnable()
    {
        Controls = UserInputManager.Instance.Controls;
        Controls.Player.Suicide.performed += Suicide;

    }
    private void OnDisable()
    {
        Controls.Player.Suicide.performed -= Suicide;
    }

    private void Suicide(InputAction.CallbackContext ctx)
    {
        Die();
    }
    public void TakeDamage(int damage)
    {
         if (isInvul) return;
        FindAnyObjectByType<HitstopManager>().Stop(hitstopDuration);
        StartCoroutine(Invulnerability());

        //hurtPart.Play();

        Debug.Log("Player Damaged" + currentHealth);
        currentHealth -= damage;
        healthBar.setHealth(currentHealth);

        CameraShaker.Presets.Explosion3D();

        //float randomPitch = Random.Range(1f - pitchVar, 1f + pitchVar);
        //hitSFX.pitch = randomPitch;
        hitSFX.Play();

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    private void Die()
    {
        if(deathPart != null)
            Instantiate(deathPart, transform.position, Quaternion.identity);
        //RESPAWN


        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        //respawn();
        //SceneManager.
    }

    public void Heal(int healAmount)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += healAmount;
            healthBar.setHealth(currentHealth);
        }
    }

    private IEnumerator DeathRoutine()
    {
        HitstopManager.Instance.Stop(hitstopDeathDuration);
        yield return new WaitForSeconds(hitstopDeathDuration);

        HitstopManager.Instance.KillHitstop();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        respawn();
    }
    public void respawn()
    {
        print("yvcowibvwywbeyfvbwevhwovwhuiebodfwyfbewofyhwbeoywbeouyewbcoweuyfbwovuywbevuoyweobvjwebvyowebvyubwevbewvuywbyuvobewvbwuovybewovbwebvwoueyvbweovyewobvwuybwevwev");
        rm.ResetToCheckpoint(player);
        currentHealth = maxHealth;
        healthBar.setHealth(currentHealth);
    }
    private IEnumerator Invulnerability()
    {
        isInvul = true;
        hurtVFX.weight = 1;
        //DOTween.To(() => hurtVFX.weight, x => hurtVFX.weight = x, 1f, 1f);
        //coll.layer = 8;
        //Physics.IgnoreLayerCollision(6, 8, true);
        //invulnerability duration

        yield return new WaitForSeconds(invulDuration);

        //Physics.IgnoreLayerCollision(7, 20, false);
        //coll.layer = 6;

        DOTween.To(() => hurtVFX.weight, x => hurtVFX.weight = x, 0f, 1f);

        isInvul = false;


        StopAllCoroutines();
    }

    public void Knockback(Transform executionSource)
    {
        print("hey");
        KnockbackEntity(executionSource);
    }

    public void KnockbackEntity(Transform executionSource)
    {
        if (isInvul) return;
        print("k");
        if (rb == null)
            return;
        print("has a rigidbody");

        Vector3 dir = (transform.position - executionSource.transform.position).normalized;
        rb.AddForce(dir * knockForce, ForceMode.Impulse);
    }
}
