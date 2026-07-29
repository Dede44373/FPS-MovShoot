using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.AI;
public class EnemyAI : MonoBehaviour
{
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

    public float lungeSpeed;

    [Header("States")]
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    private bool movingTowardsDestination = false;

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

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, Player);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, Player);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();

        if (attacking && !playerInAttackRange)
        {
            attacking = false;
        }
    }

    private async void Patrolling()
    {
        if (attacking) return;

        //if (!movingTowardsDestination)
        //{
        //    movingTowardsDestination = true;
        //    DestinationPoint = GetSearchWalkPoint();

        //    print(DestinationPoint);
        //    agent.SetDestination(DestinationPoint);
        //}
        //else
        //{
        //    Vector3 distanceToWalkPoint = DestinationPoint - transform.position;

        //    if (distanceToWalkPoint.sqrMagnitude <= 1f * 1f)
        //    {
        //        await Awaitable.WaitForSecondsAsync(2f, destroyCancellationToken);
        //        walkPointSet = false;
        //        movingTowardsDestination = false;
        //        print("Dun.");
        //    }
        //}

        agent.angularSpeed = normalTurnSpeed;
        if (movingTowardsDestination && !playerInSightRange) return;
        movingTowardsDestination = true;

        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        DestinationPoint = GetSearchWalkPoint();
        agent.SetDestination(DestinationPoint);
        print("Destination set 1");

        while ((transform.position - DestinationPoint).sqrMagnitude > 1f * 1f)
        {
            if (playerInSightRange) return;
            //waiting = false;
            await Awaitable.NextFrameAsync(destroyCancellationToken);
        }

       // waiting = true;
        await Awaitable.WaitForSecondsAsync(waitTime, destroyCancellationToken);
        movingTowardsDestination = false;

        //walkpoint reached

        //while (distanceToWalkPoint.sqrMagnitude > 1f * 1f)
        //{
        //    await Awaitable.NextFrameAsync(destroyCancellationToken);
        //}
    }
    private bool waiting;

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

    private void ChasePlayer()
    {
        agent.angularSpeed = attackTurnSpeed;
        if (attacking) return;
        //agent.SetDestination(player.position);
        print("Destination set 2");
        ExecuteGrapple();
    }


    private void AttackPlayer()
    {
        if (waiting) return;
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
            anim.SetTrigger("Bite");
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        else 
        {
            ExecuteGrapple();
        }

    }

    private void ExecuteGrapple()
    {
        Vector3 pos = player.transform.position;
        pos.y = transform.position.y;

        agent.enabled = false;
        MoveToDestination(pos);

    }

    private IEnumerator ApplyForceUntilDestinationReached(Vector3 Destination)
    {
        float Distance = Vector3.Distance(transform.position, Destination);

        while (Distance > 5f)
        {
            Vector3 Direction = (Destination - player.transform.position).normalized;
            rb.AddForce(Direction * lungeSpeed, ForceMode.Force);
            //pm.rb.AddForce(-Physics.gravity/1.75f * pm.rb.mass, ForceMode.Force);
            Distance = Vector3.Distance(player.transform.position, Destination);
            yield return null;
        }

        agent.enabled = true;
    }


    private void MoveToDestination(Vector3 Destination)
    {
        StartCoroutine(ApplyForceUntilDestinationReached(Destination));
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
        agent.SetDestination(transform.position);
        agent.isStopped = false;
        alreadyAttacked = false;
        //attacking = false;
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.bisque;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
