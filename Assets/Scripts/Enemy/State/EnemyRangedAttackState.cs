using UnityEngine;

public class EnemyRangedAttackState : EnemyState
{
    private float cooldownTimer;
    private bool isThrowing;

    public EnemyRangedAttackState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        enemy.rb.linearVelocity = new Vector2(0f, enemy.rb.linearVelocity.y);

    }

    public override void LogicUpdate()
    {
        float dist = enemy.DistanceToPlayer();

        FacePlayer();

        if (dist > enemyData.throwRange * 1.2f)
        {
            stateMachine.ChangeState(enemy.HasPatrolPoints ? enemy.PatrolState : enemy.ReturnState);
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f && !isThrowing)
        {
            isThrowing = true;
            enemy.anim.SetTrigger("throw");
            cooldownTimer = enemyData.throwCooldown;
        }
    }
    public void OnThrowAnimationEnd()
    {
        isThrowing = false;
    }

    private void FacePlayer()
    {
        float dirX = enemy.playerTransform.position.x - enemy.transform.position.x;
        enemy.spriteRenderer.flipX = dirX < 0;
    }
}