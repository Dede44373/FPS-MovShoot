using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] private MeshFilter originalMesh;
    [SerializeField] private Mesh newMesh;
    [SerializeField] AudioSource pickupSFX;
    [SerializeField] private MeshCollider col;
    public GameObject ghost;
    public int moneyValue = 1;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Instantiate(ghost, transform.position, Quaternion.identity);
            //originalMesh.mesh = newMesh;
            Debug.Log("picked up loot");
            MoneyHUD.instance.AddMoney(moneyValue);
            Destroy(gameObject);

        }
    }
}
