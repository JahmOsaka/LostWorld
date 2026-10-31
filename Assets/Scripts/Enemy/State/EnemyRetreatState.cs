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
        float awayX = enemy.transform.position.x - enemy.playerTransform.position.x;
        retreatDir = new Vector2(awayX >= 0f ? 1f : -1f, 0f);
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
        enemy.anim.SetBool("retreat", false);
        enemy.anim.SetBool("idle", true); 
    }

    private void Finish()
    {
        stateMachine.ChangeState(enemy.HasPatrolPoints ? enemy.PatrolState : enemy.IdleState);
    }
}