using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderController : MonoBehaviour
{
    [Header("Auto-Detected References")]
    [Tooltip("Automatically assigns the taller BoxCollider2D on this object.")]
    public BoxCollider2D standingCollider;
    [Tooltip("Automatically assigns the shorter BoxCollider2D on this object.")]
    public BoxCollider2D jumpRollCollider;
    [Tooltip("Automatically finds PlayerHurtbox in children.")]
    public PlayerHurtbox playerHurtbox;

    [Header("Ceiling Check Layer (Unity 6)")]
    [Tooltip("Solid ground/ceiling layer (Do NOT include OneWayPlatform).")]
    public LayerMask solidGroundLayer;

    private Player player;
    private ContactFilter2D ceilingFilter;
    private readonly List<Collider2D> overlapResults = new List<Collider2D>(4);

    public bool IsUsingSmallCollider { get; private set; }
    public bool IsStuckUnderCeiling { get; private set; }

    private void OnValidate()
    {
        AutoAssignReferences();
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        AutoAssignReferences();
    }

    private void AutoAssignReferences()
    {
        if (playerHurtbox == null)
            playerHurtbox = GetComponentInChildren<PlayerHurtbox>(true);

        if (standingCollider == null || jumpRollCollider == null)
        {
            BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
            if (colliders.Length >= 2)
            {
                if (colliders[0].size.y >= colliders[1].size.y)
                {
                    standingCollider = colliders[0];
                    jumpRollCollider = colliders[1];
                }
                else
                {
                    standingCollider = colliders[1];
                    jumpRollCollider = colliders[0];
                }
            }
        }
    }

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

        if (playerHurtbox != null && player != null && player.playerData != null)
        {
            playerHurtbox.SetHurtboxSize(player.playerData.jumpHurtboxSize, player.playerData.jumpHurtboxOffset);
        }
    }

    public void ShrinkHurtboxOnlyForDodge()
    {
        if (playerHurtbox != null && player != null && player.playerData != null)
        {
            playerHurtbox.SetHurtboxSize(player.playerData.dodgeHurtboxSize, player.playerData.dodgeHurtboxOffset);
        }
    }

    public void RestoreHurtbox() => playerHurtbox?.ResetHurtbox();

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
        if (!IsStuckUnderCeiling || jumpRollCollider == null) return;

        Vector2 origin = (Vector2)jumpRollCollider.transform.position + jumpRollCollider.offset;
        RaycastHit2D wallHit = Physics2D.Raycast(
            origin,
            Vector2.right * facingDirection,
            (jumpRollCollider.size.x * 0.5f) + 0.15f,
            solidGroundLayer
        );

        int ejectDir = (wallHit.collider != null) ? -facingDirection : facingDirection;
        float ejectSpeed = (player != null && player.playerData != null) ? player.playerData.autoEjectSpeed : 7f;

        rb.linearVelocity = new Vector2(ejectDir * ejectSpeed, rb.linearVelocity.y);
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