using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private float failsafeTimer;
    private float attackDuration = 0.5f;

    public PlayerAttackState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        failsafeTimer = 0f;
        player.Anim.speed = playerData.AttackSpeedMultiplier;

        player.InputHandler.UseAttackInput();
        player.RB.linearVelocity = Vector2.zero;

        if (Time.time >= player.LastAttackTime + playerData.comboWindow)
        {
            player.ResetCombo();
        }

        player.RecordAttack();
        player.Anim.SetInteger("Combo", player.ComboCounter);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        failsafeTimer += Time.deltaTime;

        if (player.InputHandler.DodgeInput && player.DodgeCooldownTimer <= 0)
        {
            stateMachine.ChangeState(player.DodgeState);
            return;
        }

        if (player.InputHandler.JumpInput && player.CheckIfGrounded())
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (failsafeTimer >= attackDuration)
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
    }
}