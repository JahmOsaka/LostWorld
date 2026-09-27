using UnityEngine;

public class EnemyRetreatState : EnemyState
{
    private Vector2 retreatDir;

    public EnemyRetreatState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        Vector2 away = (enemy.transform.position - enemy.playerTransform.position).normalized;
        retreatDir = away;
    }

    public override void PhysicsUpdate()
    {
        enemy.rb.linearVelocity = new Vector2(retreatDir.x * enemyData.retreatSpeed, enemy.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        float dist = enemy.DistanceToPlayer();
        if (dist >= enemyData.retreatDistance)
        {
            stateMachine.ChangeState(enemy.ChaseState);
        }
    }
}