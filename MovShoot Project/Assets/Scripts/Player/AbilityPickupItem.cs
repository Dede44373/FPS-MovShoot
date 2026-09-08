using UnityEngine;

public class AbilityPickupItem : MonoBehaviour
{
    public GameObject ghost;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            print("<color=green>Ability unlocked yeah</color>");
            Instantiate(ghost, transform.position, Quaternion.identity);
            FindFirstObjectByType<ActiveAbilityManager>().ActivateAbility(gameObject);
            Destroy(gameObject);

        }
    }

}
