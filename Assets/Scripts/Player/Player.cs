using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerSensor))]
public class Player : MonoBehaviour
{
    [Header("Core Data (Balance Here)")]
    public PlayerData playerData;

    [Header("Combat References")]
    [SerializeField] private BoxCollider2D whipHitbox;
    [SerializeField] private SpriteRenderer whipSpriteRenderer;
    [SerializeField] private SpriteRenderer vfxSpriteRenderer;

    private int originalLayer;
    private int passThroughLayer;
    private float dropDownTimer;

    // Components
    public Rigidbody2D RB { get; private set; }
    public BoxCollider2D Collider { get; private set; }
    public PlayerColliderController ColliderController { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public Animator Anim { get; private set; }
    public CoreHealth Health { get; private set; }
    public PlayerSkillController SkillController { get; private set; }
    public PlayerSensor Sensor { get; private set; }
    public bool IsInsideOneWay { get; private set; }

    // Runtime Data
    public PlayerCombatData CombatData { get; private set; } = new PlayerCombatData();

    public int FacingDirection { get; private set; } = 1;
    public float CoyoteTimeCounter { get; private set; }
    public float FallDuration { get; set; }
    public int JumpsRemaining { get; set; }

    // State Machine
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerLandState LandState { get; private set; }
    public PlayerDodgeState DodgeState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerSkillState SkillState { get; private set; }
    public PlayerSuperDashState SuperDashState { get; private set; }

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Collider = GetComponent<BoxCollider2D>();
        InputHandler = GetComponent<PlayerInputHandler>();
        Anim = GetComponentInChildren<Animator>();
        Health = GetComponent<CoreHealth>();
        ColliderController = GetComponent<PlayerColliderController>();
        SkillController = GetComponentInChildren<PlayerSkillController>();
        Sensor = GetComponent<PlayerSensor>();

        StateMachine = new PlayerStateMachine();
        RB.freezeRotation = true;
        originalLayer = gameObject.layer;
        passThroughLayer = LayerMask.NameToLayer("PlayerPassThrough");

        IdleState = new PlayerIdleState(this, StateMachine, playerData, "idle");
        MoveState = new PlayerMoveState(this, StateMachine, playerData, "move");
        JumpState = new PlayerJumpState(this, StateMachine, playerData, "jump");
        FallState = new PlayerFallState(this, StateMachine, playerData, "fall");
        LandState = new PlayerLandState(this, StateMachine, playerData, "land");
        DodgeState = new PlayerDodgeState(this, StateMachine, playerData, "dodge");
        AttackState = new PlayerAttackState(this, StateMachine, playerData, "attack");
        SkillState = new PlayerSkillState(this, StateMachine, playerData, "skill");
        SuperDashState = new PlayerSuperDashState(this, StateMachine, playerData, "superdash");
    }

    private void Start()
    {
        HideWhipVisual();
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentState.LogicUpdate();

        bool groundedForAnim = CheckIfGrounded() && StateMachine.CurrentState != JumpState;
        Anim.SetBool("isGrounded", groundedForAnim);

        CombatData.TickDodgeCooldown(Time.deltaTime);

        if (groundedForAnim)
        {
            CoyoteTimeCounter = playerData.coyoteTime;
            JumpsRemaining = SkillController != null && SkillController.hasDoubleJump ? 1 : 0;
        }
        else
        {
            CoyoteTimeCounter -= Time.deltaTime;
        }

        HandleOneWayPlatforms();
    }

    private void FixedUpdate() => StateMachine.CurrentState.PhysicsUpdate();

    public bool CheckIfGrounded()
    {
        if (StateMachine.CurrentState == JumpState && RB.linearVelocity.y > 0.05f) return false;

        if (dropDownTimer > 0f || IsInsideOneWay) return false;

        return Sensor.CheckIfGrounded();
    }

    public void CheckIfShouldFlip(float xInput)
    {
        if (xInput != 0 && xInput != FacingDirection)
        {
            FacingDirection *= -1;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    public void ResetDodgeCooldown() => CombatData.DodgeCooldownTimer = playerData.DodgeCooldown;
    public void SetInvincible(bool invincible) => Health?.SetInvincible(invincible);
    public void ResetCoyoteTime() => CoyoteTimeCounter = 0f;

    public void FinishAttack()
    {
        CombatData.LastAttackTime = Time.time;
        if (InputHandler.RawMovementInput.x != 0)
            StateMachine.ChangeState(MoveState);
        else
            StateMachine.ChangeState(IdleState);
    }

    public void EnableHitbox() => whipHitbox?.gameObject.SetActive(true);
    public void DisableHitbox() => whipHitbox?.gameObject.SetActive(false);
    public void TriggerCombatState() => CombatData.TriggerCombatState();

    public void TriggerDodgeFinishEvent()
    {
        if (StateMachine.CurrentState == DodgeState)
            DodgeState.FinishDodgeAnimation();
    }

    public void ShowWhipVisual()
    {
        if (whipSpriteRenderer != null) { whipSpriteRenderer.sprite = null; whipSpriteRenderer.enabled = true; }
        if (vfxSpriteRenderer != null) { vfxSpriteRenderer.sprite = null; vfxSpriteRenderer.enabled = true; }
    }

    public void HideWhipVisual()
    {
        if (whipSpriteRenderer != null) { whipSpriteRenderer.enabled = false; whipSpriteRenderer.sprite = null; }
        if (vfxSpriteRenderer != null) { vfxSpriteRenderer.enabled = false; vfxSpriteRenderer.sprite = null; }
    }
    public void PassThroughPlatformUpwards()
    {
        dropDownTimer = 0.2f;
    }
    public void DropThroughPlatform()
    {
        if (Sensor.IsStandingOnOneWay())
        {
            dropDownTimer = 0.25f;
            ResetCoyoteTime();
            StateMachine.ChangeState(FallState);
        }
    }

    private void HandleOneWayPlatforms()
    {
        if (dropDownTimer > 0)
        {
            dropDownTimer -= Time.deltaTime;
            gameObject.layer = passThroughLayer;
            return;
        }

        if (RB.linearVelocity.y > 0.05f)
        {
            gameObject.layer = passThroughLayer;
            return;
        }

        IsInsideOneWay = Sensor.IsInsideOneWayPlatform(ActiveCollider);
        if (IsInsideOneWay)
        {
            gameObject.layer = passThroughLayer;
            return;
        }

        gameObject.layer = originalLayer;
    }

    public BoxCollider2D ActiveCollider
    {
        get
        {
            if (ColliderController != null)
            {
                if (ColliderController.IsUsingSmallCollider && ColliderController.jumpRollCollider != null)
                    return ColliderController.jumpRollCollider;
                if (ColliderController.standingCollider != null)
                    return ColliderController.standingCollider;
            }
            return Collider;
        }
    }
}