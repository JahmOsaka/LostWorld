using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public enum CameraState { Default, Fighting }

    [Header("Current State (Read Only)")]
    [SerializeField] private CameraState currentState = CameraState.Default;

    [Header("References")]
    public Player player;
    public PlayerInputHandler inputHandler;
    public Rigidbody2D playerRb;
    private CinemachineCamera cinemachineCamera;
    private CinemachinePositionComposer positionComposer;

    [Header("Zoom Settings (Orthographic Size)")]
    [Tooltip("Default zoom during exploration (Player ~1/4 of screen).")]
    public float defaultZoom = 5f;
    [Tooltip("Zoom multiplier during combat (1.3 - 1.5x of default).")]
    public float combatZoomMultiplier = 1.4f;
    [Tooltip("Fast zoom-out duration when entering combat (seconds).")]
    public float zoomOutTime = 0.3f;
    [Tooltip("Slow zoom-in duration when returning to exploration (seconds).")]
    public float zoomInTime = 1.5f;
    private float zoomVelocity;

    [Header("Custom Look-Ahead (Default State)")]
    [Tooltip("Horizontal distance the camera leads ahead of the player.")]
    public float lookAheadDistanceX = 2.5f;
    [Tooltip("Time in seconds for Look-Ahead to reach target (Higher = Smoother/Slower start).")]
    public float lookAheadSmoothTimeX = 0.5f;
    [Tooltip("Multiplier for Look-Ahead distance when standing still (1 = Keep full distance, 0.5 = Half).")]
    [Range(0f, 1f)] public float idleLookAheadRatio = 0.75f;

    [Header("Look Up/Down (Manual Pan)")]
    public float panDistanceY = 4f;
    [Tooltip("Time in seconds for vertical pan to reach target.")]
    public float panSmoothTimeY = 0.25f;

    private Vector3 offsetVelocity;

    [Header("Combat Detection & Weighted Midpoint")]
    [Tooltip("Layer mask for detecting active enemies.")]
    public LayerMask enemyLayer;
    [Tooltip("Radius around the player to detect enemies and trigger Fighting State.")]
    public float combatDetectionRadius = 10f;
    [Tooltip("Weight of the closest enemy position (0.4 = 60% Player, 40% Enemy).")]
    [Range(0f, 1f)] public float enemyWeight = 0.4f;
    [Tooltip("Maximum distance the camera is allowed to shift away from the player in combat.")]
    public Vector2 maxCombatOffset = new Vector2(4f, 2.5f);
    [Tooltip("Smoothing speed when shifting camera toward the enemy midpoint.")]
    public float combatOffsetSpeed = 5f;
    [Tooltip("Wait time after no enemies are nearby before returning to Default State.")]
    public float combatExitDelay = 1.5f;

    private float lastCombatTime;
    private Transform closestEnemy;

    [Header("Dynamic Y Damping")]
    [Tooltip("Damping when grounded (Lower = Camera quickly recenters on the player).")]
    public float groundedDampingY = 0.3f;
    [Tooltip("Damping when airborne (Higher = Absorbs small jumps to prevent camera wobble).")]
    public float airborneDampingY = 2.5f;
    [Tooltip("Damping when falling at high speed (0.1 = Instant camera catch-up).")]
    public float fallDampingY = 0.1f;
    [Tooltip("Falling speed threshold required to trigger fallDampingY.")]
    public float fallVelocityThreshold = -10f;

    private Vector3 defaultTargetOffset;

    private void Start()
    {
        if (player == null) player = GetComponentInParent<Player>();

        cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        if (cinemachineCamera != null)
        {
            defaultZoom = cinemachineCamera.Lens.OrthographicSize;

            positionComposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();
            if (positionComposer != null)
            {
                defaultTargetOffset = positionComposer.TargetOffset;
            }
        }
    }

    private void Update()
    {
        if (cinemachineCamera == null || positionComposer == null) return;

        UpdateCombatState();
        UpdateCameraZoom();
        UpdateTargetOffsets();
        UpdateDynamicDamping();
    }

    private void UpdateCombatState()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.transform.position, combatDetectionRadius, enemyLayer);

        if (hits.Length > 0)
        {
            currentState = CameraState.Fighting;
            lastCombatTime = Time.time;

            float minDistance = float.MaxValue;
            Transform nearest = null;

            foreach (var hit in hits)
            {
                float dist = (hit.transform.position - player.transform.position).sqrMagnitude;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = hit.transform;
                }
            }
            closestEnemy = nearest;
        }
        else
        {
            closestEnemy = null;
            if (currentState == CameraState.Fighting && Time.time >= lastCombatTime + combatExitDelay)
            {
                currentState = CameraState.Default;
            }
        }
    }

    private void UpdateCameraZoom()
    {
        float targetZoom = (currentState == CameraState.Fighting)
            ? defaultZoom * combatZoomMultiplier
            : defaultZoom;

        float currentSmoothTime = (currentState == CameraState.Fighting)
            ? zoomOutTime
            : zoomInTime;

        var lens = cinemachineCamera.Lens;
        lens.OrthographicSize = Mathf.SmoothDamp(lens.OrthographicSize, targetZoom, ref zoomVelocity, currentSmoothTime);
        cinemachineCamera.Lens = lens;
    }

    private void UpdateTargetOffsets()
    {
        Vector3 targetOffset = defaultTargetOffset;

        if (currentState == CameraState.Default)
        {
            float moveInputX = inputHandler.RawMovementInput.x;
            float lookDirectionX = 0f;

            if (Mathf.Abs(moveInputX) > 0.01f)
            {
                lookDirectionX = Mathf.Sign(moveInputX);
            }
            else if (player != null)
            {
                lookDirectionX = player.FacingDirection * idleLookAheadRatio;
            }

            targetOffset.x += lookDirectionX * lookAheadDistanceX;
            targetOffset.y += inputHandler.LookAxis * panDistanceY;

            Vector3 currentOffset = positionComposer.TargetOffset;
            currentOffset.x = Mathf.SmoothDamp(currentOffset.x, targetOffset.x, ref offsetVelocity.x, lookAheadSmoothTimeX);
            currentOffset.y = Mathf.SmoothDamp(currentOffset.y, targetOffset.y, ref offsetVelocity.y, panSmoothTimeY);
            positionComposer.TargetOffset = currentOffset;
        }
        else if (currentState == CameraState.Fighting)
        {
            if (closestEnemy != null && player != null)
            {
                Vector3 toEnemy = closestEnemy.position - player.transform.position;

                float weightedOffsetX = Mathf.Clamp(toEnemy.x * enemyWeight, -maxCombatOffset.x, maxCombatOffset.x);
                float weightedOffsetY = Mathf.Clamp(toEnemy.y * enemyWeight, -maxCombatOffset.y, maxCombatOffset.y);

                targetOffset.x += weightedOffsetX;
                targetOffset.y += weightedOffsetY;
            }

            float combatSmoothTime = 1f / Mathf.Max(0.1f, combatOffsetSpeed);
            positionComposer.TargetOffset = Vector3.SmoothDamp(
                positionComposer.TargetOffset,
                targetOffset,
                ref offsetVelocity,
                combatSmoothTime
            );
        }
    }

    private void UpdateDynamicDamping()
    {
        Vector3 currentDamping = positionComposer.Damping;
        float targetDampingY;
        float transitionSpeed;

        if (playerRb.linearVelocity.y < fallVelocityThreshold)
        {
            targetDampingY = fallDampingY;
            transitionSpeed = 15f;
        }
        else if (player != null && !player.CheckIfGrounded())
        {
            targetDampingY = airborneDampingY;
            transitionSpeed = 10f;
        }
        else
        {
            targetDampingY = groundedDampingY;
            transitionSpeed = 5f;
        }

        currentDamping.y = Mathf.MoveTowards(currentDamping.y, targetDampingY, Time.deltaTime * transitionSpeed);
        positionComposer.Damping = currentDamping;
    }

    private void OnDrawGizmosSelected()
    {
        Transform center = (player != null) ? player.transform : transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center.position, combatDetectionRadius);
    }
}