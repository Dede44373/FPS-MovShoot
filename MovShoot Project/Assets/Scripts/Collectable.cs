using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] AudioSource pickupSFX;
    public GameObject ghost;
    public int moneyValue = 1;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Instantiate(ghost, transform.position, Quaternion.identity);
            Debug.Log("picked up loot");
            MoneyHUD.instance.AddMoney(moneyValue);
            Destroy(gameObject);

        }
    }
}
