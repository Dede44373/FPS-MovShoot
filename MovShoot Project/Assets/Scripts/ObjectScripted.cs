using UnityEngine;

public class ObjectScripted : MonoBehaviour
{
    public Animator anim;
    public GameObject objectScripted;
    public Transform pos;

    //private void Awake()
    //{
    //   anim = GetComponent<Animator>();
    //}

    public void Event1()
    {
        //Instantiate(objectScripted, pos);
    }
    public void Event2()
    {

    }
    public void Event3()
    {

    }

    public void TakeDamage()
    {
        objectScripted.SetActive(true);
        //Instantiate(objectScripted, pos);
        Destroy(gameObject);


    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Weapon"))
        {
            TakeDamage();
        }

        //ObjectScripted ObjectScriptedClone = Instantiate(objectScripted);
        //ObjectScriptedClone.transform.SetPositionAndRotation(transform.position, transform.rotation);
        //anim.Play("Event1");

    }

}
