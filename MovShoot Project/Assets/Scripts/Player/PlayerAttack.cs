using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;
    public UserInputs Controls;
    [SerializeField] private Collider coll;
    private WaitForSeconds ad;
    public float attackDelay = 0.5f;

    [SerializeField] int damage;
    private bool targetHit;
    private bool inAttack;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        ad = new WaitForSeconds(attackDelay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        Controls = UserInputManager.Instance.Controls;
        Controls.Player.Attack.performed += AttackStart;
        Controls.Player.Attack.canceled += AttackStop;
    }
    private void OnDisable()
    {
        Controls.Player.Attack.performed -= AttackStart;
        Controls.Player.Attack.canceled -= AttackStop;
    }

    private void AttackStart(InputAction.CallbackContext ctx)
    {
        if (inAttack == false) 
        StartCoroutine(Attacking());
    }
    // Walking
    private void AttackStop(InputAction.CallbackContext ctx)
    {
       
    }

     private IEnumerator Attacking()
    {
        inAttack = true;
        anim.SetTrigger("Attack");
        yield return ad;
        inAttack = false;
    }

    public void EnableWeaponCollider()
    {
        coll.enabled = true;
    }

    public void DisableWeaponCollider()
    {
        coll.enabled = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        //checks if you hit an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            BasicEnemy enemy = collision.gameObject.GetComponent<BasicEnemy>();

            enemy.TakeDamage(damage);
        }
        
    }
}
