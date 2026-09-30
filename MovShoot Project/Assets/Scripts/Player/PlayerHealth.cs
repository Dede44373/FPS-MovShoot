using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using CameraShake;
using UnityEngine.Rendering;
using DG.Tweening;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public int health;
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

    [Header("Audio")]
    [SerializeField] AudioSource hitSFX;
    float pitchVar = 0.05f;

    [SerializeField] private HealthBarUI healthBar;
    public Volume hurtVFX;
    public void TakeDamage(int damage)
    {
         if (isInvul) return;
        FindAnyObjectByType<Hitstop>().Stop(hitstopDuration);
        StartCoroutine(Invulnerability());

        //hurtPart.Play();

        Debug.Log("Player Damaged" + health);
        health -= damage;
        healthBar.setHealth(health);
        CameraShaker.Presets.Explosion3D();

        //float randomPitch = Random.Range(1f - pitchVar, 1f + pitchVar);
        //hitSFX.pitch = randomPitch;
        hitSFX.Play();

        if (health <= 0)
        {
            Die();
        }

    }

    private void Die()
    {
        if(deathPart != null)
            Instantiate(deathPart, transform.position, Quaternion.identity);

        //RESPAWN
        //SceneManager.LoadScene("Test");
        //SceneManager.GetActiveScene().ToString()

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
