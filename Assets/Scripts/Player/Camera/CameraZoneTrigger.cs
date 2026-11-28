using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Zone Camera")]
    [Tooltip("CinemachineCamera dedicated to this zone (set its Priority higher than the main camera, e.g. 20).")]
    [SerializeField] private CinemachineCamera zoneCamera;

    [Header("Optional Axis Lock (Good for Horizontal Platforming)")]
    [Tooltip("If true, the zone camera follows the player horizontally (X) but locks vertical (Y) to the zone center or custom Y.")]
    [SerializeField] private bool lockY = true;
    [Tooltip("If true, uses this trigger box's Y center as the locked Y height.")]
    [SerializeField] private bool useZoneCenterY = true;
    [SerializeField] private float customLockedY = 0f;
    [Tooltip("Horizontal offset ahead of the player (e.g. +2 to see platforms on the right).")]
    [SerializeField] private float offsetX = 0f;

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.85f, 1f, 0.22f);

    private BoxCollider2D boxCollider;
    private Transform playerTransform;
    private Transform lockProxy;
    private bool isPlayerInside;

    private void Reset()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.size = new Vector2(20f, 10f);
        }
    }

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null) boxCollider.isTrigger = true;

        if (zoneCamera != null)
        {
            zoneCamera.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (lockProxy != null)
        {
            Destroy(lockProxy.gameObject);
        }
    }

    private void OnDisable()
    {
        SetZoneActive(false);
    }

    private void LateUpdate()
    {
        if (!isPlayerInside || !lockY || lockProxy == null || playerTransform == null) return;

        float targetY = useZoneCenterY && boxCollider != null
            ? boxCollider.bounds.center.y
            : customLockedY;

        lockProxy.position = new Vector3(
            playerTransform.position.x + offsetX,
            targetY,
            playerTransform.position.z
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isPlayerInside) return;

        playerTransform = other.transform.root;
        SetupZoneTarget();
        SetZoneActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !isPlayerInside) return;
        if (playerTransform != null && other.transform.root != playerTransform) return;

        SetZoneActive(false);
    }

    private void SetupZoneTarget()
    {
        if (zoneCamera == null || playerTransform == null) return;

        if (lockY)
        {
            if (lockProxy == null)
            {
                GameObject proxyObj = new GameObject($"{name}_CameraProxy");
                lockProxy = proxyObj.transform;
            }

            float targetY = useZoneCenterY && boxCollider != null
                ? boxCollider.bounds.center.y
                : customLockedY;

            lockProxy.position = new Vector3(
                playerTransform.position.x + offsetX,
                targetY,
                playerTransform.position.z
            );

            zoneCamera.Follow = lockProxy;
        }
        else if (zoneCamera.Follow == null)
        {
            zoneCamera.Follow = playerTransform;
        }
    }

    private void SetZoneActive(bool active)
    {
        isPlayerInside = active;
        if (zoneCamera != null)
        {
            zoneCamera.enabled = active;
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D col = boxCollider != null ? boxCollider : GetComponent<BoxCollider2D>();
        if (col == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(col.offset, col.size);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.9f);
        Gizmos.DrawWireCube(col.offset, col.size);
    }
}