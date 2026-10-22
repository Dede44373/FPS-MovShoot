using Unity.VisualScripting;
using UnityEngine;

public class ObjectScripted : MonoBehaviour
{
    public float hitstopDuration;
    public Animator anim;
    public GameObject objectScripted;
    public Transform pos;
    public ParticleSystem deathPart;


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
        SoundManager.PlaySound(SoundType.Rock_Break);
        if(objectScripted != null)
            objectScripted.SetActive(true);
        //Instantiate(objectScripted, pos);
        HitstopManager.Instance.Stop(hitstopDuration);
        Instantiate(deathPart, transform.position, Quaternion.LookRotation(Vector3.up));
        Destroy(gameObject);


    }
    private void OnCollisionEnter(Collision collision)
    {
        print(collision.transform.name);
        if (collision.gameObject.CompareTag("Weapon"))
        {
            TakeDamage();
        }

        //ObjectScripted ObjectScriptedClone = Instantiate(objectScripted);
        //ObjectScriptedClone.transform.SetPositionAndRotation(transform.position, transform.rotation);
        //anim.Play("Event1");

    }
    private void OnTriggerEnter(Collider other)
    {
        print(other.transform.name);
        if (other.gameObject.CompareTag("Weapon"))
        {
            TakeDamage();
        }
    }
}
