using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private float cooldownTimer;
    private bool isAttacking;
    private bool lastHitLanded;
    private bool waitingCombo;
    private float comboTimer;
    private CoreHealth coreHealth;

    private MeleeEnemyData melee => (MeleeEnemyData)enemyData;
    private bool hasCombo => enemyData is EliteEnemyData;

    public EnemyAttackState(Enemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName) { }

    public override void Enter()
    {
        enemy.rb.linearVelocity = Vector2.zero;
        cooldownTimer = 0f;
        isAttacking = false;
        waitingCombo = false;
        lastHitLanded = false;

        enemy.anim.ResetTrigger("combo");
        enemy.anim.ResetTrigger("comboEnd");
    }

    public override void LogicUpdate()
    {
        if (waitingCombo)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                waitingCombo = false;
                enemy.anim.SetTrigger("combo");   
            }
            return;
        }

        if (isAttacking) return;

        enemy.FacePlayer();
        float dist = enemy.DistanceToPlayer();

        if (dist > melee.attackRange * 1.2f)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            isAttacking = true;
            lastHitLanded = false;
            enemy.anim.SetTrigger("attack");
            cooldownTimer = melee.attackCooldown;
        }
    }
    private bool CanUseOrbAttack()
    {
        if (!hasCombo) return false;
        if (coreHealth == null) coreHealth = enemy.GetComponent<CoreHealth>();
        if (coreHealth == null) return false;

        var elite = (EliteEnemyData)enemyData;
        return coreHealth.currentHealth <= coreHealth.maxHealth * elite.orbHpPercent;
    }

    private bool CanHurt(CoreHealth health)
    {
        foreach (Collider2D col in health.GetComponentsInChildren<Collider2D>())
        {
            if (health.CanBeDamagedBy(col)) return true;
        }
        return false;
    }

    public void DealHit()
    {
        bool landed = false;

        if (enemy.DistanceToPlayer() <= melee.attackRange)
        {
            var target = enemy.playerTransform.GetComponentInParent<IDamageable>();
            var health = enemy.playerTransform.GetComponentInParent<CoreHealth>();

            if (target != null && (health == null || CanHurt(health)))
            {
                target.TakeDamage(melee.damage);
                landed = true;
            }
        }

        lastHitLanded = landed;
    }

    public void OnComboCheck()
    {
        if (!hasCombo || waitingCombo) return;

        if (lastHitLanded && CanUseOrbAttack())
        {
            waitingCombo = true;
            comboTimer = melee.comboInterval;    
        }
        else
        {
            enemy.anim.SetTrigger("comboEnd"); 
            OnAttackAnimationEnd();
        }
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        waitingCombo = false;

        if (enemyData.ShouldRetreat(enemy.DistanceToPlayer()))
            stateMachine.ChangeState(enemy.RetreatState);
    }
}