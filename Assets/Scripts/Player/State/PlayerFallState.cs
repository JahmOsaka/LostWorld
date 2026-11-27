using UnityEngine;

public class PlayerFallState : PlayerState
{
    public PlayerFallState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        player.FallDuration = 0f;
        player.Anim.Play("Jump_Air", 0, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.FallDuration += Time.deltaTime;

        if (player.InputHandler.AttackInput && player.CombatData.CanAirAttack)
        {
            player.InputHandler.UseAttackInput();
            stateMachine.ChangeState(player.AttackState);
            return;
        }

        if (player.InputHandler.SuperDashInput && player.SkillController != null && player.SkillController.hasSuperDash)
        {
            stateMachine.ChangeState(player.SuperDashState);
            return;
        }

        if (player.InputHandler.JumpInput && (player.CheckIfGrounded() || player.JumpsRemaining > 0 || player.CoyoteTimeCounter > 0f))
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

        bool isStuck = false;
        if (player.ColliderController != null)
        {
            bool restored = player.ColliderController.TryRestoreStandingCollider();
            if (!restored)
            {
                isStuck = true;
                player.ColliderController.HandleAutoEject(player.RB, player.FacingDirection);
            }
        }

        float velocityY = player.RB.linearVelocity.y;
        if (velocityY < 0f)
        {
            velocityY = Mathf.Max(velocityY, -playerData.maxFallSpeed);
        }

        if (!isStuck)
        {
            float inputX = player.InputHandler.RawMovementInput.x;
            float moveDirection = inputX == 0 ? 0 : Mathf.Sign(inputX);

            player.RB.linearVelocity = new Vector2(moveDirection * playerData.moveSpeed, velocityY);
            player.CheckIfShouldFlip(moveDirection);
        }
        else
        {
            player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, velocityY);
        }
    }
}