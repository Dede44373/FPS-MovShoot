using System.Collections;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
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
    public void TakeDamage (int damage)
    {
        FindAnyObjectByType<Hitstop>().Stop(hitstopDuration);
        StartCoroutine(Invulnerability());
        hurtPart.Play();

        Debug.Log("Enemy Damaged" + health);
        health -= damage;

        if(health <= 0 )
        {
            Die();
        }

    }

    private void Die()
    {

            GetComponent<LootBag>().InstantiateLoot(transform.position); 

            FindAnyObjectByType<Hitstop>().Stop(hitstopDuration);
            deathPart.Play();
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
