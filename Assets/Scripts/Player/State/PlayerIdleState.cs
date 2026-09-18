using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        player.RB.linearVelocity = new Vector2(0f, player.RB.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (Time.time - player.LastAttackTime < player.combatIdleDuration)
        {
            player.Anim.SetBool("isCombatIdle", true);
        }
        else
        {
            player.Anim.SetBool("isCombatIdle", false);
        }


        if (player.InputHandler.AttackInput)
        {
            stateMachine.ChangeState(player.AttackState);
        }
        else if (player.InputHandler.JumpInput && player.CheckIfGrounded())
        {
            stateMachine.ChangeState(player.JumpState);
        }
        else if (player.InputHandler.DodgeInput && player.DodgeCooldownTimer <= 0)
        {
            stateMachine.ChangeState(player.DodgeState);
        }

        else if (!player.CheckIfGrounded())
        {
            stateMachine.ChangeState(player.FallState);
        }

        else if (player.InputHandler.RawMovementInput.x != 0)
        {
            stateMachine.ChangeState(player.MoveState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        player.Anim.SetBool("isCombatIdle", false);
    }
}