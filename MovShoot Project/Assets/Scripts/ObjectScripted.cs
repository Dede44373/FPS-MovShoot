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

    public void TakeDamage(int damage= 0)
    {
        objectScripted.SetActive(true);
        Instantiate(objectScripted, pos);


    }
    private void OnCollisionEnter(Collision collision)
    {
        

        //ObjectScripted ObjectScriptedClone = Instantiate(objectScripted);
        //ObjectScriptedClone.transform.SetPositionAndRotation(transform.position, transform.rotation);
        //anim.Play("Event1");

    }

}
