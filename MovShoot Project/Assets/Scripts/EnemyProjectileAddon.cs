using System.Collections;
using UnityEngine;

public class EnemyProjectileAddon : MonoBehaviour
{
    private Rigidbody rb;
    GameObject raycastObj;
    public Collider col;

    [SerializeField] int damage;
    private bool targetHit;
    private bool moving;

    public LayerMask layerMask;
    Vector3 hitPos;


    private void Start()
    {
        moving = true;
        StartCoroutine(Movement());
        rb = GetComponent<Rigidbody>();
    }

    IEnumerator Movement()
    {
        while (moving)
        {

            bool passedThrough = RaycastCheck();
            if (passedThrough)
            {
                moving = false;
                SnapObject();
            }
            else
            {
                yield return null;
            }
        }

    }
    void SnapObject()
    {
        transform.position = hitPos;

    }

    bool RaycastCheck()
    {
        Debug.DrawRay(transform.position, transform.up * 100f);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up, out hit, Mathf.Infinity, layerMask))
        {
            raycastObj = hit.collider.gameObject;
            hitPos = hit.point;
            //GetComponent<CapsuleCollider>().enabled = false;
            return false;
        }
        else
        {
            if (raycastObj != null)
            {
                raycastObj = null;
                Debug.Log("passed through something");
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //make sure only to stick to the first target you hit
        if (targetHit)
            return;
        else
            targetHit = true;

        //rb.isKinematic = true;

        //checks if you hit an enemy
        if (collision.gameObject.GetComponent<PlayerHealth>() != null)
        {
            print("FBIWEFWIFWEIFJWKFWEIJKFW");
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();

            //rb.isKinematic = true;
            player.TakeDamage(damage);
            transform.SetParent(collision.transform, true);
            Destroy(gameObject);
            //GetComponent<CapsuleCollider>().enabled = false;  
            //col.isTrigger = true;
        }



        //make sure projectiles sticks to surface
        rb.isKinematic = true;

        //makes sure projectile moves with target


        transform.SetParent(collision.transform);
    }
}
