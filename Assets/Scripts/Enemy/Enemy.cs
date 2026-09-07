using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;

    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform throwPoint;

    private EnemyRangedAttackState rangedAttackState;

    private CoreHealth health;

    public Vector3 spawnPosition;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    private EnemyStateMachine stateMachine;
    private EnemyIdleState idleState;
    private EnemyPatrolState patrolState;
    private EnemyChaseState chaseState;
    private EnemyAttackState attackState;
    private EnemyReturnState returnState;
    public EnemyReturnState ReturnState => returnState;

    public Vector3 SpawnPosition => spawnPosition;
    public EnemyRangedAttackState RangedAttackState => rangedAttackState;
    // เปลี่ยนบรรทัดนี้
    public bool HasPatrolPoints => patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null;

    public SpriteRenderer spriteRenderer { get; private set; }
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Transform playerTransform { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        spawnPosition = transform.position;
        health = GetComponent<CoreHealth>();

        stateMachine = new EnemyStateMachine();
        idleState = new EnemyIdleState(this, stateMachine, enemyData, "idle");
        patrolState = new EnemyPatrolState(this, stateMachine, enemyData, "move");
        chaseState = new EnemyChaseState(this, stateMachine, enemyData, "move");
        attackState = new EnemyAttackState(this, stateMachine, enemyData, "attack");
        rangedAttackState = new EnemyRangedAttackState(this, stateMachine, enemyData, "throw");
        returnState = new EnemyReturnState(this, stateMachine, enemyData, "move");
    }

    private void Start()
    {
        if (HasPatrolPoints)
            stateMachine.Initialize(patrolState);
        else
            stateMachine.Initialize(idleState);
    }
    private void Update() => stateMachine.CurrentState.LogicUpdate();
    private void FixedUpdate() => stateMachine.CurrentState.PhysicsUpdate();

    public float DistanceToPlayer() => Vector2.Distance(transform.position, playerTransform.position);

    public Transform GetCurrentPatrolPoint() => patrolPoints[currentPatrolIndex];

    public void SwitchPatrolPoint()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
    public void SpawnProjectile()
    {
        if (projectilePrefab == null || throwPoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);

        if (enemyData.useArcProjectile)
        {
            proj.GetComponent<EnemyArcProjectile>()
                .Init(throwPoint.position, playerTransform.position, enemyData.arcHeight, enemyData.damage, enemyData.arcSpeedMultiplier);
        }
        else
        {
            Vector2 dir = (playerTransform.position - throwPoint.position).normalized;
            proj.GetComponent<EnemyProjectile>().Init(dir, enemyData.projectileSpeed, enemyData.damage);
        }
    }

    public void NotifyThrowAnimationEnd()
    {
        rangedAttackState.OnThrowAnimationEnd();
    }

    public void NotifyAttackHit()
    {
        attackState.DealHit();
    }

    public void NotifyAttackAnimationEnd()
    {
        attackState.OnAttackAnimationEnd();
    }



    public EnemyIdleState IdleState => idleState;
    public EnemyPatrolState PatrolState => patrolState;
    public EnemyChaseState ChaseState => chaseState;
    public EnemyAttackState AttackState => attackState;
}