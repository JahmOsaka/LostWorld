using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class Player : MonoBehaviour
{
    [Header("Core Data (Balance Here)")]
    public PlayerData playerData;

    [Header("Environment Detection (Auto-Filled if named GroundCheck/CeilingCheck)")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Transform ceilingCheckPoint;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Combat References")]
    [SerializeField] private BoxCollider2D whipHitbox;
    [SerializeField] private SpriteRenderer whipSpriteRenderer;

    public Rigidbody2D RB { get; private set; }
    public BoxCollider2D Collider { get; private set; }
    public PlayerColliderController ColliderController { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public Animator Anim { get; private set; }
    public CoreHealth Health { get; private set; }

    public int ComboCounter { get; private set; }
    public float LastAttackTime { get; set; } = -999f;
    public float DodgeCooldownTimer { get; private set; }
    public bool IsInvincible { get; private set; }
    public int FacingDirection { get; private set; } = 1;
    public float CoyoteTimeCounter { get; private set; }
    public float FallDuration { get; set; }

    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerLandState LandState { get; private set; }
    public PlayerDodgeState DodgeState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }

    private void OnValidate()
    {
        if (groundCheckPoint == null)
        {
            Transform found = transform.Find("GroundCheck") ?? transform.Find("GroundCheckPoint");
            if (found != null) groundCheckPoint = found;
        }
        if (ceilingCheckPoint == null)
        {
            Transform found = transform.Find("CeilingCheck") ?? transform.Find("CeilingCheckPoint");
            if (found != null) ceilingCheckPoint = found;
        }
        if (whipHitbox == null)
        {
            WeaponHitbox weapon = GetComponentInChildren<WeaponHitbox>(true);
            if (weapon != null) whipHitbox = weapon.GetComponent<BoxCollider2D>();
        }
        if (whipSpriteRenderer == null)
        {
            Transform whipTransform = transform.Find("PlayerVisual/Whip") ?? transform.Find("Whip");
            if (whipTransform != null)
                whipSpriteRenderer = whipTransform.GetComponent<SpriteRenderer>();
        }
    }

    private void Awake()
    {
        OnValidate();
        RB = GetComponent<Rigidbody2D>();
        Collider = GetComponent<BoxCollider2D>();
        InputHandler = GetComponent<PlayerInputHandler>();
        Anim = GetComponentInChildren<Animator>();
        Health = GetComponent<CoreHealth>();
        ColliderController = GetComponent<PlayerColliderController>();
        StateMachine = new PlayerStateMachine();
        RB.freezeRotation = true;

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
        HideWhipVisual();
    }

    private void Update()
    {
        StateMachine.CurrentState.LogicUpdate();

        bool groundedForAnim = CheckIfGrounded() && StateMachine.CurrentState != JumpState;
        Anim.SetBool("isGrounded", groundedForAnim);

        if (DodgeCooldownTimer > 0) DodgeCooldownTimer -= Time.deltaTime;

        if (CheckIfGrounded())
            CoyoteTimeCounter = playerData.coyoteTime;
        else
            CoyoteTimeCounter -= Time.deltaTime;
    }

    private void FixedUpdate() => StateMachine.CurrentState.PhysicsUpdate();

    public bool CheckIfGrounded()
    {
        if (RB.linearVelocity.y > 0.05f || groundCheckPoint == null || playerData == null) return false;

        Vector2 groundCastPos = (Vector2)groundCheckPoint.position + (Vector2.down * playerData.boxCastDistance);
        return Physics2D.OverlapBox(groundCastPos, playerData.groundCheckSize, 0f, whatIsGround) != null;
    }

    public bool CheckIfTouchingCeiling()
    {
        if (ceilingCheckPoint == null || playerData == null) return false;

        RaycastHit2D hit = Physics2D.BoxCast(ceilingCheckPoint.position, playerData.ceilingCheckSize, 0f, Vector2.up, playerData.boxCastDistance, whatIsGround);
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
    public void SetInvincible(bool invincible) => Health?.SetInvincible(invincible);
    public void ResetCoyoteTime() => CoyoteTimeCounter = 0f;

    public void RecordAttack()
    {
        ComboCounter++;
        if (ComboCounter > 3) ComboCounter = 1;
    }

    public void ResetCombo() => ComboCounter = 0;

    public void FinishAttack()
    {
        LastAttackTime = Time.time;
        StateMachine.ChangeState(IdleState);
    }

    public void EnableHitbox() => whipHitbox?.gameObject.SetActive(true);
    public void DisableHitbox() => whipHitbox?.gameObject.SetActive(false);
    public void TriggerCombatState() => LastAttackTime = Time.time;

    public void TriggerDodgeFinishEvent()
    {
        if (StateMachine.CurrentState == DodgeState)
            DodgeState.FinishDodgeAnimation();
    }

    public void ShowWhipVisual()
    {
        if (whipSpriteRenderer != null) whipSpriteRenderer.enabled = true;
    }

    public void HideWhipVisual()
    {
        if (whipSpriteRenderer != null)
        {
            whipSpriteRenderer.enabled = false;
            whipSpriteRenderer.sprite = null;
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
        if (playerData == null) return;

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 groundCastPos = groundCheckPoint.position + (Vector3)(Vector2.down * playerData.boxCastDistance);
            Gizmos.DrawWireCube(groundCastPos, playerData.groundCheckSize);
        }

        if (ceilingCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 ceilingCastPos = ceilingCheckPoint.position + (Vector3)(Vector2.up * playerData.boxCastDistance);
            Gizmos.DrawWireCube(ceilingCastPos, playerData.ceilingCheckSize);
        }
    }
}