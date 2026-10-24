using UnityEngine;

public class PlayerColliderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider2D physicsCollider;
    [SerializeField] private PlayerHurtbox playerHurtbox;

    [Tooltip("Assign the solid ground/wall layers here. Do NOT include One-Way Platforms.")]
    [SerializeField] private LayerMask solidGroundLayer;

    [Header("Jump Shrunk Settings (Physics)")]
    [SerializeField] private Vector2 jumpPhysicsSize = new Vector2(0.6f, 1.2f);
    [SerializeField] private Vector2 jumpPhysicsOffset = new Vector2(0f, 0.3f);

    [Header("Jump Shrunk Settings (Hurtbox)")]
    [SerializeField] private Vector2 jumpHurtboxSize = new Vector2(0.5f, 1.1f);
    [SerializeField] private Vector2 jumpHurtboxOffset = new Vector2(0f, 0.3f);

    [Header("Dodge Shrunk Settings (Hurtbox)")]
    [SerializeField] private Vector2 dodgeHurtboxSize = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 dodgeHurtboxOffset = new Vector2(0f, -0.3f);

    private Vector2 defaultPhysicsSize;
    private Vector2 defaultPhysicsOffset;
    private Vector2 defaultHurtboxSize;
    private Vector2 defaultHurtboxOffset;

    public bool IsPhysicsShrunk { get; private set; }

    private void Awake()
    {
        if (physicsCollider == null) physicsCollider = GetComponent<BoxCollider2D>();

        if (playerHurtbox == null) playerHurtbox = GetComponentInChildren<PlayerHurtbox>();
    }
    private void Start()
    {
        if (physicsCollider != null)
        {
            defaultPhysicsSize = physicsCollider.size;
            defaultPhysicsOffset = physicsCollider.offset;
        }

        if (playerHurtbox != null && playerHurtbox.BoxCollider != null)
        {
            defaultHurtboxSize = playerHurtbox.BoxCollider.size;
            defaultHurtboxOffset = playerHurtbox.BoxCollider.offset;
        }
    }
    public void ShrinkForJump()
    {
        if (physicsCollider != null)
        {
            physicsCollider.size = jumpPhysicsSize;
            physicsCollider.offset = jumpPhysicsOffset;
            IsPhysicsShrunk = true;
        }

        if (playerHurtbox != null)
        {
            playerHurtbox.SetHurtboxSize(jumpHurtboxSize, jumpHurtboxOffset);
        }
    }

    public void ShrinkHurtboxOnlyForDodge()
    {
        if (playerHurtbox != null)
        {
            playerHurtbox.SetHurtboxSize(dodgeHurtboxSize, dodgeHurtboxOffset);
        }
    }

    public void RestoreHurtbox()
    {
        if (playerHurtbox != null)
        {
            playerHurtbox.SetHurtboxSize(defaultHurtboxSize, defaultHurtboxOffset);
        }
    }

    public bool TryRestorePhysicsCollider()
    {
        if (!IsPhysicsShrunk || physicsCollider == null) return true;

        Vector2 targetCenter = (Vector2)physicsCollider.transform.position + defaultPhysicsOffset;
        Collider2D hit = Physics2D.OverlapBox(targetCenter, defaultPhysicsSize * 0.95f, 0f, solidGroundLayer);

        if (hit == null)
        {
            physicsCollider.size = defaultPhysicsSize;
            physicsCollider.offset = defaultPhysicsOffset;
            IsPhysicsShrunk = false;
            return true;
        }

        return false;
    }

    public void ForceRestoreAll()
    {
        RestoreHurtbox();
        if (physicsCollider != null)
        {
            physicsCollider.size = defaultPhysicsSize;
            physicsCollider.offset = defaultPhysicsOffset;
            IsPhysicsShrunk = false;
        }
    }
}