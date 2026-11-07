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

    private void Awake()
    {
        player = GetComponent<Player>();
        data = player.playerData;
        OnValidate();
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

    public bool CheckIfGrounded()
    {
        if (groundCheckPoint == null || data == null) return false;
        Vector2 groundCastPos = (Vector2)groundCheckPoint.position + (Vector2.down * data.boxCastDistance);
        return Physics2D.OverlapBox(groundCastPos, data.groundCheckSize, 0f, whatIsGround) != null;
    }

    public bool CheckIfTouchingCeiling()
    {
        if (ceilingCheckPoint == null || data == null) return false;
        RaycastHit2D hit = Physics2D.BoxCast(ceilingCheckPoint.position, data.ceilingCheckSize, 0f, Vector2.up, data.boxCastDistance, whatIsGround);
        return hit.collider != null;
    }

    public bool CheckIfTouchingWall(BoxCollider2D currentCollider, int facingDirection)
    {
        if (currentCollider == null) return false;
        Vector2 boxSize = new Vector2(0.1f, currentCollider.bounds.size.y * 0.8f);
        Vector2 direction = facingDirection == 1 ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.BoxCast(currentCollider.bounds.center, boxSize, 0f, direction, 0.1f, whatIsGround);
        return hit.collider != null;
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
    }

    public bool IsStandingOnOneWay()
    {
        if (groundCheckPoint == null || data == null) return false;
        Vector2 groundCastPos = (Vector2)groundCheckPoint.position + (Vector2.down * data.boxCastDistance);
        return Physics2D.OverlapBox(groundCastPos, data.groundCheckSize, 0f, oneWayPlatformLayer) != null;
    }

    public bool IsInsideOneWayPlatform(BoxCollider2D currentCollider)
    {
        if (currentCollider == null) return false;

        Vector2 boxSize = currentCollider.bounds.size;
        boxSize.y -= 0.2f;
        boxSize.x -= 0.1f;

        Vector2 boxCenter = currentCollider.bounds.center;
        boxCenter.y += 0.1f;

        return Physics2D.OverlapBox(boxCenter, boxSize, 0f, oneWayPlatformLayer) != null;
    }
}