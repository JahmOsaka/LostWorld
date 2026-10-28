using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderController : MonoBehaviour
{
    [Header("Two Separate Physics Colliders")]
    [Tooltip("Full-size collider for standing and running.")]
    public BoxCollider2D standingCollider;
    [Tooltip("Smaller collider aligned at the feet for jumping or rolling.")]
    public BoxCollider2D jumpRollCollider;

    [Header("Hurtbox Reference & Sizes")]
    public PlayerHurtbox playerHurtbox;
    public Vector2 jumpHurtboxSize = new Vector2(0.6f, 1.0f);
    public Vector2 jumpHurtboxOffset = new Vector2(0f, -0.3f);
    public Vector2 dodgeHurtboxSize = new Vector2(0.8f, 0.7f);
    public Vector2 dodgeHurtboxOffset = new Vector2(0f, -0.5f);

    [Header("Ceiling Check & Auto-Eject (Unity 6)")]
    [Tooltip("Solid ground/ceiling layer (Do NOT include OneWayPlatform).")]
    public LayerMask solidGroundLayer;
    [Tooltip("Horizontal push speed when stuck under a low ceiling.")]
    public float autoEjectSpeed = 7f;

    private ContactFilter2D ceilingFilter;
    private readonly List<Collider2D> overlapResults = new List<Collider2D>(4);

    public bool IsUsingSmallCollider { get; private set; }
    public bool IsStuckUnderCeiling { get; private set; }

    private void Start()
    {
        ceilingFilter = new ContactFilter2D();
        ceilingFilter.SetLayerMask(solidGroundLayer);
        ceilingFilter.useLayerMask = true;
        ceilingFilter.useTriggers = false;

        SetStandingColliderActive();
    }

    public void ActivateJumpCollider()
    {
        if (standingCollider != null && jumpRollCollider != null)
        {
            jumpRollCollider.enabled = true;
            standingCollider.enabled = false;
            IsUsingSmallCollider = true;
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
        playerHurtbox?.ResetHurtbox();
    }

    public bool TryRestoreStandingCollider()
    {
        if (!IsUsingSmallCollider || standingCollider == null)
        {
            IsStuckUnderCeiling = false;
            return true;
        }

        Vector2 targetCenter = (Vector2)standingCollider.transform.position + standingCollider.offset;

        Vector2 checkSize = new Vector2(standingCollider.size.x * 0.9f, standingCollider.size.y * 0.98f);

        int hitCount = Physics2D.OverlapBox(targetCenter, checkSize, 0f, ceilingFilter, overlapResults);

        if (hitCount == 0)
        {
            SetStandingColliderActive();
            RestoreHurtbox();
            IsStuckUnderCeiling = false;
            return true;
        }

        IsStuckUnderCeiling = true;
        return false;
    }

    public void HandleAutoEject(Rigidbody2D rb, int facingDirection)
    {
        if (!IsStuckUnderCeiling) return;

        Vector2 origin = (Vector2)jumpRollCollider.transform.position + jumpRollCollider.offset;
        RaycastHit2D wallHit = Physics2D.Raycast(
            origin,
            Vector2.right * facingDirection,
            (jumpRollCollider.size.x * 0.5f) + 0.15f,
            solidGroundLayer
        );

        int ejectDir = (wallHit.collider != null) ? -facingDirection : facingDirection;

        rb.linearVelocity = new Vector2(ejectDir * autoEjectSpeed, rb.linearVelocity.y);
    }

    private void SetStandingColliderActive()
    {
        if (standingCollider != null && jumpRollCollider != null)
        {
            standingCollider.enabled = true;
            jumpRollCollider.enabled = false;
            IsUsingSmallCollider = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (standingCollider == null) return;
        Gizmos.color = IsStuckUnderCeiling ? Color.red : Color.cyan;
        Vector2 targetCenter = (Vector2)standingCollider.transform.position + standingCollider.offset;
        Vector2 checkSize = new Vector2(standingCollider.size.x * 0.9f, standingCollider.size.y * 0.98f);
        Gizmos.DrawWireCube(targetCenter, checkSize);
    }
}