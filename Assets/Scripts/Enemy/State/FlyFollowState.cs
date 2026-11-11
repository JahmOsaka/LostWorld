using UnityEngine;

public class FlyFollowState : EnemyState
{
    private readonly FlyerEnemy fly;
    private FlyerData data => (FlyerData)enemyData;

    public FlyFollowState(FlyerEnemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName)
    {
        fly = enemy;
    }

    public override void LogicUpdate()
    {
        if (enemy.playerTransform == null)
        {
            stateMachine.ChangeState(fly.FlyPatrol);
            return;
        }

        float spawnDist = Vector2.Distance(enemy.transform.position, enemy.spawnPosition);
        float dist = enemy.DistanceToPlayer();

        if (spawnDist > data.leashRadius || dist > enemyData.chaseRange * 1.5f)
        {
            stateMachine.ChangeState(fly.FlyPatrol);
            return;
        }

        enemy.FacePlayer();

        if (dist <= data.attackRange && Time.time >= fly.nextChargeTime)
            stateMachine.ChangeState(fly.FlyCharge);
    }

    public override void PhysicsUpdate()
    {
        if (enemy.playerTransform == null) return;

        Vector2 p = enemy.playerTransform.position;
        float side = enemy.transform.position.x >= p.x ? 1f : -1f;
        Vector2 desired = p + new Vector2(side * data.keepDistance, data.followHeight);

        Vector2 to = desired - (Vector2)enemy.transform.position;
        float d = to.magnitude;

        Vector2 vel = d < 0.15f
            ? Vector2.zero
            : to.normalized * data.followSpeed * Mathf.Clamp01(d);

        enemy.rb.linearVelocity = fly.SlideAgainstWalls(vel);
    }
}