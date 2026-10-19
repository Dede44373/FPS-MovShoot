using UnityEngine;
using UnityEngine.ProBuilder;

public class RangedEnemyAnimation : MonoBehaviour
{
    public float projectileSpeed, projectileUpForce;
    public GameObject projectile;
    public Transform attackPoint;
    public Transform player;
    public void Shoot()
    {
        Vector3 pos = player.transform.position;
        Rigidbody rb = Instantiate(projectile, attackPoint.position, attackPoint.localRotation).GetComponent<Rigidbody>();
        rb.transform.LookAt(pos);
        rb.transform.rotation *= Quaternion.Euler(90, 0, 0);
        //rb.transform.localEulerAngles = new Vector3(90, 0, 0);

        // 
        rb.AddForce(attackPoint.forward * projectileSpeed, ForceMode.Impulse);
        //This one for dropoff
        rb.AddForce(attackPoint.up * projectileUpForce, ForceMode.Impulse);
    }
}
