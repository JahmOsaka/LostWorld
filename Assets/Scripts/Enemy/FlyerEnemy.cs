using UnityEngine;

public class FlyerEnemy : Enemy
{
    public FlyPatrolState FlyPatrol { get; private set; }
    public FlyFollowState FlyFollow { get; private set; }
    public FlyChargeState FlyCharge { get; private set; }

    [HideInInspector] public float nextChargeTime;

    private bool burstSpawned;

    protected override void Awake()
    {
        base.Awake();
        rb.gravityScale = 0f;
        FlyPatrol = new FlyPatrolState(this, stateMachine, enemyData, "idle");
        FlyFollow = new FlyFollowState(this, stateMachine, enemyData, "idle");
        FlyCharge = new FlyChargeState(this, stateMachine, enemyData, "charge");

        CoreHealth health = GetComponent<CoreHealth>();
        if (health != null) health.OnDeath.AddListener(SpawnBurstHead);
    }

    protected override void Start()
    {
        stateMachine.Initialize(FlyPatrol);
    }

    private void SpawnBurstHead()
    {
        if (burstSpawned) return;
        burstSpawned = true;

        FlyerData data = (FlyerData)enemyData;
        if (data.headPrefab == null) return;

        GameObject head = Instantiate(data.headPrefab, transform.position, Quaternion.identity);
        FlyerHead sporeHead = head.GetComponent<FlyerHead>();
        if (sporeHead != null) sporeHead.Init(data.burstDamage, data.burstRadius, data.burstFuse);
    }
    public bool IsBlocked(Vector2 dir, float distance)
    {
        if (dir.sqrMagnitude < 0.0001f) return false;

        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        if (col == null) return false;

        Bounds b = col.bounds;
        RaycastHit2D hit = Physics2D.BoxCast(b.center, b.size * 0.95f, 0f, dir.normalized, distance, groundLayer);
        return hit.collider != null;
    }


    public Vector2 SlideAgainstWalls(Vector2 vel)
    {
        float look = wallCheckDistance + vel.magnitude * Time.fixedDeltaTime;

        if (Mathf.Abs(vel.x) > 0.001f && IsBlocked(new Vector2(Mathf.Sign(vel.x), 0f), look)) vel.x = 0f;
        if (Mathf.Abs(vel.y) > 0.001f && IsBlocked(new Vector2(0f, Mathf.Sign(vel.y)), look)) vel.y = 0f;

        return vel;
    }

    public Vector2 ClampToOpenSpace(Vector2 target)
    {
        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        if (col == null) return target;

        Bounds b = col.bounds;
        Vector2 from = b.center;
        Vector2 delta = target - from;
        float dist = delta.magnitude;
        if (dist < 0.01f) return target;

        Vector2 dir = delta / dist;
        RaycastHit2D hit = Physics2D.BoxCast(from, b.size * 0.95f, 0f, dir, dist, groundLayer);
        if (hit.collider == null) return target;

        return hit.centroid - dir * 0.05f;
    }

    protected override void OnDrawGizmosSelected()
    {
        FlyerData data = enemyData as FlyerData;
        if (data == null) return;


        Vector3 anchor = Application.isPlaying ? spawnPosition : transform.position;

        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        Vector3 body = col != null ? (Vector3)col.bounds.center : transform.position;


        Gizmos.color = new Color(0.3f, 1f, 0.5f);
        Gizmos.DrawWireCube(anchor, new Vector3(data.hoverRadiusX * 2f, data.hoverRadiusY * 2f, 0f));
        DrawPointLabel(anchor + Vector3.up * data.hoverRadiusY, $"Hover {data.hoverRadiusX:0.0}x{data.hoverRadiusY:0.0}");
        DrawRange(anchor, data.leashRadius, new Color(1f, 0.9f, 0.2f), "Leash");


        DrawRangeGizmos();
        DrawRange(body, data.chaseRange * 1.5f, new Color(0.3f, 0.8f, 1f, 0.35f), "Chase exit x1.5");


        DrawRange(body, data.attackRange, new Color(1f, 0.2f, 0.2f), "Charge trigger");
        DrawRange(body, data.burstRadius, new Color(1f, 0.4f, 0.8f), "Death burst");
        DrawBlockedGizmos(data);

        Transform player = playerTransform;
        if (player == null && !Application.isPlaying)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player == null) return;

        Vector2 playerPos = player.position;

        float side = transform.position.x >= playerPos.x ? 1f : -1f;
        Vector3 followTarget = playerPos + new Vector2(side * data.keepDistance, data.followHeight);
        Gizmos.color = new Color(0.3f, 0.8f, 1f);
        Gizmos.DrawWireSphere(followTarget, 0.25f);
        Gizmos.DrawLine(body, followTarget);
        DrawPointLabel(followTarget + Vector3.up * 0.4f, "Follow target");

        Vector2 to = playerPos - (Vector2)body;
        Vector2 dir = to.sqrMagnitude > 0.0001f ? to.normalized : Vector2.right;
        Vector3 dashEnd = body + (Vector3)(dir * (to.magnitude + data.chargeOvershoot));
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawLine(body, dashEnd);
        Gizmos.DrawWireSphere(dashEnd, 0.2f);
        DrawPointLabel(dashEnd + Vector3.up * 0.4f, "Dash end");
    }

    private static void DrawEllipse(Vector3 center, float rx, float ry, Color color, string label)
    {
        Gizmos.color = color;
        const int segments = 48;
        Vector3 prev = center + new Vector3(rx, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            Vector3 next = center + new Vector3(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry, 0f);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
        DrawPointLabel(center + Vector3.up * ry, $"{label} {rx:0.0}x{ry:0.0}");
    }

    private void DrawBlockedGizmos(FlyerData data)
    {
        Bounds b = GetBodyBounds();
        float dist = wallCheckDistance + data.chargeSpeed * Time.fixedDeltaTime;
        Vector2[] dirs = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

        foreach (Vector2 d in dirs)
        {
            bool blocked = IsBlocked(d, dist);
            Gizmos.color = blocked ? Color.red : new Color(1f, 0.6f, 0.1f);
            Gizmos.DrawWireCube(b.center + (Vector3)(d * dist), b.size * 0.95f);
        }

        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
        Gizmos.DrawWireCube(b.center, b.size);   
    }

    private static void DrawPointLabel(Vector3 pos, string text)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(pos, text);
#endif
    }
}