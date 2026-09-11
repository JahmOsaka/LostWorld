using UnityEngine;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void LogicUpdate()
    {
        enemy.FacePlayer();
        float dist = enemy.DistanceToPlayer();
        float triggerRange = enemyData.isRangedEnemy ? enemyData.throwRange : enemyData.attackRange;

        if (dist <= triggerRange)
        {
            stateMachine.ChangeState(enemyData.isRangedEnemy ? enemy.RangedAttackState : enemy.AttackState);
            return;
        }
        if (dist > enemyData.chaseRange * 1.5f)
        {
            stateMachine.ChangeState(enemy.HasPatrolPoints ? enemy.PatrolState : enemy.ReturnState);
        }
    }

    public override void PhysicsUpdate()
    {
        Vector2 dir = (enemy.playerTransform.position - enemy.transform.position).normalized;
        enemy.rb.linearVelocity = new Vector2(dir.x * enemyData.moveSpeed, enemy.rb.linearVelocity.y);
    }
}