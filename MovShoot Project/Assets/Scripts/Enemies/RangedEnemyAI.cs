using UnityEngine;
using UnityEngine.AI;


public class RangedEnemyAI : Enemy
{
    public EnemyState enemyState;

    public float waitTime;
    [Header("Patrolling")]
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    public float normalTurnSpeed;
    public float attackTurnSpeed;
    public float EnemyDistance;

    [Header("Attacking")]
    public int damage;
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public float dashForce;
    private bool attacking;
    public float chaseLungeTime;
    public float projectileSpeed, projectileUpForce;
    public Transform attackPoint;


    [Header("States")]
    public float sightRange, attackRange, retreatRange;
    public bool playerInSightRange, playerInAttackRange, playerInRetreatRange;
    private bool movingTowardsDestination = false;
    private bool lunging;
    private bool retreatGrace;

    [Header("Raycasts")]
    float distanceToTarget;

    [Range(1, 360)]
    public float veiwAngle = 50f;
    public float viewDistance = 10f;

    [Header("References")]
    public LayerMask whatisGround, Player, checkIfWallInWay;
    public NavMeshAgent agent;
    public Transform player;
    public Animator anim;
    public Collider coll;
    public Rigidbody rb;
    public GameObject projectile;
    Vector3 DestinationPoint = Vector3.zero;

    public enum EnemyState
    {
        idle,
        patrol,
        chase,
        attack,
        retreat,
        lunge
    }

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
        if(playerInSightRange || enemyState == EnemyState.chase)
        {
            if (CanSeePlayer())
            {
                playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, Player);
                if (playerInAttackRange) 
                    print("Ranged can SEe player");
                
            }
        }
        if(playerInSightRange)
            playerInRetreatRange = Physics.CheckSphere(transform.position, retreatRange, Player);

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
            case EnemyState.retreat:
                Retreat();
                break;
        }

        if (playerInRetreatRange)
        {
            if (attacking)
                ResetAttack();
            
            enemyState = EnemyState.retreat;
            return;
        }
        if (attacking && !playerInAttackRange)
        {
            attacking = false;
        }
    }
    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= sightRange)
        {
            // Cast a ray towards the player. If it hits a wall first, it returns false.
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer, checkIfWallInWay))
            {
                // The ray hit a wall before reaching the player
                return false;
            }

            // No obstacle was hit
            return true;
        }

        return false;
    }

    void Idle()
    {
        anim.SetTrigger("Idle");
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
        else if (playerInRetreatRange)
        {
            enemyState = EnemyState.retreat;
        }
        else
        {
            enemyState = EnemyState.chase;
        }
    }

    void Patrol()
    {
        anim.SetTrigger("Walk");
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
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                waitTime = 2.0f;
                enemyState = EnemyState.idle;
            }
        }
    }

    void Retreat()
    {
        anim.SetTrigger("Walk");
        print("Running away");
        float squaredDist = (transform.position - player.transform.position).sqrMagnitude;
        float EnemyDistanceRunSqrt = EnemyDistance * EnemyDistance;

        if (squaredDist < EnemyDistanceRunSqrt)
        {
            //Vector3 direction = (transform.position - player.position).normalized;
            Vector3 dirToPlayer = transform.position - player.transform.position;

            Vector3 newPos = transform.position + dirToPlayer; // maybe - instead of +

            agent.SetDestination(newPos);
            retreatGrace = true;
        }
        else
        {
            if (retreatGrace)
            {
                waitTime = 5.0f;
                retreatGrace = false;
            }

            if (waitTime >= 0 )
            {
                waitTime -= Time.deltaTime;
                return;
            }
            enemyState = EnemyState.patrol;
        }
    }
    void Chase()
    {
        anim.SetTrigger("Walk");
        if (playerInAttackRange && playerInSightRange)
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
        //pos.y = transform.position.y;
        transform.LookAt(pos);
        Quaternion.LookRotation(pos);
        anim.SetTrigger("Shoot");

        if (!alreadyAttacked)
        {
            // Attack code here \/\/\/
            agent.SetDestination(player.position);

            Rigidbody rb = Instantiate(projectile, attackPoint.position, attackPoint.localRotation).GetComponent<Rigidbody>();
            rb.transform.LookAt(pos);
            rb.transform.rotation *= Quaternion.Euler(90, 0, 0);
            //rb.transform.localEulerAngles = new Vector3(90, 0, 0);

            // 
            rb.AddForce(attackPoint.forward * projectileSpeed, ForceMode.Impulse);
            //This one for dropoff
            rb.AddForce(attackPoint.up * projectileUpForce, ForceMode.Impulse);

            agent.isStopped = true;
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.bisque;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retreatRange);
    }
}