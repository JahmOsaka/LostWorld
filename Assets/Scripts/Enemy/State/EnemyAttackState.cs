using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private float cooldownTimer;
    private bool isAttacking;

    public EnemyAttackState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void Enter()
    {
        enemy.rb.linearVelocity = Vector2.zero;
        cooldownTimer = 0f;
    }

    public override void LogicUpdate()
    {
        if (isAttacking) return;

        enemy.FacePlayer();
        float dist = enemy.DistanceToPlayer();

        if (dist > enemyData.attackRange * 1.2f)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f && !isAttacking)
        {
            isAttacking = true;
            enemy.anim.SetTrigger("attack");
            cooldownTimer = enemyData.attackCooldown;
        }
    }

    public void DealHit()
    {
        if (enemy.DistanceToPlayer() <= enemyData.attackRange)
        {
            enemy.playerTransform.GetComponent<IDamageable>()?.TakeDamage(enemyData.damage);
        }
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        if (enemyData.retreatsAfterAttack)
        {
            stateMachine.ChangeState(enemy.RetreatState);
        }
    }
}