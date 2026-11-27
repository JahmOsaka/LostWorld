using System.Collections.Generic;
using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    private Player player;
    private PlayerData data;

    [Header("Detection Points")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Transform ceilingCheckPoint;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask oneWayPlatformLayer;

    [Header("Safe Ground Respawn")]
    [SerializeField] private float respawnOffsetY = 0.1f;
    [SerializeField] private float safeLedgePadding = 0.25f;

    private readonly List<Collider2D> groundOverlapResults = new List<Collider2D>(8);
    private ContactFilter2D groundFilter;

    public Vector3 LastSafePosition { get; private set; }
    public bool HasSafePosition { get; private set; }
    public LayerMask OneWayPlatformLayer => oneWayPlatformLayer;
    private LayerMask SolidGroundMask => whatIsGround & ~oneWayPlatformLayer;

    private void Awake()
    {
        player = GetComponent<Player>();
        data = player.playerData;
        OnValidate();

        groundFilter = new ContactFilter2D();
        groundFilter.SetLayerMask(whatIsGround);
        groundFilter.useLayerMask = true;
        groundFilter.useTriggers = false;

        LastSafePosition = transform.position;
        HasSafePosition = true;
    }

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
    }

    private void FixedUpdate()
    {
        if (player != null && player.RB != null && Mathf.Abs(player.RB.linearVelocity.y) > 0.1f)
            return;

        if (CheckIfFullyOnSolidGround())
        {
            LastSafePosition = transform.position + Vector3.up * respawnOffsetY;
            HasSafePosition = true;
        }
    }

    public bool CheckIfGrounded(List<Collider2D> ignoredColliders = null)
    {
        if (groundCheckPoint == null || data == null) return false;
        Vector2 groundCastPos = (Vector2)groundCheckPoint.position + (Vector2.down * data.boxCastDistance);

        if (ignoredColliders == null || ignoredColliders.Count == 0)
        {
            return Physics2D.OverlapBox(groundCastPos, data.groundCheckSize, 0f, whatIsGround) != null;
        }

        groundFilter.SetLayerMask(whatIsGround);
        int count = Physics2D.OverlapBox(groundCastPos, data.groundCheckSize, 0f, groundFilter, groundOverlapResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = groundOverlapResults[i];
            if (col != null && !ignoredColliders.Contains(col))
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckIfTouchingCeiling()
    {
        if (ceilingCheckPoint == null || data == null) return false;
        RaycastHit2D hit = Physics2D.BoxCast(ceilingCheckPoint.position, data.ceilingCheckSize, 0f, Vector2.up, data.boxCastDistance, SolidGroundMask);
        return hit.collider != null;
    }

    public bool CheckIfTouchingWall(BoxCollider2D currentCollider, int facingDirection)
    {
        if (currentCollider == null) return false;
        Bounds bounds = currentCollider.bounds;
        Vector2 boxSize = new Vector2(0.1f, Mathf.Max(0.05f, bounds.size.y * 0.8f));
        Vector2 direction = facingDirection == 1 ? Vector2.right : Vector2.left;
        float distance = bounds.extents.x + 0.05f;
        RaycastHit2D hit = Physics2D.BoxCast(bounds.center, boxSize, 0f, direction, distance, SolidGroundMask);
        return hit.collider != null;
    }

    public Collider2D[] GetOneWayPlatformsUnderFeet()
    {
        if (groundCheckPoint == null || data == null) return System.Array.Empty<Collider2D>();
        Vector2 groundCastPos = (Vector2)groundCheckPoint.position + (Vector2.down * data.boxCastDistance);
        return Physics2D.OverlapBoxAll(groundCastPos, data.groundCheckSize, 0f, oneWayPlatformLayer);
    }

    public bool IsStandingOnOneWay()
    {
        if (groundCheckPoint == null || data == null) return false;
        Vector2 groundCastPos = (Vector2)groundCheckPoint.position + (Vector2.down * data.boxCastDistance);
        return Physics2D.OverlapBox(groundCastPos, data.groundCheckSize, 0f, oneWayPlatformLayer) != null;
    }

    public bool CheckIfSolidGrounded()
    {
        if (groundCheckPoint == null || data == null) return false;
        Vector2 groundCastPos = (Vector2)groundCheckPoint.position + (Vector2.down * data.boxCastDistance);
        return Physics2D.OverlapBox(groundCastPos, data.groundCheckSize, 0f, SolidGroundMask) != null;
    }

    public bool IsInsideOneWayPlatform(BoxCollider2D currentCollider)
    {
        if (currentCollider == null) return false;

        Vector2 boxSize = currentCollider.bounds.size;
        boxSize.y = Mathf.Max(0.05f, boxSize.y - 0.3f);
        boxSize.x += 0.25f;

        Vector2 boxCenter = currentCollider.bounds.center;
        boxCenter.y += 0.15f;

        return Physics2D.OverlapBox(boxCenter, boxSize, 0f, oneWayPlatformLayer) != null;
    }

    public bool CheckIfFullyOnSolidGround()
    {
        if (groundCheckPoint == null || data == null) return false;

        float halfWidth = (data.groundCheckSize.x * 0.5f) + safeLedgePadding;
        float rayDistance = data.boxCastDistance + (data.groundCheckSize.y * 0.5f) + 0.05f;
        Vector2 originCenter = groundCheckPoint.position;

        RaycastHit2D hitLeft = Physics2D.Raycast(originCenter + Vector2.left * halfWidth, Vector2.down, rayDistance, SolidGroundMask);
        RaycastHit2D hitRight = Physics2D.Raycast(originCenter + Vector2.right * halfWidth, Vector2.down, rayDistance, SolidGroundMask);

        return hitLeft.collider != null && hitRight.collider != null;
    }

    public void SetManualSafePosition(Vector3 position)
    {
        LastSafePosition = position;
        HasSafePosition = true;
    }

    private void OnDrawGizmos()
    {
        if (data == null) return;

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 groundCastPos = groundCheckPoint.position + (Vector3)(Vector2.down * data.boxCastDistance);
            Gizmos.DrawWireCube(groundCastPos, data.groundCheckSize);
        }

        if (ceilingCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 ceilingCastPos = ceilingCheckPoint.position + (Vector3)(Vector2.up * data.boxCastDistance);
            Gizmos.DrawWireCube(ceilingCastPos, data.ceilingCheckSize);
        }

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            float halfWidth = data.groundCheckSize.x * 0.5f;
            float rayDistance = data.boxCastDistance + (data.groundCheckSize.y * 0.5f) + 0.05f;
            Vector3 leftOrigin = groundCheckPoint.position + Vector3.left * halfWidth;
            Vector3 rightOrigin = groundCheckPoint.position + Vector3.right * halfWidth;
            Gizmos.DrawLine(leftOrigin, leftOrigin + Vector3.down * rayDistance);
            Gizmos.DrawLine(rightOrigin, rightOrigin + Vector3.down * rayDistance);
        }
    }
}