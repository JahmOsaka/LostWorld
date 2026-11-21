using System;
using UnityEngine;

[System.Serializable]
public struct VFXData
{
    public string vfxName;
    public GameObject prefab;
    public Transform spawnPoint;
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;

    [Header("Hitbox Flip")]
    public BoxCollider2D physicsCollider;
    public BoxCollider2D hitboxCollider;
    public Transform damageHitboxTransform;

    [Header("Attack Hitboxes")]
    public GameObject[] attackHitboxes;

    [Header("VFX Settings")]
    public VFXData[] vfxList;

    protected Vector2 originalPhysicsOffset;
    protected Vector2 originalColliderOffset;
    protected float originalDamageLocalX;

    [Header("Edge Detection")]
    public float edgeCheckDistance = 1f;
    public float edgeCheckForwardOffset = 0.4f;
    public float wallCheckDistance = 0.1f;
    public LayerMask groundLayer;

    protected CoreHealth health;
    public Vector3 spawnPosition;
    public Transform[] patrolPoints;
    protected int currentPatrolIndex = 0;

    protected EnemyStateMachine stateMachine;
    protected EnemyIdleState idleState;
    protected EnemyPatrolState patrolState;
    protected EnemyChaseState chaseState;
    protected EnemyAttackState attackState;
    protected EnemyReturnState returnState;
    protected EnemyRetreatState retreatState;

    public EnemyRetreatState RetreatState => retreatState;
    public EnemyReturnState ReturnState => returnState;
    public EnemyAttackState AttackState => attackState;
    public Vector3 SpawnPosition => spawnPosition;
    public bool HasPatrolPoints => patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null;

    public SpriteRenderer spriteRenderer { get; private set; }
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Transform playerTransform { get; private set; }

    protected virtual void Awake()
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

