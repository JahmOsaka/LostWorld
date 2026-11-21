using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private bool isAirAttack;
    private float defaultGravityScale;
    private float airAttackTimer;

    private int comboCounter;
    private bool comboQueued;

    public PlayerAttackState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.InputHandler.UseAttackInput();

        isAirAttack = !player.CheckIfGrounded();
        defaultGravityScale = player.RB.gravityScale;
        comboQueued = false;

        if (isAirAttack)
        {
            player.CombatData.UseAirAttack();
            player.CombatData.SetComboCounter(2);
            airAttackTimer = playerData.airAttackDuration;

            player.RB.gravityScale = playerData.airAttackGravityScale;
            player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, player.RB.linearVelocity.y * 0.25f);

            player.Anim.Play("WhipAttack2", 0, 0f);
        }
        else
        {
            if (comboCounter > 2 || Time.time - player.CombatData.LastAttackTime > playerData.comboWindow)
            {
                comboCounter = 0;
            }

            player.CombatData.SetComboCounter(comboCounter + 1);

            PlayGroundComboAnimation(comboCounter);
            player.RB.linearVelocity = new Vector2(player.FacingDirection * playerData.attackPushSpeed, player.RB.linearVelocity.y);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.InputHandler.DodgeInput && player.CombatData.DodgeCooldownTimer <= 0f && player.CheckIfGrounded())
        {
            comboCounter = 0;
            stateMachine.ChangeState(player.DodgeState);
            return;
        }

        if (isAirAttack)
        {
            airAttackTimer -= Time.deltaTime;

            if (player.CheckIfGrounded())
            {
                stateMachine.ChangeState(player.IdleState);
                return;
            }

            if (isAnimationFinished || airAttackTimer <= 0f)
            {
                stateMachine.ChangeState(player.FallState);
                return;
            }
        }
        else
        {
            if (player.InputHandler.AttackInput && comboCounter < 2)
            {
                player.InputHandler.UseAttackInput();
                comboQueued = true;
            }

            if (isAnimationFinished)
            {
                player.CombatData.LastAttackTime = Time.time;

                if (comboQueued)
                {
                    comboCounter++;
                    stateMachine.ChangeState(player.AttackState);
                }
                else
                {
                    comboCounter++;
                    if (player.InputHandler.RawMovementInput.x != 0)
                        stateMachine.ChangeState(player.MoveState);
                    else
                        stateMachine.ChangeState(player.IdleState);
                }
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (isAirAttack)
        {
            float inputX = player.InputHandler.RawMovementInput.x;
            float moveDirection = inputX == 0 ? 0 : Mathf.Sign(inputX);

            player.RB.linearVelocity = new Vector2(moveDirection * playerData.airAttackMoveSpeed, player.RB.linearVelocity.y);
        }
        else
        {
            float stoppedX = Mathf.Lerp(player.RB.linearVelocity.x, 0f, 15f * Time.fixedDeltaTime);
            player.RB.linearVelocity = new Vector2(stoppedX, player.RB.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();

        player.RB.gravityScale = defaultGravityScale;
        player.DisableHitbox();
        player.HideWhipVisual();
        player.HideVFXVisual();
    }

    public void ExitAttackState()
    {
        isAnimationFinished = true;
    }

    private void PlayGroundComboAnimation(int step)
    {
        switch (step)
        {
            case 0: player.Anim.Play("WhipAttack1", 0, 0f); break;
            case 1: player.Anim.Play("WhipAttack2", 0, 0f); break;
            case 2: player.Anim.Play("WhipAttack3", 0, 0f); break;
        }
    }
}