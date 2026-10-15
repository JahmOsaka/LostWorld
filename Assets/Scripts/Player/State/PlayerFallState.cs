using UnityEngine;

public class PlayerFallState : PlayerState
{
    [Header("Fall Settings")]
    public float maxFallSpeed = 20f;
    public PlayerFallState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        player.FallDuration = 0f;

    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.FallDuration += Time.deltaTime;

        if (player.RB.linearVelocity.y < 0)
        {
            player.RB.linearVelocity = new Vector2(
                player.RB.linearVelocity.x,
                Mathf.Max(player.RB.linearVelocity.y, -maxFallSpeed)
            );
        }

        if (player.InputHandler.JumpInput && player.CoyoteTimeCounter > 0f)
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

        float inputX = player.InputHandler.RawMovementInput.x;
        float moveDirection = inputX == 0 ? 0 : Mathf.Sign(inputX);

        player.RB.linearVelocity = new Vector2(moveDirection * playerData.moveSpeed, player.RB.linearVelocity.y);
        player.CheckIfShouldFlip(moveDirection);
    }
}