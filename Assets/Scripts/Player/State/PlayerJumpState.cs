using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private bool hasJumped;
    public PlayerJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        hasJumped = false;

        player.InputHandler.UseJumpInput();
        player.ResetCoyoteTime();

        player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, 0f);
        player.RB.gravityScale = 0f;

        player.ColliderController?.ActivateJumpCollider();

        player.Anim.Play("Jump_Ground", 0, 0f);
        player.Anim.Update(0f);
    }

    public void ExecuteJumpForce()
    {
        if (!hasJumped)
        {
            player.RB.gravityScale = playerData.defaultGravity;

            player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, playerData.maxJumpVelocity);
            hasJumped = true;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (!hasJumped) return;

        if (!player.InputHandler.IsJumpHolding && player.RB.linearVelocity.y > 0)
        {
            player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, player.RB.linearVelocity.y * playerData.minJumpMultiplier);
        }

        if (player.CheckIfGrounded() && player.RB.linearVelocity.y <= 0.01f)
        {
            stateMachine.ChangeState(player.LandState);
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

        player.RB.linearVelocity = new Vector2(moveDirection * playerData.moveSpeed, player.RB.linearVelocity.y);
        player.CheckIfShouldFlip(moveDirection);
    }

    public override void Exit()
    {
        base.Exit();
        player.RB.gravityScale = playerData.defaultGravity;
    }

}