using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private float failsafeTimer;

    public PlayerAttackState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        failsafeTimer = 0f;
        player.Anim.speed = playerData.AttackSpeedMultiplier;

        player.InputHandler.UseAttackInput();
        player.RB.linearVelocity = Vector2.zero;

        if (Time.time >= player.CombatData.LastAttackTime + playerData.comboWindow)
        {
            player.CombatData.ResetCombo();
        }

        player.CombatData.RecordAttack();
        player.Anim.SetInteger("Combo", player.CombatData.ComboCounter);

        player.ShowWhipVisual();

        player.Anim.Update(0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        failsafeTimer += Time.deltaTime;

        if (player.InputHandler.DodgeInput && player.CombatData.DodgeCooldownTimer <= 0)
        {
            stateMachine.ChangeState(player.DodgeState);
            return;
        }

        if (player.InputHandler.JumpInput)
        {
            if (player.InputHandler.RawMovementInput.y < 0f && player.Sensor.IsStandingOnOneWay())
            {
                player.InputHandler.UseJumpInput();
                player.DropThroughPlatform();
                return;
            }
            else if (player.CheckIfGrounded() || player.JumpsRemaining > 0 || player.CoyoteTimeCounter > 0f)
            {
                stateMachine.ChangeState(player.JumpState);
                return;
            }
        }

        if (failsafeTimer >= playerData.attackDuration)
        {
            if (player.InputHandler.RawMovementInput.x != 0)
            {
                stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.Anim.speed = 1f;
        player.DisableHitbox();
        player.HideWhipVisual();
    }
}