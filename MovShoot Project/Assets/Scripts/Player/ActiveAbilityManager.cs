using NUnit.Framework;
using UnityEngine;

public class ActiveAbilityManager : MonoBehaviour
{
    [Header("List")]
    public bool ThrowingActive;
    public bool GrappleActive;

    [Header ("References")]
    public PlayerThrow pThrow;
    public PlayerGrapple pg;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateAbility(GameObject Activator)
    {
        if (Activator.CompareTag("Gun Activator"))
        {
            print("Gunning yeah");
            pThrow.active = true;
            ThrowingActive = true;        
        }
        if (Activator.CompareTag("Grapple Activator"))
        {
            print("Grappling yeah");
            pg.Active = true;
            GrappleActive = true;
        }
    }
}
