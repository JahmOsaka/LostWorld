using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private bool isJumpCut;

    public PlayerJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        isJumpCut = false;

        player.InputHandler.UseJumpInput();

        player.ResetCoyoteTime();

        player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, playerData.maxJumpVelocity);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (!player.InputHandler.IsJumpHolding && !isJumpCut && player.RB.linearVelocity.y > 0)
        {
            player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, player.RB.linearVelocity.y * playerData.minJumpMultiplier);
            isJumpCut = true;
        }

        if (player.CheckIfTouchingCeiling())
        {
            player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, 0f);
            stateMachine.ChangeState(player.FallState);
        }
        else if (player.CheckIfGrounded() && player.RB.linearVelocity.y <= 0f)
        {
            stateMachine.ChangeState(player.LandState);
        }
        else if (player.CheckIfTouchingWall() && player.RB.linearVelocity.y < 0f)
        {
            stateMachine.ChangeState(player.FallState);
        }
        else if (player.RB.linearVelocity.y < playerData.fallVelocityThreshold)
        {
            stateMachine.ChangeState(player.FallState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        float inputX = player.InputHandler.RawMovementInput.x;
        float moveDirection = inputX == 0 ? 0 : Mathf.Sign(inputX);

        if (player.CheckIfTouchingWall() && moveDirection == player.FacingDirection)
        {
            if (Mathf.Abs(player.RB.linearVelocity.x) > 0.1f)
            {
                player.RB.linearVelocity = new Vector2(0f, player.RB.linearVelocity.y);
            }
        }
        else
        {
            player.RB.linearVelocity = new Vector2(moveDirection * playerData.moveSpeed, player.RB.linearVelocity.y);
        }

        player.CheckIfShouldFlip(moveDirection);
    }
}