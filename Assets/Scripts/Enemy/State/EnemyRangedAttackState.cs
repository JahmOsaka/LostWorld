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
        // ไม่ reset cooldownTimer แล้ว ปล่อยให้นับต่อจากค่าที่ค้างไว้
    }

    public override void LogicUpdate()
    {
        float dist = enemy.DistanceToPlayer();

        FacePlayer();

        if (dist > enemyData.throwRange * 1.2f)  // เพิ่ม buffer กันเด้งเข้า-ออกถี่
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

    // เรียกจาก Animation Event ตัวที่ 2 ที่วางไว้เฟรมสุดท้ายของ clip EnemyThrow
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