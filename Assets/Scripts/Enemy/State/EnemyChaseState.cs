using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private bool isBlocked;

    public EnemyChaseState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        isBlocked = false;
    }

    public override void Exit()
    {
        base.Exit();
        enemy.anim.SetBool("idle", false);
    }

    public override void LogicUpdate()
    {
        enemy.FacePlayer();
        float dist = enemy.DistanceToPlayer();

        var ranged = enemyData as RangedEnemyData;
        var melee = enemyData as MeleeEnemyData;
        float triggerRange = ranged != null ? ranged.throwRange
                           : melee != null ? melee.attackRange
                           : 0f;

        float yDiff = Mathf.Abs(enemy.playerTransform.position.y - enemy.transform.position.y);
        if (dist <= triggerRange && yDiff <= 1.5f && enemy.CanSeePlayer())
        {
            if (ranged != null && enemy is RangedEnemy rangedEnemy)
                stateMachine.ChangeState(rangedEnemy.RangedAttackState);
            else
                stateMachine.ChangeState(enemy.AttackState);
            return;
        }
        if (dist > enemyData.chaseRange * 1.5f)
        {
            stateMachine.ChangeState(enemy.HasPatrolPoints ? enemy.PatrolState : enemy.ReturnState);
        }

        if (!isBlocked && enemy is EliteEnemy elite && elite.CanTeleportToPlayer())
        {
            stateMachine.ChangeState(elite.TeleportState);
            return;
        }

        if (dist > enemyData.chaseRange * 1.5f)
        {
            stateMachine.ChangeState(enemy.HasPatrolPoints ? enemy.PatrolState : enemy.ReturnState);
        }
    }

    public override void PhysicsUpdate()
    {
        float dirX = enemy.playerTransform.position.x - enemy.transform.position.x;
        dirX = dirX >= 0f ? 1f : -1f;

        if (!enemy.IsGroundAheadOf(dirX) || enemy.IsWallAheadOf(dirX))
        {
            enemy.rb.linearVelocity = new Vector2(0f, enemy.rb.linearVelocity.y);
            SetBlocked(true);
            return;
        }

        SetBlocked(false);
        enemy.rb.linearVelocity = new Vector2(dirX * enemyData.moveSpeed, enemy.rb.linearVelocity.y);
    }

    private void SetBlocked(bool blocked)
    {
        if (isBlocked == blocked) return;
        isBlocked = blocked;
        enemy.anim.SetBool(animBoolName, !blocked);
        enemy.anim.SetBool("idle", blocked); 
    }
}