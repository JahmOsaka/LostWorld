using UnityEngine;

public class PlayerSkillState : PlayerState
{
    private SkillData currentSkill;
    private float failsafeTimer;

    public PlayerSkillState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public void SetSkill(SkillData skill) => currentSkill = skill;

    public override void Enter()
    {
        base.Enter();
        failsafeTimer = 0f;
        player.RB.linearVelocity = Vector2.zero;
        currentSkill?.ExecuteSkill(player.gameObject);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        failsafeTimer += Time.deltaTime;
        if (failsafeTimer >= (currentSkill != null ? currentSkill.castDuration : 0.5f))
        {
            stateMachine.ChangeState(player.InputHandler.RawMovementInput.x != 0 ? player.MoveState : player.IdleState);
        }
    }
}