using UnityEngine;

public class EnemyAnimatorFunctions : MonoBehaviour
{
    public Collider coll;
    public Rigidbody rb;
    public EnemyAI enemy;

    public void EnableAttackCollider()
    {
        coll.enabled = true;
    }

    public void DisableAttackCollider()
    {
        coll.enabled = false;
    }
    public void DashFowards()
    {
        rb.AddForce(transform.forward * enemy.dashForce, ForceMode.Impulse);

    }

}
