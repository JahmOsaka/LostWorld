public abstract class PlayerState
{
    protected Player player;
    protected PlayerStateMachine stateMachine;
    protected PlayerData playerData;
    protected string animBoolName;
    protected bool isAnimationFinished;

    public PlayerState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.playerData = playerData;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter() 
    {
        player.Anim.SetBool(animBoolName, true);
        isAnimationFinished = false;
    }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void Exit() 
    {
        player.Anim.SetBool(animBoolName, false);
    }
    public virtual void AnimationFinishTrigger()
    {
        isAnimationFinished = true;
    }
}