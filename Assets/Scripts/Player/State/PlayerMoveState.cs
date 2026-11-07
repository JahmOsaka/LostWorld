using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        player.ColliderController?.TryRestoreStandingCollider();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

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

        if (player.InputHandler.JumpInput && (player.CheckIfGrounded() || player.JumpsRemaining > 0 || player.CoyoteTimeCounter > 0f))
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (!player.CheckIfGrounded() && player.CoyoteTimeCounter <= 0f)
        {
            stateMachine.ChangeState(player.FallState);
            return;
        }

        if (player.InputHandler.RawMovementInput.x == 0f)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (player.ColliderController != null && player.ColliderController.IsStuckUnderCeiling)
        {
            player.ColliderController.HandleAutoEject(player.RB, player.FacingDirection);
            return;
        }

        float inputX = player.InputHandler.RawMovementInput.x;
        float moveDirection = inputX == 0f ? 0f : Mathf.Sign(inputX);

        float velY = player.CheckIfGrounded() ? Mathf.Min(0f, player.RB.linearVelocity.y) : player.RB.linearVelocity.y;

        player.RB.linearVelocity = new Vector2(moveDirection * playerData.moveSpeed, velY);
        player.CheckIfShouldFlip(moveDirection);
    }
}