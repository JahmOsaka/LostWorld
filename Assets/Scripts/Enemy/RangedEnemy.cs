using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Ranged Attack")]
    public Transform throwPoint;
    private EnemyRangedAttackState rangedAttackState;
    private float originalThrowPointLocalX;

    public EnemyRangedAttackState RangedAttackState => rangedAttackState;

    protected override void Awake()
    {
        base.Awake();

        if (throwPoint != null)
            originalThrowPointLocalX = throwPoint.localPosition.x;

        rangedAttackState = new EnemyRangedAttackState(this, stateMachine, enemyData, "throw");
    }

    public override bool CanSeePlayer()
    {
        if (playerTransform == null) return false;
        Vector2 startPos = BodyCenter;
        Vector2 targetPos = (Vector2)playerTransform.position + Vector2.up * 1.0f;
        return Physics2D.Linecast(startPos, targetPos, groundLayer).collider == null;
    }

    public void SpawnProjectile()
    {
        var ranged = enemyData as RangedEnemyData;
        if (ranged == null || ranged.projectilePrefab == null || throwPoint == null || playerTransform == null) return;
        if (stateMachine.CurrentState != rangedAttackState) return;

        GameObject proj = Instantiate(ranged.projectilePrefab, throwPoint.position, Quaternion.identity);

        if (ranged.useArcProjectile)
        {
            proj.GetComponent<EnemyArcProjectile>()
                .Init(throwPoint.position, playerTransform.position, ranged.arcLaunchAngle, ranged.damage, ranged.projectileSpeed);
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

    protected override void OnFlip(float sign)
    {
        if (throwPoint != null)
        {
            Vector3 pos = throwPoint.localPosition;
            pos.x = Mathf.Abs(originalThrowPointLocalX) * sign;
            throwPoint.localPosition = pos;
        }
    }

    protected override void DrawRangeGizmos()
    {
        var ranged = enemyData as RangedEnemyData;
        bool stationaryRanged = ranged != null && ranged.isStationary;

        if (!stationaryRanged)
        {
            base.DrawRangeGizmos();
        }

        if (ranged == null) return;

        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        Vector3 center = col != null ? (Vector3)col.bounds.center : transform.position;

        DrawRange(center, ranged.throwRange, new Color(1f, 0.5f, 0f), "Throw");
        DrawRange(center, ranged.throwRange * 1.2f, new Color(1f, 0.5f, 0f, 0.35f), "Throw exit");
    }
}