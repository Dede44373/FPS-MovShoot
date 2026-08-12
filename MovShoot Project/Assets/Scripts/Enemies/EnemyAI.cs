using System.Collections;
//using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.AI;
public class EnemyAI : MonoBehaviour
{
    public EnemyState enemyState;

    public float waitTime;
    [Header("Patrolling")]
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    public float normalTurnSpeed;
    public float attackTurnSpeed;

    [Header("Attacking")]
    public int damage;
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public float dashForce;
    private bool attacking;
    public float chaseLungeTime;

    [Header("Lunge")]
    float lungeTimer;
    float beforeLungeWait;
    public float lungeWaitTime = 2;
    public float lungeSpeed;
    public AnimationCurve lungeJump;
    public float lungeAirTime;
    public float jumpHeight;

    [Header("States")]
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    private bool movingTowardsDestination = false;
    private bool lunging;

    [Header("Raycasts")]
    float distanceToTarget;

    [Range(1, 360)]
    public float veiwAngle = 50f;
    public float viewDistance = 10f;

    [Header("References")]
    public LayerMask whatisGround, Player;
    public NavMeshAgent agent;
    public Transform player;
    private Animator anim;
    public Collider coll;
    public Rigidbody rb;
    Vector3 DestinationPoint = Vector3.zero;

    public enum EnemyState
    {
        idle,
        patrol,
        chase,
        attack,
        lunge
    }

    private void Awake()
    {
        lungeTimer = chaseLungeTime;
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        beforeLungeWait = lungeWaitTime;
    }


    private void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, Player);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, Player);

        switch (enemyState)
        {
            case EnemyState.idle:
                Idle();
                break;
            case EnemyState.patrol:
                Patrol();
                break;
            case EnemyState.chase:
                Chase();
                break;
            case EnemyState.attack:
                Attack();
                break;
            case EnemyState.lunge:
                Lunge();
                break;
        }

        if (attacking && !playerInAttackRange)
        {
            attacking = false;
        }
    }

    void Idle()
    {
        if (!playerInSightRange)
        {
            if (waitTime >= 0)
            {
                waitTime -= Time.deltaTime;
                return;
            }
            walkPointSet = false;
            enemyState = EnemyState.patrol;
        }
        else
        {
            enemyState = EnemyState.chase;
        }
    }

    void Patrol()
    {
        if (playerInSightRange && !playerInAttackRange)
        {
            enemyState = EnemyState.chase;
        }

        agent.angularSpeed = normalTurnSpeed;

        if (!walkPointSet)
        {
            DestinationPoint = GetSearchWalkPoint();
            agent.SetDestination(DestinationPoint);
            print("Destination set 1");

            walkPointSet = true;
        }
        else
        {
            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                waitTime = 2.0f;
                enemyState = EnemyState.idle;
            }
        }
    }

    void Chase()
    {
        lungeTimer -= Time.deltaTime;
        if(lungeTimer <= 0)
        {
            lungeTimer = chaseLungeTime;
            enemyState = EnemyState.lunge;
        }


        if (playerInAttackRange)
        {
            enemyState = EnemyState.attack;
        }
        else
        {
            movingTowardsDestination = true;
            agent.angularSpeed = attackTurnSpeed;
            agent.SetDestination(player.position);
            print("Destination set 2");

            if (!playerInSightRange)
            {
                waitTime = 1.0f;
                enemyState = EnemyState.idle;
            }
        }
    }

    void Attack()
    {
        if (attacking) return;

        attacking = true;
        //Make sure enemy doesn't move
        Vector3 pos = player.transform.position;
        pos.y = transform.position.y;
        transform.LookAt(pos);

        if (!alreadyAttacked)
        {
            // Attack code here \/\/\/
            agent.SetDestination(player.position);
            print("Destination set 3");

            agent.isStopped = true;
            anim.Play("Bite");
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    void Lunge()
    {
        if (lunging)
            return;

        Vector3 pos = player.transform.position;
        pos.y = transform.position.y;

        beforeLungeWait -= Time.deltaTime;
        transform.LookAt(pos);
        Debug.Log("waiting for lunge"); 

        if(beforeLungeWait >= 0)
        {
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            Debug.Log("Lunging");
            lunging = true;

         
            transform.LookAt(pos);

            agent.enabled = false;

            StartCoroutine(ApplyForceUntilDestinationReached(pos));
        }

    }

    private Vector3 GetSearchWalkPoint()
    {
        // calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (NavMesh.SamplePosition(walkPoint, out NavMeshHit Hit, float.MaxValue, NavMesh.AllAreas))
        {
            return Hit.position;
        }
        else
        {
            print("Invalid re-searching");
            return Vector3.zero;
        } 
    }

    private IEnumerator ApplyForceUntilDestinationReached(Vector3 Destination)
    {

        Vector3 startPos = transform.position;
        float t = 0;

        while (t <1)
        {
            t += Time.deltaTime * lungeSpeed;
            if(t > 1)
            {
                t = 1;
            }
            Vector3 newPos = Vector3.Lerp(startPos, Destination, t);
            newPos.y += lungeJump.Evaluate(t) * jumpHeight;
            transform.position = newPos;
            yield return null;
        }

        agent.enabled = true;
        lunging = false;
        beforeLungeWait = lungeWaitTime;
        enemyState = EnemyState.chase;
    }

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
        rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);

    }

    private void ResetAttack()
    {
        Debug.Log("Attack resetting");
        agent.SetDestination(transform.position);
        agent.isStopped = false;
        alreadyAttacked = false;
        enemyState = EnemyState.chase;
        attacking = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        //checks if you hit an enemy
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth player = collision.gameObject.GetComponentInParent<PlayerHealth>();
            Debug.Log(player);

            player.TakeDamage(damage);
        }

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.bisque;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
