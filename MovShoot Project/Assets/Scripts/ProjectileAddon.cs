using UnityEngine;

public class ProjectileAddon : MonoBehaviour
{
    private Rigidbody rb;

    public int damage;
    private bool targetHit;

    private void Start()
    {
        rb= GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //make sure only to stick to the first target you hit
        if (targetHit)
            return;
        else
            targetHit = true;

        //checks if you hit an enemy
        if (collision.gameObject.GetComponent<BasicEnemy>() != null)
        {
            BasicEnemy enemy = collision.gameObject.GetComponent<BasicEnemy>();

            enemy.TakeDamage(damage);

            Destroy(gameObject);
        }

    
        //make sure projectiles sticks to surface
        rb.isKinematic = true;

        //makes sure projectile moves with target
        transform.SetParent(collision.transform);
    }
}
