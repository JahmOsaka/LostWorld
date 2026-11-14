using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private bool hasJumped;
    private bool hasAppliedJumpCut;
    private float timer;
    private bool bufferedDoubleJump;

    public PlayerJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        hasAppliedJumpCut = false;
        bufferedDoubleJump = false;
        timer = 0f;

        player.InputHandler.UseJumpInput();
        player.ColliderController?.ActivateJumpCollider();

        bool isGroundJump = player.CheckIfGrounded() || player.CoyoteTimeCounter > 0f;

        if (isGroundJump)
        {
            player.ResetCoyoteTime();
            player.JumpsRemaining = (player.SkillController != null && player.SkillController.hasDoubleJump) ? 1 : 0;

            player.Anim.Play("Jump_Ground", 0, 0f);
            ApplyJumpPhysics();
        }
        else
        {
            player.JumpsRemaining--;
            player.Anim.Play("Jump_Air", 0, 0f);
            ApplyJumpPhysics();
        }
    }

    private void ApplyJumpPhysics()
    {
        hasJumped = true;
        player.RB.gravityScale = playerData.JumpGravityScale;
        player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, playerData.MaxJumpVelocity);
    }

    public void ExecuteJumpForce() { }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        timer += Time.deltaTime;

        if (player.InputHandler.JumpInput && player.SkillController.hasDoubleJump && player.JumpsRemaining > 0)
        {
            player.InputHandler.UseJumpInput();
            bufferedDoubleJump = true;
        }

        if (bufferedDoubleJump && timer >= 0.04f)
        {
            bufferedDoubleJump = false;
            player.JumpsRemaining--;

            timer = 0f;
            hasAppliedJumpCut = false;

            ApplyJumpPhysics();
            player.Anim.Play("Jump_Air", 0, 0f);
            return;
        }

        if (timer < 0.12f) return;

        if (player.CheckIfGrounded() && player.RB.linearVelocity.y <= 0.01f)
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

        if (!bufferedDoubleJump && !hasAppliedJumpCut && !player.InputHandler.IsJumpHolding && player.RB.linearVelocity.y > 0 && timer > playerData.minJumpHoldTime)
        {
            player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, player.RB.linearVelocity.y * playerData.minJumpMultiplier);
            hasAppliedJumpCut = true;
        }

        float inputX = player.InputHandler.RawMovementInput.x;
        float moveDirection = inputX == 0 ? 0 : Mathf.Sign(inputX);

        player.RB.linearVelocity = new Vector2(moveDirection * playerData.moveSpeed, player.RB.linearVelocity.y);
        player.CheckIfShouldFlip(moveDirection);
    }

    public override void Exit()
    {
        base.Exit();
        player.RB.gravityScale = playerData.defaultGravity;
    }
}