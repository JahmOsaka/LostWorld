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

        if (enemy.DistanceToPlayer() <= detectRange)
        {
            stateMachine.ChangeState(stationaryRanged ? enemy.RangedAttackState : enemy.ChaseState);
        }
    }
}