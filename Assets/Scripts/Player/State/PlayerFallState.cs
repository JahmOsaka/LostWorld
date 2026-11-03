using UnityEngine;

public class PlayerFallState : PlayerState
{
    public PlayerFallState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        player.FallDuration = 0f;
        player.Anim.Play("Jump_Air", 0, 0f);

    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.FallDuration += Time.deltaTime;

        if (player.RB.linearVelocity.y < 0)
        {
            player.RB.linearVelocity = new Vector2(
                player.RB.linearVelocity.x,
                Mathf.Max(player.RB.linearVelocity.y, -playerData.maxFallSpeed)
            );
        }

        if (player.InputHandler.SuperDashInput && player.SkillController.hasSuperDash)
        {
            stateMachine.ChangeState(player.SuperDashState);
            return;
        }

        if (player.InputHandler.JumpInput && (player.CheckIfGrounded() || player.JumpsRemaining > 0 || player.CoyoteTimeCounter > 0f))
        {
            stateMachine.ChangeState(player.JumpState);
        }
        else if (player.CheckIfGrounded())
        {
            stateMachine.ChangeState(player.LandState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        bool isStuck = false;
        if (player.ColliderController != null)
        {
            bool restored = player.ColliderController.TryRestoreStandingCollider();
            if (!restored)
            {
                isStuck = true;
                player.ColliderController.HandleAutoEject(player.RB, player.FacingDirection);
            }
        }

        if (!isStuck)
        {
            float inputX = player.InputHandler.RawMovementInput.x;
            float moveDirection = inputX == 0 ? 0 : Mathf.Sign(inputX);

            player.RB.linearVelocity = new Vector2(moveDirection * playerData.moveSpeed, player.RB.linearVelocity.y);
            player.CheckIfShouldFlip(moveDirection);
        }
    }
}