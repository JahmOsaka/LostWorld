using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private float failsafeTimer;
    public PlayerAttackState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        player.Anim.speed = playerData.AttackSpeedMultiplier;

        player.InputHandler.UseAttackInput();
        player.RB.linearVelocity = Vector2.zero;

        if (Time.time >= player.LastAttackTime + playerData.comboWindow)
        {
            player.ResetCombo();
        }

        player.RecordAttack();

        player.Anim.SetInteger("Combo", player.ComboCounter);

        failsafeTimer = 0.5f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        failsafeTimer -= Time.deltaTime;
        if (failsafeTimer <= 0)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        player.DisableHitbox();
    }
}
