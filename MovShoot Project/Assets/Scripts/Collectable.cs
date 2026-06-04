using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] AudioSource pickupSFX;
    public GameObject ghost;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Instantiate(ghost, transform.position, Quaternion.identity);
            Destroy(gameObject);

        }
    }
}
