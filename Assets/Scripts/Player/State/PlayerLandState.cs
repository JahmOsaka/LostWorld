using UnityEngine;

public class PlayerLandState : PlayerState
{
    public PlayerLandState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        
        player.RB.linearVelocity = new Vector2(0f, 0f);

        float animSpeed = playerData.defaultLandingSpeed;

        if (player.FallDuration < playerData.shortFallDuration)
        {
            animSpeed = playerData.shortFallAnimSpeed;
        }
        else if (player.FallDuration < playerData.mediumFallDuration)
        {
            animSpeed = playerData.mediumFallAnimSpeed;
        }

        player.Anim.SetFloat("LandingSpeed", animSpeed);
        player.FallDuration = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.InputHandler.JumpInput)
        {
            stateMachine.ChangeState(player.JumpState);
        }
        else if (isAnimationFinished)
        {
            if (player.InputHandler.RawMovementInput.x == 0)
            {
                stateMachine.ChangeState(player.IdleState);
            }
            else
            {
                stateMachine.ChangeState(player.MoveState);
            }
        }
    }
}