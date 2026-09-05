using UnityEngine;

public class EnemyPatrolState : EnemyState
{
    private float idleTimer;
    private bool isWaiting;

    public EnemyPatrolState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void Enter()
    {

        isWaiting = false;
        enemy.anim.SetBool("move", true);
        enemy.anim.SetBool("idle", false);
    }

    public override void Exit()
    {
        enemy.anim.SetBool("move", false);
        enemy.anim.SetBool("idle", false);
    }

    public override void LogicUpdate()
    {
        if (!enemyData.canDetectPlayer)
        {
            HandlePatrolWaiting();
            return;
        }

        float detectRange = enemyData.isStationary ? enemyData.throwRange : enemyData.chaseRange;

        if (enemy.DistanceToPlayer() <= detectRange)
        {
            if (enemyData.isStationary && enemyData.isRangedEnemy)
                stateMachine.ChangeState(enemy.RangedAttackState);
            else
                stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        HandlePatrolWaiting();
    }

    private void HandlePatrolWaiting()
    {
        if (isWaiting)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                enemy.SwitchPatrolPoint();
                isWaiting = false;
                enemy.anim.SetBool("idle", false);
                enemy.anim.SetBool("move", true);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        if (isWaiting)
        {
            enemy.rb.linearVelocity = new Vector2(0f, enemy.rb.linearVelocity.y);
            return;
        }

        Transform target = enemy.GetCurrentPatrolPoint();
        float dist = Vector2.Distance(enemy.transform.position, target.position);

        if (dist < 0.2f)
        {
            isWaiting = true;
            idleTimer = enemyData.idleTimeAtPoint;

            enemy.rb.linearVelocity = new Vector2(0f, enemy.rb.linearVelocity.y);
            enemy.anim.SetBool("move", false);
            enemy.anim.SetBool("idle", true);
            return;
        }

        Vector2 dir = (target.position - enemy.transform.position).normalized;
        enemy.rb.linearVelocity = new Vector2(dir.x * enemyData.patrolSpeed, enemy.rb.linearVelocity.y);
        FlipTowards(dir.x);
    }

    private void FlipTowards(float dirX)
    {
        if (dirX > 0) enemy.spriteRenderer.flipX = false;
        else if (dirX < 0) enemy.spriteRenderer.flipX = true;

    }
}