using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] AudioSource pickupSFX;
    public GameObject ghost;
    public float moneyValue;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Instantiate(ghost, transform.position, Quaternion.identity);
            Debug.Log("picked up loot");
            FindFirstObjectByType<MoneyHUD>().totalMoney += moneyValue;
            Destroy(gameObject);

        }
    }
}
