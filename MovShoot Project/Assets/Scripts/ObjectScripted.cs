using UnityEngine;

public class ObjectScripted : MonoBehaviour
{
    public Animator anim;
    private void Awake()
    {
       anim = GetComponent<Animator>();
    }

    public void Event1()
    {
        anim.Play("Event1");
    }
    public void Event2()
    {

    }
    public void Event3()
    {

    }

}
