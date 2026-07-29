using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public int health;
    public float hitstopDuration;
    public float hitstopDeathDuration;

    [Header("iFrames")]
    [SerializeField] private float invulDuration;
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;

    [Header("Particles")]
    public ParticleSystem hurtPart;
    public ParticleSystem deathPart;

    [Header("Audio")]
    [SerializeField] AudioSource hitSFX;
    float pitchVar = 0.05f;
    public void TakeDamage(int damage)
    {
        FindAnyObjectByType<Hitstop>().Stop(hitstopDuration);
        StartCoroutine(Invulnerability());
        hurtPart.Play();

        Debug.Log("Player Damaged" + health);
        health -= damage;

        //float randomPitch = Random.Range(1f - pitchVar, 1f + pitchVar);
        //hitSFX.pitch = randomPitch;
        //hitSFX.Play();

        if (health <= 0)
        {
            Die();
        }

    }

    private void Die()
    {
        Instantiate(deathPart, transform.position, Quaternion.identity);
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

}
