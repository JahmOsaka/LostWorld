using UnityEngine;

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void LogicUpdate()
    {
        var ranged = enemyData as RangedEnemyData;
        bool stationaryRanged = ranged != null && ranged.isStationary;
        float detectRange = stationaryRanged ? ranged.throwRange : enemyData.chaseRange;

        if(enemy.DistanceToPlayer() <= detectRange && enemy.CanSeePlayer())
        {
            if (stationaryRanged && enemy is RangedEnemy rangedEnemy)
            {
                stateMachine.ChangeState(rangedEnemy.RangedAttackState);
            }
            else
            {
                stateMachine.ChangeState(enemy.ChaseState);
            }
        }
    }
}