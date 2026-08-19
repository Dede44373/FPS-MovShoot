using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class BasicEnemy : MonoBehaviour, IKnockable
{
    [Header("Stats")]
    public int health;
    public float hitstopDuration;
    public float hitstopDeathDuration;
    public Rigidbody rb;
    public float knockForce;

    [Header("iFrames")]
    [SerializeField] private float invulDuration;
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;
    [SerializeField] bool hasLoot;

    [Header("Particles")]
    public ParticleSystem hurtPart;
    public ParticleSystem deathPart;

    [Header("Audio")]
    [SerializeField] AudioSource hitSFX;
    float pitchVar = 0.05f;
    public void TakeDamage (int damage)
    {
        FindAnyObjectByType<Hitstop>().Stop(hitstopDuration);
        StartCoroutine(Invulnerability());
        hurtPart.Play();

        Debug.Log("Enemy Damaged" + health);
        health -= damage;

        float randomPitch = Random.Range(1f - pitchVar, 1f + pitchVar);
        hitSFX.pitch =  randomPitch;   
        hitSFX.Play();

        if(health <= 0 )
        {
            Die();   
        }

    }

    private void Die()
    {
            Instantiate(deathPart, transform.position, Quaternion.identity);
        if(hasLoot)
            GetComponent<LootBag>().InstantiateLoot(transform.position); 

            Destroy(gameObject);

    }
    private IEnumerator Invulnerability()
    {

        //invulnerability duration
        for (int i = 0; i < numberOfFlashes; i++)
        {
            //spriteRend.color = new Color(1, 0.8f, 0.8f, 0.9f);
            yield return new WaitForSeconds(0.01f);
            //spriteRend.color = Color.white;
            yield return new WaitForSeconds(0.01f);
        }
        StopAllCoroutines();
    }

    public void Knockback(Transform executionSource)
    {
        KnockbackEntity(executionSource);
    }

    public void KnockbackEntity(Transform executionSource)
    {
        if (rb == null)
            return;

        Vector3 dir = (transform.position - executionSource.transform.position).normalized;
        rb.AddForce(dir * knockForce, ForceMode.Impulse);
    }
}
