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
        timer = 0f;

        bool restored = player.ColliderController != null && player.ColliderController.TryRestoreStandingCollider();
        if (restored)
        {
            player.RB.linearVelocity = Vector2.zero;
        }

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

        if (player.ColliderController != null && player.ColliderController.IsUsingSmallCollider)
        {
            if (!player.CheckIfGrounded())
            {
                stateMachine.ChangeState(player.FallState);
            }
            return;
        }

        float inputX = player.InputHandler.RawMovementInput.x;

        if (player.InputHandler.JumpInput)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }
        if (player.InputHandler.AttackInput)
        {
            stateMachine.ChangeState(player.AttackState);
            return;
        }
        if (player.InputHandler.DodgeInput && player.DodgeCooldownTimer <= 0)
        {
            stateMachine.ChangeState(player.DodgeState);
            return;
        }

        if (!isHardLanding && inputX != 0f)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
        }

        float exitTime = isHardLanding ? playerData.hardLandingExitTime : playerData.softLandingExitTime;

        if (timer >= exitTime)
        {
            if (inputX != 0f)
                stateMachine.ChangeState(player.MoveState);
            else
                stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (player.ColliderController != null)
        {
            bool restored = player.ColliderController.TryRestoreStandingCollider();
            if (!restored)
            {
                player.ColliderController.HandleAutoEject(player.RB, player.FacingDirection);
            }
        }
    }
}