using UnityEngine;

public class EnemyReturnState : EnemyState
{
    public EnemyReturnState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void LogicUpdate()
    {
        if (enemy.DistanceToPlayer() <= enemyData.chaseRange)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        float dist = Vector2.Distance(enemy.transform.position, enemy.SpawnPosition);
        if (dist < 0.2f)
        {
            stateMachine.ChangeState(enemy.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        Vector2 dir = (enemy.SpawnPosition - enemy.transform.position).normalized;
        enemy.rb.linearVelocity = new Vector2(dir.x * enemyData.patrolSpeed, enemy.rb.linearVelocity.y);
        FaceDirection(dir.x);
    }

    private void FaceDirection(float dirX)
    {
        if (dirX > 0) enemy.spriteRenderer.flipX = false;
        else if (dirX < 0) enemy.spriteRenderer.flipX = true;
    }
}