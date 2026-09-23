using UnityEngine;

public class LevelHazards : MonoBehaviour
{
    public int damage;
    
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth player = collision.gameObject.GetComponentInParent<PlayerHealth>();
            Debug.Log(player);

            player.TakeDamage(damage);
        }
    }
}
