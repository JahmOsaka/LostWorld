using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void FinishAttack()
    {
        player.FinishAttack();
    }
    public void TriggerDodgeFinishEvent()
    {
        player.TriggerDodgeFinishEvent();
    }
    public void TriggerLandFinishEvent()
    {
        player.StateMachine.CurrentState.AnimationFinishTrigger();
    }
    public void TriggerJumpForceEvent()
    {
        if (player.StateMachine.CurrentState is PlayerJumpState jumpState)
        {
            jumpState.ExecuteJumpForce();
        }
    }
    private void EnableHitbox()
    {
        player.EnableHitbox();
    }

    private void DisableHitbox()
    {
        player.DisableHitbox();
    }
}