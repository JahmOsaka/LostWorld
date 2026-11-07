using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        player.ColliderController?.TryRestoreStandingCollider();

        if (player.ColliderController == null || !player.ColliderController.IsStuckUnderCeiling)
        {
            float velY = player.CheckIfGrounded() ? Mathf.Min(0f, player.RB.linearVelocity.y) : player.RB.linearVelocity.y;
            player.RB.linearVelocity = new Vector2(0f, velY);
        }

        UpdateCombatIdleStatus();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        UpdateCombatIdleStatus();

        if (player.ColliderController != null && player.ColliderController.IsStuckUnderCeiling)
        {
            player.ColliderController.TryRestoreStandingCollider();
            return;
        }

        if (player.InputHandler.AttackInput && player.CheckIfGrounded())
        {
            stateMachine.ChangeState(player.AttackState);
            return;
        }

        if (player.InputHandler.DodgeInput && player.CombatData.DodgeCooldownTimer <= 0 && player.CheckIfGrounded())
        {
            stateMachine.ChangeState(player.DodgeState);
            return;
        }
        else if (player.InputHandler.SuperDashInput && player.SkillController.hasSuperDash)
        {
            stateMachine.ChangeState(player.SuperDashState);
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

        if (!player.CheckIfGrounded() && player.CoyoteTimeCounter <= 0f)
        {
            stateMachine.ChangeState(player.FallState);
            return;
        }

        if (player.InputHandler.RawMovementInput.x != 0f)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
        }
        
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (player.ColliderController != null && player.ColliderController.IsStuckUnderCeiling)
        {
            player.ColliderController.HandleAutoEject(player.RB, player.FacingDirection);
        }
        else
        {
            float velY = player.CheckIfGrounded() ? Mathf.Min(0f, player.RB.linearVelocity.y) : player.RB.linearVelocity.y;
            player.RB.linearVelocity = new Vector2(0f, velY);
        }
    }

    private void UpdateCombatIdleStatus()
    {
        if (playerData == null) return;
        bool isCombatActive = Time.time < player.CombatData.LastAttackTime + playerData.combatIdleDuration;

        player.Anim.SetBool("isCombatIdle", isCombatActive);
    }
}