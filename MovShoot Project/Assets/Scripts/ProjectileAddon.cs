using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileAddon : MonoBehaviour
{
    public PlayerCam cam;
    private Rigidbody rb;
    GameObject raycastObj;

    [SerializeField] int damage;
    private bool targetHit;
    private bool moving;

    public LayerMask layerMask;
    Vector3 hitPos;


    private void Start()
    {
        moving = true;
        StartCoroutine(Movement());
        rb= GetComponent<Rigidbody>();
    }

    IEnumerator Movement()
    {
        while (moving)
        {

            bool passedThrough = RaycastCheck();
            if(passedThrough)
            {
                moving =false;
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
        Debug.DrawRay(transform.position, hitPos);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask))
        {
            raycastObj = hit.collider.gameObject;
            hitPos = hit.point;
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
        if (collision.gameObject.CompareTag("Player")) return;

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
            transform.SetParent(collision.transform);
        }

    
        
        //make sure projectiles sticks to surface
        rb.isKinematic = true;

        //makes sure projectile moves with target
        

        //transform.SetParent(collision.transform);
    }
}
