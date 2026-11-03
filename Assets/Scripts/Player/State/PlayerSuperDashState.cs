using UnityEngine;
public class PlayerSuperDashState : PlayerState
{
    public PlayerSuperDashState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        player.InputHandler.UseSuperDashInput();
        player.RB.gravityScale = 0f;
        player.RB.linearVelocity = new Vector2(player.FacingDirection * playerData.superDashSpeed, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.InputHandler.DodgeInput || player.CheckIfTouchingWall())
        {
            player.InputHandler.UseDodgeInput();
            stateMachine.ChangeState(player.CheckIfGrounded() ? player.IdleState : player.FallState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.RB.gravityScale = playerData.defaultGravity;
    }
}