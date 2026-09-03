using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }
    public override void Enter()
    {
        base.Enter();
        player.ResetCombo();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (player.InputHandler.AttackInput)
        {
            stateMachine.ChangeState(player.AttackState);
        }

        else if (player.InputHandler.DodgeInput && player.DodgeCooldownTimer <= 0)
        {
            stateMachine.ChangeState(player.DodgeState);
        }

        else if (player.InputHandler.JumpInput && player.CheckIfGrounded())
        {
            stateMachine.ChangeState(player.JumpState);
        }

        else if (!player.CheckIfGrounded())
        {
            stateMachine.ChangeState(player.FallState);
        }

        else if (player.InputHandler.RawMovementInput.x == 0)
        {
            stateMachine.ChangeState(player.IdleState);
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