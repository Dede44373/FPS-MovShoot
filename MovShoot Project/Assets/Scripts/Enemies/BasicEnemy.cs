using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int health;

    public void TakeDamage (int damage)
    {
        health -= damage;

        if(health <= 0 )
            Destroy(gameObject);

    }
}
