using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using CameraShake;
using UnityEngine.Rendering;
using DG.Tweening;

public class PlayerHealth : MonoBehaviour, IKnockable
{
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

    private void Start()
    {
        currentHealth = maxHealth;
        Debug.Log(SceneManager.GetActiveScene().name.ToString());
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
        
        respawn();
        //SceneManager.
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
        print("k");
        if (rb == null)
            return;
        print("has a rigidbody");

        Vector3 dir = (transform.position - executionSource.transform.position).normalized;
        rb.AddForce(dir * knockForce, ForceMode.Impulse);
    }
}
