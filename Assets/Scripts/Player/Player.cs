using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    public PlayerData playerData;

    [Header("Ground Detection")]
    [Tooltip("Layer mask used to detect the ground.")]
    public LayerMask groundLayer;
    [Tooltip("The size of the box cast used for ground detection.")]
    public Vector2 boxCastSize = new Vector2(0.8f, 0.1f);
    [Tooltip("The distance of the box cast below the player.")]
    public float boxCastDistance = 0.1f;

    // Components
    public Rigidbody2D RB { get; private set; }
    public BoxCollider2D Collider { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public Animator Anim { get; private set; }

    // State Machine
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }

    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }

    public PlayerDodgeState DodgeState { get; private set; }
    public float DodgeCooldownTimer { get; private set; }
    public bool IsInvincible { get; private set; }

    public int FacingDirection { get; private set; } = 1;

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Collider = GetComponent<BoxCollider2D>();
        InputHandler = GetComponent<PlayerInputHandler>();
        Anim = GetComponent<Animator>();
        StateMachine = new PlayerStateMachine();

        RB.freezeRotation = true;

        // Initialize States
        IdleState = new PlayerIdleState(this, StateMachine, playerData, "idle");
        MoveState = new PlayerMoveState(this, StateMachine, playerData, "move");

        JumpState = new PlayerJumpState(this, StateMachine, playerData, "jump");
        FallState = new PlayerFallState(this, StateMachine, playerData, "fall");

        DodgeState = new PlayerDodgeState(this, StateMachine, playerData, "dodge");
    }

    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentState.LogicUpdate();

        if (DodgeCooldownTimer > 0)
        {
            DodgeCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }

    public bool CheckIfGrounded()
    {
        Vector2 origin = new Vector2(Collider.bounds.center.x, Collider.bounds.min.y);
        RaycastHit2D hit = Physics2D.BoxCast(origin, boxCastSize, 0f, Vector2.down, boxCastDistance, groundLayer);
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
    public void SetInvincible(bool invincible) => IsInvincible = invincible;

    private void OnDrawGizmos()
    {
        if (GetComponent<BoxCollider2D>() != null)
        {
            Gizmos.color = Color.red;
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y) + (Vector2.down * boxCastDistance);
            Gizmos.DrawWireCube(origin, boxCastSize);
        }
    }
}