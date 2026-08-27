using UnityEngine;

public class PlayerDodgeState : PlayerState
{
    private float dodgeTimer;
    private int dodgeDirection;
    private float defaultGravity;

    public PlayerDodgeState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        player.InputHandler.UseDodgeInput();

        player.SetInvincible(true);
        dodgeTimer = playerData.DodgeDuration;

        if (player.InputHandler.RawMovementInput.x != 0)
            dodgeDirection = (int)Mathf.Sign(player.InputHandler.RawMovementInput.x);
        else
            dodgeDirection = player.FacingDirection;

        player.CheckIfShouldFlip(dodgeDirection);

        defaultGravity = player.RB.gravityScale;
        player.RB.gravityScale = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        dodgeTimer -= Time.deltaTime;

        if (dodgeTimer <= 0)
        {
            if (player.CheckIfGrounded())
            {
                if (player.InputHandler.RawMovementInput.x == 0)
                    stateMachine.ChangeState(player.IdleState);
                else
                    stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                stateMachine.ChangeState(player.FallState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.RB.linearVelocity = new Vector2(dodgeDirection * playerData.DodgeSpeed, 0f);
    }

    public override void Exit()
    {
        base.Exit();

        player.SetInvincible(false);
        player.ResetDodgeCooldown();
        player.RB.gravityScale = defaultGravity;
    }
}