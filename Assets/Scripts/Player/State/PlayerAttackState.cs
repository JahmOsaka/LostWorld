using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private float failsafeTimer;
    private bool comboQueued;

    public PlayerAttackState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        failsafeTimer = 0f;
        comboQueued = false;
        player.Anim.speed = playerData.AttackSpeedMultiplier;

        player.InputHandler.UseAttackInput();

        float attackPushSpeed = playerData.attackPushSpeed;
        player.RB.linearVelocity = new Vector2(player.FacingDirection * attackPushSpeed, player.RB.linearVelocity.y);

        if (Time.time >= player.CombatData.LastAttackTime + playerData.comboWindow)
        {
            player.CombatData.ResetCombo();
        }

        player.CombatData.RecordAttack();

        string attackAnimName = "WhipAttack" + player.CombatData.ComboCounter;
        player.Anim.Play(attackAnimName, 0, 0f);

        player.HideAllCombatVisuals();

        player.Anim.Update(0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        failsafeTimer += Time.deltaTime;

        if (player.InputHandler.AttackInput)
        {
            player.InputHandler.UseAttackInput();
            comboQueued = true;
        }

        if (player.InputHandler.DodgeInput && player.CombatData.DodgeCooldownTimer <= 0)
        {
            stateMachine.ChangeState(player.DodgeState);
            return;
        }

        if (player.InputHandler.JumpInput)
        {
            if (player.InputHandler.RawMovementInput.y < 0f && player.Sensor.IsStandingOnOneWay())
            {
                player.InputHandler.UseJumpInput();
                player.DropThroughPlatform();
                return;
            }
            else if (player.CheckIfGrounded() || player.JumpsRemaining > 0 || player.CoyoteTimeCounter > 0f)
            {
                stateMachine.ChangeState(player.JumpState);
                return;
            }
        }

        if (failsafeTimer >= playerData.attackDuration)
        {
            ExitAttackState();
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        float dragForce = 10f;
        float currentX = player.RB.linearVelocity.x;
        float currentY = player.RB.linearVelocity.y;

        player.RB.linearVelocity = new Vector2(Mathf.Lerp(currentX, 0f, Time.fixedDeltaTime * dragForce), currentY);
    }

    public void ExitAttackState()
    {
        player.CombatData.LastAttackTime = Time.time;

        if (comboQueued && player.CombatData.ComboCounter < 3)
        {
            stateMachine.ChangeState(player.AttackState);
            return;
        }

        if (player.CombatData.ComboCounter >= 3)
        {
            player.CombatData.ResetCombo();
        }

        if (player.InputHandler.RawMovementInput.x != 0)
        {
            stateMachine.ChangeState(player.MoveState);
        }
        else
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.Anim.speed = 1f;
        player.DisableHitbox();
        player.HideAllCombatVisuals();
    }
}