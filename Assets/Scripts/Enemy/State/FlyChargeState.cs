using UnityEngine;

public class FlyChargeState : EnemyState
{
    private enum Phase { Windup, Dash, Recover }

    private readonly FlyerEnemy fly;
    private FlyerData data => (FlyerData)enemyData;

    private Phase phase;
    private float timer;
    private Vector2 dashDir;


    private readonly int windupHash = Animator.StringToHash("Windup");
    private readonly int dashHash = Animator.StringToHash("Dash");
    private readonly int recoverHash = Animator.StringToHash("Recover");

    public FlyChargeState(FlyerEnemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName)
    {
        fly = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        phase = Phase.Windup;
        timer = data.chargeWindup;
        enemy.rb.linearVelocity = Vector2.zero;

     
        enemy.anim.Play(windupHash);
    }

    public override void LogicUpdate()
    {
        if (enemy.playerTransform == null)
        {
            stateMachine.ChangeState(fly.FlyPatrol);
            return;
        }

        timer -= Time.deltaTime;

        switch (phase)
        {
            case Phase.Windup:
                enemy.FacePlayer();

                if (timer <= 0f) 
                {
                    Vector2 to = (Vector2)enemy.playerTransform.position - enemy.BodyCenter;
                    float distance = to.magnitude + data.chargeOvershoot;
                    dashDir = to.normalized;

                    timer = distance / data.chargeSpeed;
                    phase = Phase.Dash;

                    if (Mathf.Abs(dashDir.x) > 0.05f)
                        enemy.FlipX = dashDir.x > 0f;

                    
                    enemy.anim.Play(dashHash);
                }
                break;

            case Phase.Dash:
                if (timer <= 0f) 
                {
                    enemy.rb.linearVelocity = Vector2.zero;

                    phase = Phase.Recover;
                    timer = data.chargeRecover; 

                  
                    enemy.anim.Play(recoverHash);
                }
                break;

            case Phase.Recover:
                if (timer <= 0f)
                {
                    fly.nextChargeTime = Time.time + data.chargeCooldown;
                    stateMachine.ChangeState(fly.FlyFollow);
                }
                break;
        }
    }

    public override void PhysicsUpdate()
    {
        if (phase == Phase.Dash)
            enemy.rb.linearVelocity = dashDir * data.chargeSpeed;
    }
}