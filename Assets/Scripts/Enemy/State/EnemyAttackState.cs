using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private float cooldownTimer;
    private bool isAttacking;

    private MeleeEnemyData melee => (MeleeEnemyData)enemyData;

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

        if (dist > melee.attackRange * 1.2f)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f && !isAttacking)
        {
            isAttacking = true;
            enemy.anim.SetTrigger("attack");
            cooldownTimer = melee.attackCooldown;
        }
    }

    public void DealHit()
    {
        if (enemy.DistanceToPlayer() <= melee.attackRange)
        {
            enemy.playerTransform.GetComponentInParent<IDamageable>()?.TakeDamage(melee.damage);
        }
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        if (enemyData.retreatsAfterAttack && enemy.DistanceToPlayer() <= enemyData.retreatTriggerRange)
        {
            stateMachine.ChangeState(enemy.RetreatState);
        }
    }
}