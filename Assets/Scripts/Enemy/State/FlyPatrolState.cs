using UnityEngine;

public class FlyPatrolState : EnemyState
{
    private const int MaxPickAttempts = 6;       
    private const float MinTravelDistance = 0.5f; 
    private const float StuckTime = 0.4f;         
    private const float StuckSpeed = 0.05f;

    private readonly FlyerEnemy fly;
    private FlyerData data => (FlyerData)enemyData;

    private Vector2 target;
    private bool waiting;
    private float waitTimer;
    private float stuckTimer;

    public FlyPatrolState(FlyerEnemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName)
    {
        fly = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        waiting = false;
        stuckTimer = 0f;
        PickTarget();
    }

    public override void LogicUpdate()
    {
        float spawnDist = Vector2.Distance(enemy.transform.position, enemy.spawnPosition);
        if (enemy.DistanceToPlayer() <= enemyData.chaseRange && spawnDist <= data.leashRadius * 0.7f)
        {
            stateMachine.ChangeState(fly.FlyFollow);
            return;
        }

        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                stuckTimer = 0f;
                if (enemy.HasPatrolPoints) enemy.SwitchPatrolPoint();
                PickTarget();
            }
            return;
        }

        bool arrived = Vector2.Distance(enemy.transform.position, target) < 0.25f;

        if (!arrived && enemy.rb.linearVelocity.magnitude < StuckSpeed) stuckTimer += Time.deltaTime;
        else stuckTimer = 0f;

        if (arrived || stuckTimer >= StuckTime)
        {
            waiting = true;
            waitTimer = enemyData.idleTimeAtPoint;
            stuckTimer = 0f;
        }
    }

    public override void PhysicsUpdate()
    {
        if (waiting)
        {
            enemy.rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = (target - (Vector2)enemy.transform.position).normalized;
        enemy.rb.linearVelocity = fly.SlideAgainstWalls(dir * enemyData.patrolSpeed);

        if (Mathf.Abs(dir.x) > 0.05f)
            enemy.FlipX = dir.x > 0f;
    }

    private void PickTarget()
    {
        if (enemy.HasPatrolPoints)
        {
            target = enemy.GetCurrentPatrolPoint().position;
            return;
        }

        Vector2 center = enemy.spawnPosition;
        Vector2 current = enemy.transform.position;

        for (int i = 0; i < MaxPickAttempts; i++)
        {
            Vector2 candidate = center + new Vector2(
                Random.Range(-data.hoverRadiusX, data.hoverRadiusX),
                Random.Range(-data.hoverRadiusY, data.hoverRadiusY));

            candidate = fly.ClampToOpenSpace(candidate);  

            if (Vector2.Distance(candidate, current) >= MinTravelDistance)
            {
                target = candidate;
                return;
            }
        }

        target = current;  
    }
}