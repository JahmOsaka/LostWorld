using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    [Header("Hitbox Flip")]
    public BoxCollider2D physicsCollider;
    public BoxCollider2D hitboxCollider;
    public Transform damageHitboxTransform;

    private Vector2 originalPhysicsOffset;
    private Vector2 originalColliderOffset;
    private float originalDamageLocalX;
    private float originalThrowPointLocalX;

    [Header("Edge Detection")]
    public float edgeCheckDistance = 1f;
    public float edgeCheckForwardOffset = 0.4f;
    public LayerMask groundLayer;

    private List<GameObject> activeOrbs = new List<GameObject>();

    [Header("Ranged Attack")]
    public Transform throwPoint;

    private EnemyRangedAttackState rangedAttackState;

    private EnemyRetreatState retreatState;
    public EnemyRetreatState RetreatState => retreatState;

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

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        spawnPosition = transform.position;
        health = GetComponent<CoreHealth>();
        if (health != null && enemyData != null)
        {
            health.InitializeMaxHealth(enemyData.maxHealth);
        }

        if (physicsCollider == null) physicsCollider = GetComponent<BoxCollider2D>();
        if (physicsCollider != null) originalPhysicsOffset = physicsCollider.offset;

        if (hitboxCollider != null)
            originalColliderOffset = hitboxCollider.offset;
        if (damageHitboxTransform != null)
            originalDamageLocalX = damageHitboxTransform.localPosition.x;
        if (throwPoint != null)
            originalThrowPointLocalX = throwPoint.localPosition.x;

        stateMachine = new EnemyStateMachine();
        idleState = new EnemyIdleState(this, stateMachine, enemyData, "idle");
        patrolState = new EnemyPatrolState(this, stateMachine, enemyData, "move");
        chaseState = new EnemyChaseState(this, stateMachine, enemyData, "move");
        attackState = new EnemyAttackState(this, stateMachine, enemyData, "attack");
        rangedAttackState = new EnemyRangedAttackState(this, stateMachine, enemyData, "throw");
        returnState = new EnemyReturnState(this, stateMachine, enemyData, "move");
        retreatState = new EnemyRetreatState(this, stateMachine, enemyData, "retreat");
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

    public Transform GetCurrentPatrolPoint() => patrolPoints[currentPatrolIndex];

    public void SwitchPatrolPoint()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    public void SpawnProjectile()
    {
        var ranged = enemyData as RangedEnemyData;
        if (ranged == null || ranged.projectilePrefab == null || throwPoint == null || playerTransform == null) return;

        GameObject proj = Instantiate(ranged.projectilePrefab, throwPoint.position, Quaternion.identity);

        if (stateMachine.CurrentState != rangedAttackState) return;
        if (ranged.useArcProjectile)
        {
            proj.GetComponent<EnemyArcProjectile>()
                .Init(throwPoint.position, playerTransform.position, ranged.arcHeight, ranged.damage, ranged.arcSpeedMultiplier);
        }
        else
        {
            Vector2 dir = (playerTransform.position - throwPoint.position).normalized;
            proj.GetComponent<EnemyProjectile>().Init(dir, ranged.projectileSpeed, ranged.damage);
        }
    }

    public void NotifyThrowAnimationEnd()
    {
        if (stateMachine.CurrentState != rangedAttackState) return;
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

    public void NotifySpawnOrbs()
    {
        var elite = enemyData as EliteEnemyData;
        if (elite == null || elite.orbPrefab == null) return;

        foreach (var orb in activeOrbs)
        {
            if (orb != null) Destroy(orb);
        }
        activeOrbs.Clear();

        for (int i = 0; i < elite.orbCount; i++)
        {
            GameObject orbObj = Instantiate(elite.orbPrefab, transform.position, Quaternion.identity);
            float startAngle = (360f / elite.orbCount) * i;
            orbObj.GetComponent<OrbitingDamageOrb>()
                .Init(transform, elite.orbitRadius, elite.orbitSpeed, startAngle, elite.orbDuration, elite.damage);
            activeOrbs.Add(orbObj);
        }
    }

    public void FacePlayer()
    {
        if (playerTransform != null)
        {
            float dirX = playerTransform.position.x - transform.position.x;
            if (dirX > 0)
                FlipX = true;
            else if (dirX < 0)
                FlipX = false;
        }
    }

    public bool FlipX
    {
        get => spriteRenderer.flipX;
        set
        {
            spriteRenderer.flipX = value;
            float sign = value ? 1f : -1f;

            if (physicsCollider != null)
            {
                physicsCollider.offset = new Vector2(Mathf.Abs(originalPhysicsOffset.x) * sign, originalPhysicsOffset.y);
            }

            if (hitboxCollider != null)
            {
                hitboxCollider.offset = new Vector2(Mathf.Abs(originalColliderOffset.x) * sign, originalColliderOffset.y);
            }

            if (damageHitboxTransform != null)
            {
                Vector3 pos = damageHitboxTransform.localPosition;
                pos.x = Mathf.Abs(originalDamageLocalX) * sign;
                damageHitboxTransform.localPosition = pos;
            }

            if (throwPoint != null)
            {
                Vector3 pos = throwPoint.localPosition;
                pos.x = Mathf.Abs(originalThrowPointLocalX) * sign;
                throwPoint.localPosition = pos;
            }
        }
    }

    public Vector2 BodyCenter => physicsCollider != null ? (Vector2)physicsCollider.bounds.center : (Vector2)transform.position;

    public float DistanceToPlayer()
    {
        if (playerTransform == null) return Mathf.Infinity;
        return Vector2.Distance(BodyCenter, playerTransform.position);
    }

    public bool IsGroundAheadOf(float dirX)
    {
        if (Mathf.Approximately(dirX, 0f)) return true;

        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        Vector2 feetPos = col != null ? new Vector2(col.bounds.center.x, col.bounds.min.y) : (Vector2)transform.position;


        Vector2 origin = feetPos + new Vector2(Mathf.Sign(dirX) * edgeCheckForwardOffset, 0.1f);
        return Physics2D.Raycast(origin, Vector2.down, edgeCheckDistance, groundLayer);
    }
    public void ClearActiveOrbs()
    {
        foreach (var orb in activeOrbs)
        {
            if (orb != null) Destroy(orb);
        }
        activeOrbs.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        Vector2 feetPos = (col != null)
            ? (Application.isPlaying ? new Vector2(col.bounds.center.x, col.bounds.min.y) : (Vector2)transform.position - new Vector2(0, col.size.y / 2f * transform.localScale.y))
            : (Vector2)transform.position;

        Vector2 rightOrigin = feetPos + new Vector2(edgeCheckForwardOffset, 0.1f);
        Vector2 leftOrigin = feetPos + new Vector2(-edgeCheckForwardOffset, 0.1f);

        Gizmos.DrawLine(rightOrigin, rightOrigin + Vector2.down * edgeCheckDistance);
        Gizmos.DrawLine(leftOrigin, leftOrigin + Vector2.down * edgeCheckDistance);
    }

    public EnemyIdleState IdleState => idleState;
    public EnemyPatrolState PatrolState => patrolState;
    public EnemyChaseState ChaseState => chaseState;
    public EnemyAttackState AttackState => attackState;
}