using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private float comboTimer;
    private bool secondHitPending;

    public EnemyAttackState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        enemy.rb.linearVelocity = Vector2.zero;

        DealHit();                          
        comboTimer = enemyData.comboInterval;
        secondHitPending = true;
    }

    public override void LogicUpdate()
    {
        if (secondHitPending)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                DealHit();                  
                secondHitPending = false;
            }
        }

        if (enemy.DistanceToPlayer() > enemyData.attackRange)
        {
            stateMachine.ChangeState(enemy.ChaseState);
        }
    }

    private void DealHit()
    {
        if (enemy.DistanceToPlayer() <= enemyData.attackRange)
        {
            enemy.playerTransform.GetComponent<IDamageable>()?.TakeDamage(enemyData.damage);
        }
    }
}