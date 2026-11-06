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

        float duration = 0.5f;
        if (currentSkill is ActiveSkillData activeSkill)
        {
            duration = activeSkill.castDuration;
        }

        if (failsafeTimer >= duration)
        {
            stateMachine.ChangeState(player.InputHandler.RawMovementInput.x != 0 ? player.MoveState : player.IdleState);
        }
    }
}