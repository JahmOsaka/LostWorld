using UnityEngine;

public class PlayerLandState : PlayerState
{
    private bool isHardLanding;
    private float timer;

    public PlayerLandState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        player.RB.linearVelocity = Vector2.zero;
        timer = 0f;

        if (player.FallDuration >= playerData.hardLandingThreshold)
        {
            isHardLanding = true;
            player.Anim.SetFloat("LandingSpeed", playerData.hardLandingAnimSpeed);
        }
        else
        {
            isHardLanding = false;
            player.Anim.SetFloat("LandingSpeed", playerData.softLandingAnimSpeed);
        }

        player.FallDuration = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        timer += Time.deltaTime;

        float inputX = player.InputHandler.RawMovementInput.x;

        if (player.InputHandler.JumpInput)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (!isHardLanding && inputX != 0f)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
        }

        float exitTime = isHardLanding ? 0.4f : 0.1f;

        if (timer >= exitTime)
        {
            if (inputX != 0f)
            {
                stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }
}