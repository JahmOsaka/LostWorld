using UnityEngine;

public class EnemyRetreatState : EnemyState
{
    private Vector2 retreatDir;
    private bool isPausing;
    private float pauseTimer;

    public EnemyRetreatState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        isPausing = false;
        retreatDir = (enemy.transform.position - enemy.playerTransform.position).normalized;
    }

    public override void Exit()
    {
        base.Exit();
        enemy.anim.SetBool("idle", false);
    }

    public override void PhysicsUpdate()
    {
        if (isPausing)
        {
            enemy.rb.linearVelocity = new Vector2(0f, enemy.rb.linearVelocity.y);
            return;
        }

        float awayX = enemy.transform.position.x - enemy.playerTransform.position.x;
        if (Mathf.Sign(awayX) != Mathf.Sign(retreatDir.x))
        {
            BeginPause();
            return;
        }

        if (!enemy.IsGroundAheadOf(retreatDir.x))
        {
            BeginPause();
            return;
        }
        enemy.rb.linearVelocity = new Vector2(retreatDir.x * enemyData.retreatSpeed, enemy.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        if (isPausing)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f) Finish();
            return;
        }

        if (enemy.DistanceToPlayer() >= enemyData.retreatDistance)
        {
            BeginPause();
        }
    }

    private void BeginPause()
    {
        if (enemyData.retreatPauseTime <= 0f)
        {
            Finish();
            return;
        }

        isPausing = true;
        pauseTimer = enemyData.retreatPauseTime;
        enemy.rb.linearVelocity = new Vector2(0f, enemy.rb.linearVelocity.y);
        enemy.anim.SetBool("retreat", false);   // ออกจากท่าถอย
        enemy.anim.SetBool("idle", true);       // เล่นท่ายืนระหว่างหยุด
    }

    private void Finish()
    {
        stateMachine.ChangeState(enemy.HasPatrolPoints ? enemy.PatrolState : enemy.IdleState);
    }
}