        stateMachine = new EnemyStateMachine();
        idleState = new EnemyIdleState(this, stateMachine, enemyData, "idle");
        patrolState = new EnemyPatrolState(this, stateMachine, enemyData, "move");
        chaseState = new EnemyChaseState(this, stateMachine, enemyData, "move");
        attackState = new EnemyAttackState(this, stateMachine, enemyData, "attack");
        returnState = new EnemyReturnState(this, stateMachine, enemyData, "move");
        retreatState = new EnemyRetreatState(this, stateMachine, enemyData, "retreat");
    }

    protected virtual void Start()
    {
        if (HasPatrolPoints)
            stateMachine.Initialize(patrolState);
        else
            stateMachine.Initialize(idleState);
    }

    protected virtual void Update() => stateMachine.CurrentState.LogicUpdate();
    protected virtual void FixedUpdate() => stateMachine.CurrentState.PhysicsUpdate();

    public Transform GetCurrentPatrolPoint() => patrolPoints[currentPatrolIndex];

    public void SwitchPatrolPoint()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    public void NotifyAttackAnimationEnd()
    {
        if (stateMachine.CurrentState != attackState) return;
        attackState.OnAttackAnimationEnd();
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
                physicsCollider.offset = new Vector2(Mathf.Abs(originalPhysicsOffset.x) * sign, originalPhysicsOffset.y);

            if (hitboxCollider != null)
                hitboxCollider.offset = new Vector2(Mathf.Abs(originalColliderOffset.x) * sign, originalColliderOffset.y);

            if (attackHitboxes != null)
            {
                foreach (GameObject hb in attackHitboxes)
                {
                    if (hb != null)
                    {
                        Vector3 pos = hb.transform.localPosition;
                        pos.x = Mathf.Abs(pos.x) * sign;
                        hb.transform.localPosition = pos;
                    }
                }
            }

            if (vfxList != null)
            {
                foreach (VFXData vfx in vfxList)
                {
                    if (vfx.spawnPoint != null)
                    {
                        Vector3 vfxPos = vfx.spawnPoint.localPosition;
                        vfxPos.x = Mathf.Abs(vfxPos.x) * sign;
                        vfx.spawnPoint.localPosition = vfxPos;
                    }
                }
            }

            OnFlip(sign);
        }
    }

    protected virtual void OnFlip(float sign) { }

    public void EnableAttackHitbox(int index)
    {
        if (index >= 0 && index < attackHitboxes.Length && attackHitboxes[index] != null)
            attackHitboxes[index].SetActive(true);
    }
    public void DisableAttackHitbox(int index)
    {
        if (index >= 0 && index < attackHitboxes.Length && attackHitboxes[index] != null)
            attackHitboxes[index].SetActive(false);
    }
    public void DisableAllAttackHitboxes()
    {
        foreach (var hb in attackHitboxes)
        {
            if (hb != null) hb.SetActive(false);
        }
    }
    public void SpawnVFX(int index)
    {
        if (index >= 0 && index < vfxList.Length)
        {
            var data = vfxList[index];
            if (data.prefab != null && data.spawnPoint != null)
            {
                GameObject vfx = Instantiate(data.prefab, data.spawnPoint.position, data.spawnPoint.rotation);

                SpriteRenderer vfxSprite = vfx.GetComponentInChildren<SpriteRenderer>();
                if (vfxSprite != null)
                {
                    vfxSprite.flipX = this.spriteRenderer.flipX;
                }
            }
        }
    }

    public Vector2 BodyCenter => physicsCollider != null ? (Vector2)physicsCollider.bounds.center : (Vector2)transform.position;

    public float DistanceToPlayer()
    {
        if (playerTransform == null) return Mathf.Infinity;
        return Vector2.Distance(BodyCenter, playerTransform.position);
    }

    public bool CanSeePlayer()
    {
        if (playerTransform == null) return false;
        Vector2 startPos = BodyCenter;
        Vector2 targetPos = (Vector2)playerTransform.position + Vector2.up * 1.0f;
        return Physics2D.Linecast(startPos, targetPos, groundLayer).collider == null;
    }

    public bool IsGroundAheadOf(float dirX)
    {
        if (Mathf.Approximately(dirX, 0f)) return true;
        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        Vector2 feetPos = col != null ? new Vector2(col.bounds.center.x, col.bounds.min.y) : (Vector2)transform.position;
        Vector2 origin = feetPos + new Vector2(Mathf.Sign(dirX) * edgeCheckForwardOffset, 0.1f);
        return Physics2D.Raycast(origin, Vector2.down, edgeCheckDistance, groundLayer);
    }

    public bool IsWallAheadOf(float dirX)
    {
        if (Mathf.Approximately(dirX, 0f)) return false;
        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        if (col == null) return false;
        Bounds b = col.bounds;
        Vector2 dir = new Vector2(Mathf.Sign(dirX), 0f);
        float dist = b.extents.x + wallCheckDistance;
        Vector2 mid = b.center;
        Vector2 low = new Vector2(b.center.x, b.min.y + 0.1f);
        return Physics2D.Raycast(mid, dir, dist, groundLayer) || Physics2D.Raycast(low, dir, dist, groundLayer);
    }

    public virtual void ResetEnemyState()
    {
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        DisableAllAttackHitboxes();

        currentPatrolIndex = 0;

        if (stateMachine != null)
        {
            if (HasPatrolPoints)
                stateMachine.Initialize(patrolState);
            else
                stateMachine.Initialize(idleState);
        }
    }

    protected virtual void OnDrawGizmosSelected()
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
        DrawWallCheckGizmos();
        DrawRangeGizmos();
    }

    protected virtual void DrawRangeGizmos()
    {
        if (enemyData == null) return;
        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        Vector3 center = col != null ? (Vector3)col.bounds.center : transform.position;

        DrawRange(center, enemyData.chaseRange, new Color(0.3f, 0.8f, 1f), "Chase");

        if (enemyData.retreatsAfterAttack)
        {
            DrawRange(center, enemyData.retreatTriggerRange, new Color(0.8f, 0.3f, 1f), "Retreat trigger");
            DrawRange(center, enemyData.retreatDistance, new Color(0.3f, 1f, 0.4f), "Retreat dist");
        }
    }

    protected static void DrawRange(Vector3 center, float radius, Color color, string label)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(center, radius);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(center + Vector3.up * radius, $"{label} {radius:0.0}");
#endif
    }

    protected Bounds GetBodyBounds()
    {
        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        if (col == null) return new Bounds(transform.position, Vector3.one);
        if (Application.isPlaying) return col.bounds;
        Vector3 s = col.transform.lossyScale;
        Vector3 center = col.transform.TransformPoint(col.offset);
        Vector3 size = new Vector3(Mathf.Abs(col.size.x * s.x), Mathf.Abs(col.size.y * s.y), 0.1f);
        return new Bounds(center, size);
    }

    protected void DrawWallCheckGizmos()
    {
        Bounds b = GetBodyBounds();
        float dist = b.extents.x + wallCheckDistance;
        Vector2 mid = b.center;
        Vector2 low = new Vector2(b.center.x, b.min.y + 0.1f);
        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
        Gizmos.DrawWireCube(b.center, b.size);
        foreach (float dirX in new[] { -1f, 1f })
        {
            Vector2 dir = new Vector2(dirX, 0f);
            bool hitMid = Physics2D.Raycast(mid, dir, dist, groundLayer);
            bool hitLow = Physics2D.Raycast(low, dir, dist, groundLayer);
            Gizmos.color = hitMid ? Color.red : new Color(1f, 0.6f, 0.1f);
            Gizmos.DrawLine(mid, mid + dir * dist);
            Gizmos.color = hitLow ? Color.red : new Color(1f, 0.6f, 0.1f);
            Gizmos.DrawLine(low, low + dir * dist);
        }
    }

    public EnemyIdleState IdleState => idleState;
    public EnemyPatrolState PatrolState => patrolState;
    public EnemyChaseState ChaseState => chaseState;
}