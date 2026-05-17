using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int health;

    public void TakeDamage (int damage)
    {
        Debug.Log("Enemy Damaged" + health);
        health -= damage;

        if(health <= 0 )
            Destroy(gameObject);

    }
}
