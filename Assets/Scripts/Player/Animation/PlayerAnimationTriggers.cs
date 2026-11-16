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
        if (player.StateMachine.CurrentState is PlayerAttackState attackState)
        {
            attackState.ExitAttackState();
        }
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

    private void ShowWhipVisual()
    {
        player.ShowWhipVisual();
    }

    private void HideWhipVisual()
    {
        player.HideWhipVisual();
    }

    private void ShowVFXVisual()
    {
        player.ShowVFXVisual();
    }

    private void HideVFXVisual()
    {
        player.HideVFXVisual();
    }
}