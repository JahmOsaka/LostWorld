using UnityEngine;

public class FlyPatrolState : EnemyState
{
    private readonly FlyerEnemy fly;
    private FlyerData data => (FlyerData)enemyData;

    private Vector2 target;
    private bool waiting;
    private float waitTimer;

    public FlyPatrolState(FlyerEnemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName)
    {
        fly = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        waiting = false;
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
                if (enemy.HasPatrolPoints) enemy.SwitchPatrolPoint();
                PickTarget();
            }
            return;
        }

        if (Vector2.Distance(enemy.transform.position, target) < 0.25f)
        {
            waiting = true;
            waitTimer = enemyData.idleTimeAtPoint;
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
        enemy.rb.linearVelocity = dir * enemyData.patrolSpeed;

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
        target = center + new Vector2(
            Random.Range(-data.hoverRadiusX, data.hoverRadiusX),
            Random.Range(-data.hoverRadiusY, data.hoverRadiusY));
    }
}