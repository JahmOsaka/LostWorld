using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class Player : MonoBehaviour
{
    public PlayerData playerData;

    [Header("Ground & Ceiling Check Points")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private Transform ceilingCheckPoint;
    [SerializeField] private Vector2 ceilingCheckSize = new Vector2(0.6f, 0.1f);
    [SerializeField] private LayerMask whatIsGround;
    [Tooltip("The size of the box cast used for ground detection.")]
    public Vector2 boxCastSize = new Vector2(0.8f, 0.1f);
    [Tooltip("The distance of the box cast below the player.")]
    public float boxCastDistance = 0.1f;

    [Header("Combat Hitbox")]
    [SerializeField] private BoxCollider2D whipHitbox;

    // Components
    public Rigidbody2D RB { get; private set; }
    public BoxCollider2D Collider { get; private set; }
    public PlayerColliderController ColliderController { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public Animator Anim { get; private set; }

    public CoreHealth Health { get; private set; }

    [Header("Combat Settings")]
    public float combatIdleDuration = 4f;
    public int ComboCounter { get; private set; }
    public float LastAttackTime { get; set; }

    // State Machine
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }

    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerLandState LandState { get; private set; }
    public PlayerDodgeState DodgeState { get; private set; }
    public float DodgeCooldownTimer { get; private set; }
    public bool IsInvincible { get; private set; }
    public int FacingDirection { get; private set; } = 1;
    public float CoyoteTimeCounter { get; private set; }
    public float FallDuration { get; set; }

    public PlayerAttackState AttackState { get; private set; }

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Collider = GetComponent<BoxCollider2D>();
        InputHandler = GetComponent<PlayerInputHandler>();
        Anim = GetComponentInChildren<Animator>();
        StateMachine = new PlayerStateMachine();
        Health = GetComponent<CoreHealth>();
        ColliderController = GetComponent<PlayerColliderController>();
        RB.freezeRotation = true;

        // Initialize States
        IdleState = new PlayerIdleState(this, StateMachine, playerData, "idle");
        MoveState = new PlayerMoveState(this, StateMachine, playerData, "move");

        JumpState = new PlayerJumpState(this, StateMachine, playerData, "jump");
        FallState = new PlayerFallState(this, StateMachine, playerData, "fall");
        LandState = new PlayerLandState(this, StateMachine, playerData, "land");

        DodgeState = new PlayerDodgeState(this, StateMachine, playerData, "dodge");

        AttackState = new PlayerAttackState(this, StateMachine, playerData, "attack");
    }

    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentState.LogicUpdate();
        Anim.SetBool("isGrounded", CheckIfGrounded());

        if (DodgeCooldownTimer > 0)
        {
            DodgeCooldownTimer -= Time.deltaTime;
        }

        if (CheckIfGrounded())
        {
            CoyoteTimeCounter = playerData.coyoteTime;
        }
        else
        {
            CoyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }

    public bool CheckIfGrounded()
    {
        if (RB.linearVelocity.y > 0.05f) return false;
        if (groundCheckPoint == null) return false;

        return Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, whatIsGround) != null;
    }
    public bool CheckIfTouchingCeiling()
    {
        if (ceilingCheckPoint == null) return false;

        RaycastHit2D hit = Physics2D.BoxCast(ceilingCheckPoint.position, ceilingCheckSize, 0f, Vector2.up, boxCastDistance, whatIsGround);
        return hit.collider != null;
    }

    public bool CheckIfTouchingWall()
    {
        BoxCollider2D currentCollider = ActiveCollider;
        if (currentCollider == null) return false;

        Vector2 boxSize = new Vector2(0.1f, currentCollider.bounds.size.y * 0.8f);
        Vector2 direction = FacingDirection == 1 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.BoxCast(currentCollider.bounds.center, boxSize, 0f, direction, 0.1f, whatIsGround);
        return hit.collider != null;
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
    public void ResetDodgeCooldown() => DodgeCooldownTimer = playerData.DodgeCooldown;
    public void SetInvincible(bool invincible)
    {
        if (Health != null)
        {
            Health.SetInvincible(invincible);
        }
    }
    public void ResetCoyoteTime() => CoyoteTimeCounter = 0f;
    
    public void RecordAttack()
    {
        ComboCounter++;
        if (ComboCounter > 3) ComboCounter = 1;
    }

    public void ResetCombo()
    {
        ComboCounter = 0;
    
    }

    public void FinishAttack()
    {
        LastAttackTime = Time.time;
        StateMachine.ChangeState(IdleState);
    }

    public void EnableHitbox()
    {
        if (whipHitbox != null)
        {
            whipHitbox.gameObject.SetActive(true);
        }
    }
    public void DisableHitbox()
    {
        if (whipHitbox != null)
        {
            whipHitbox.gameObject.SetActive(false);
        }
    }

    public void TriggerCombatState()
    {
        LastAttackTime = Time.time;
    }

    public void TriggerDodgeFinishEvent()
    {
        if (StateMachine.CurrentState == DodgeState)
        {
            DodgeState.FinishDodgeAnimation();
        }
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

    private void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 groundCastPos = groundCheckPoint.position + (Vector3)(Vector2.down * boxCastDistance);
            Gizmos.DrawWireCube(groundCastPos, groundCheckSize);
        }

        if (ceilingCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 ceilingCastPos = ceilingCheckPoint.position + (Vector3)(Vector2.up * boxCastDistance);
            Gizmos.DrawWireCube(ceilingCastPos, ceilingCheckSize);
        }
    }
}