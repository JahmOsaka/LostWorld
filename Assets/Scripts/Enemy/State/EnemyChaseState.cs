using UnityEngine;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void LogicUpdate()
    {
        enemy.FacePlayer();
        float dist = enemy.DistanceToPlayer();

        var ranged = enemyData as RangedEnemyData;
        var melee = enemyData as MeleeEnemyData;
        float triggerRange = ranged != null ? ranged.throwRange
                           : melee != null ? melee.attackRange
                           : 0f;

        if (dist <= triggerRange)
        {
            stateMachine.ChangeState(ranged != null ? enemy.RangedAttackState : enemy.AttackState);
            return;
        }
        if (dist > enemyData.chaseRange * 1.5f)
        {
            stateMachine.ChangeState(enemy.HasPatrolPoints ? enemy.PatrolState : enemy.ReturnState);
        }
    }

    public override void PhysicsUpdate()
    {
        float dirX = enemy.playerTransform.position.x - enemy.transform.position.x;
        dirX = dirX >= 0f ? 1f : -1f;

        if (!enemy.IsGroundAheadOf(dirX) || enemy.IsWallAheadOf(dirX))
        {
            enemy.rb.linearVelocity = new Vector2(0f, enemy.rb.linearVelocity.y);
            return;
        }

        enemy.rb.linearVelocity = new Vector2(dirX * enemyData.moveSpeed, enemy.rb.linearVelocity.y);
    }
}