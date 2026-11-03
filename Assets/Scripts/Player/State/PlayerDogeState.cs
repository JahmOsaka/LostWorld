using UnityEngine;

public class PlayerDodgeState : PlayerState
{
    private bool isDodgeFinished;
    private int dodgeDirection;
    private float dodgeTimer;

    public PlayerDodgeState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.ResetCombo();
        player.InputHandler.UseDodgeInput();
        player.SetInvincible(true);

        player.ColliderController?.ShrinkHurtboxOnlyForDodge();

        isDodgeFinished = false;
        dodgeTimer = 0f;

        if (player.InputHandler.RawMovementInput.x != 0)
            dodgeDirection = (int)Mathf.Sign(player.InputHandler.RawMovementInput.x);
        else
            dodgeDirection = player.FacingDirection;

        player.CheckIfShouldFlip(dodgeDirection);
        player.RB.gravityScale = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        dodgeTimer += Time.deltaTime;

        if (dodgeTimer >= playerData.DodgeDuration)
        {
            isDodgeFinished = true;
        }

        if (dodgeTimer >= playerData.DodgeIFrameDuration)
        {
            player.SetInvincible(false);
        }

        if (!player.CheckIfGrounded())
        {
            stateMachine.ChangeState(player.FallState);
            return;
        }

        if (isDodgeFinished)
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
        player.RB.gravityScale = playerData.defaultGravity;

        player.ColliderController?.RestoreHurtbox();
    }

    public void FinishDodgeAnimation()
    {
        isDodgeFinished = true;
    }
}