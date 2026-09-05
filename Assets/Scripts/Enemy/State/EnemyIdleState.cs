using UnityEngine;

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void LogicUpdate()
    {
        float detectRange = enemyData.isStationary ? enemyData.throwRange : enemyData.chaseRange;

        if (enemy.DistanceToPlayer() <= detectRange)
        {
           
            if (enemyData.isStationary && enemyData.isRangedEnemy)
            {
                stateMachine.ChangeState(enemy.RangedAttackState);
            }
            else
            {
                stateMachine.ChangeState(enemy.ChaseState);
            }
        }
    }
}