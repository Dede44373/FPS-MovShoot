using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public int health;
    public float hitstopDuration;
    public float hitstopDeathDuration;

    [Header("iFrames")]
    public LayerMask invulLayer;
    public LayerMask playerLayer;
    [SerializeField] private float invulDuration;
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;
    public Rigidbody rb;
    public float knockForce;

    [Header("Particles")]
    public ParticleSystem hurtPart;
    public ParticleSystem deathPart;

    [Header("Audio")]
    [SerializeField] AudioSource hitSFX;
    float pitchVar = 0.05f;

    [SerializeField] private HealthBarUI healthBar;

    public void TakeDamage(int damage)
    {
        FindAnyObjectByType<Hitstop>().Stop(hitstopDuration);
        StartCoroutine(Invulnerability());
        //hurtPart.Play();

        Debug.Log("Player Damaged" + health);
        health -= damage;
        healthBar.setHealth(health);

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
        SceneManager.LoadScene("Test");
        //SceneManager.GetActiveScene().ToString()

    }
    private IEnumerator Invulnerability()
    {
        gameObject.layer = invulLayer;
        //invulnerability duration
        for (int i = 0; i < numberOfFlashes; i++)
        {
            //spriteRend.color = new Color(1, 0.8f, 0.8f, 0.9f);
            yield return new WaitForSeconds(0.01f);
            //spriteRend.color = Color.white;
            yield return new WaitForSeconds(0.01f);
        }
        gameObject.layer = playerLayer;
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
