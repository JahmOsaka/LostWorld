using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        player.InputHandler.UseJumpInput();

        player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, playerData.maxJumpVelocity);
        player.Anim.Play("Jump_Ground");

        player.ColliderController?.ShrinkForJump();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (!player.InputHandler.IsJumpHolding && player.RB.linearVelocity.y > 0)
        {
            player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, player.RB.linearVelocity.y * playerData.minJumpMultiplier);
        }
        if (player.CheckIfGrounded() && player.RB.linearVelocity.y <= 0f)
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

        player.ColliderController?.RestoreHurtbox();
    }
}