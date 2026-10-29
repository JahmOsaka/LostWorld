using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private float jumpStartTime;
    public PlayerJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        jumpStartTime = Time.time;

        player.InputHandler.UseJumpInput();
        player.ResetCoyoteTime();

        player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, playerData.maxJumpVelocity);

        player.ColliderController?.ActivateJumpCollider();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (!player.InputHandler.IsJumpHolding && player.RB.linearVelocity.y > 0)
        {
            player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, player.RB.linearVelocity.y * playerData.minJumpMultiplier);
        }
        if (Time.time >= jumpStartTime + 0.08f)
        {
            if (player.CheckIfGrounded() && player.RB.linearVelocity.y <= 0.01f)
            {
                stateMachine.ChangeState(player.LandState);
            }
            else if (player.RB.linearVelocity.y < playerData.fallVelocityThreshold)
            {
                stateMachine.ChangeState(player.FallState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        float inputX = player.InputHandler.RawMovementInput.x;
        float moveDirection = inputX == 0 ? 0 : Mathf.Sign(inputX);

        player.RB.linearVelocity = new Vector2(moveDirection * playerData.moveSpeed, player.RB.linearVelocity.y);

        player.CheckIfShouldFlip(moveDirection);
    }

    public override void Exit()
    {
        base.Exit();

    }
}