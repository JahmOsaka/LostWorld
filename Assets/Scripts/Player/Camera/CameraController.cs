using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraController : MonoBehaviour
{
    public enum CameraState { Default, Fighting }

    [Header("1. Debug & Gizmos")]
    [Tooltip("Current camera behavior mode (Read-only).")]
    [SerializeField] private CameraState currentState = CameraState.Default;
    [Tooltip("Draw camera gizmos in Scene view even when this object is not selected.")]
    [SerializeField] private bool alwaysDrawGizmos = true;
    [Tooltip("Show the 2D deadzone box, anchor cross, and follow proxy.")]
    [SerializeField] private bool drawDeadzoneGizmos = true;
    [Tooltip("Show the combat detection radius and line to the nearest enemy.")]
    [SerializeField] private bool drawCombatGizmos = true;

    [Header("2. References (Auto-Found if Empty)")]
    [Tooltip("Reference to the Player script (auto-detected if left empty).")]
    [SerializeField] private Player player;
    [Tooltip("Reference to PlayerInputHandler (auto-detected if left empty).")]
    [SerializeField] private PlayerInputHandler inputHandler;
    [Tooltip("Reference to the Player's Rigidbody2D (auto-detected if left empty).")]
    [SerializeField] private Rigidbody2D playerRb;

    [Header("3. Base Framing & Zoom")]
    [Tooltip("Base camera offset from the player. Positive Y places the player slightly below screen center.")]
    [SerializeField] private Vector2 baseFramingOffset = new Vector2(0f, 1.2f);
    [Tooltip("Default camera Z distance during exploration (Perspective mode).")]
    [Range(2f, 30f)][SerializeField] private float defaultCameraDistance = 10f;
    [Tooltip("Distance multiplier applied during combat (1.25 - 1.35 gives a comfortable zoom-out).")]
    [Range(1f, 2f)][SerializeField] private float combatZoomMultiplier = 1.3f;
    [Tooltip("Smooth time (seconds) to zoom out when entering combat.")]
    [Range(0.05f, 3f)][SerializeField] private float zoomOutTime = 0.4f;
    [Tooltip("Smooth time (seconds) to zoom back in after combat ends.")]
    [Range(0.1f, 4f)][SerializeField] private float zoomInTime = 1.5f;

    [Header("4. Vertical Deadzone & Platform Snapping")]
    [Tooltip("How high the player can jump before the camera starts moving upward.")]
    [Range(0.5f, 6f)][SerializeField] private float verticalDeadzoneUp = 3.2f;
    [Tooltip("How far the player can fall before the camera starts moving downward.")]
    [Range(0.2f, 5f)][SerializeField] private float verticalDeadzoneDown = 1.2f;
    [Tooltip("Smooth time (seconds) to recenter the camera when landing on a new elevation.")]
    [Range(0.05f, 1f)][SerializeField] private float platformSnapSmoothTime = 0.3f;
    [Tooltip("General vertical follow smooth time (seconds). Higher values feel softer.")]
    [Range(0.05f, 1f)][SerializeField] private float verticalFollowSmoothTime = 0.25f;
    [Tooltip("Downward speed required to trigger fast fall catch-up and downward look-ahead.")]
    [SerializeField] private float fallVelocityThreshold = -10f;
    [Tooltip("Extra downward offset applied while falling fast so you can see the ground ahead.")]
    [Range(-5f, 0f)][SerializeField] private float fallLookAheadY = -2f;
    [Tooltip("Faster vertical smooth time (seconds) used when falling at high speed.")]
    [Range(0.02f, 0.5f)][SerializeField] private float fallCatchUpSmoothTime = 0.1f;

    [Header("5. Horizontal Deadzone & Look-Ahead")]
    [Tooltip("Total width the player can walk left/right before the camera starts following horizontally.")]
    [Range(0f, 4f)][SerializeField] private float horizontalDeadzone = 1.6f;
    [Tooltip("General horizontal follow smooth time (seconds). Higher values feel less glued to input.")]
    [Range(0.05f, 0.6f)][SerializeField] private float horizontalFollowSmoothTime = 0.18f;
    [Tooltip("How far ahead of the player the camera leads horizontally while running.")]
    [Range(0f, 6f)][SerializeField] private float lookAheadDistanceX = 1.8f;
    [Tooltip("Smooth time (seconds) for horizontal look-ahead transitions.")]
    [Range(0.1f, 2f)][SerializeField] private float lookAheadSmoothTime = 0.65f;
    [Tooltip("Fraction of look-ahead distance kept while standing still (0 = centered, 1 = full lead).")]
    [Range(0f, 1f)][SerializeField] private float idleLookAheadRatio = 0.7f;
    [Tooltip("How long (seconds) you must run in a new direction before look-ahead flips sides.")]
    [Range(0f, 0.8f)][SerializeField] private float turnCommitDelay = 0.22f;

    [Header("6. Vertical Peek (Look Up / Down)")]
    [Tooltip("Allow holding Up/Down while standing still to pan the camera.")]
    [SerializeField] private bool enableVerticalPeek = true;
    [Tooltip("Distance the camera pans upward when looking up.")]
    [Range(0f, 5f)][SerializeField] private float peekDistanceUp = 2.5f;
    [Tooltip("Distance the camera pans downward when looking down.")]
    [Range(0f, 5f)][SerializeField] private float peekDistanceDown = 3f;
    [Tooltip("How long (seconds) Up/Down must be held before the camera starts panning.")]
    [Range(0.1f, 1.5f)][SerializeField] private float peekHoldDelay = 0.35f;
    [Tooltip("Smooth time (seconds) for vertical peek panning.")]
    [Range(0.05f, 1f)][SerializeField] private float peekSmoothTime = 0.25f;

    [Header("7. Combat Framing & Weighted Midpoint")]
    [Tooltip("Enable gentle camera pull toward nearby enemies. Disable to keep only combat zoom.")]
    [SerializeField] private bool enableCombatMidpoint = true;
    [Tooltip("Layer mask used to detect nearby enemies.")]
    [SerializeField] private LayerMask enemyLayer;
    [Tooltip("Radius around the player to detect enemies and enter Fighting state.")]
    [Range(2f, 25f)][SerializeField] private float combatDetectionRadius = 9f;
    [Tooltip("Blend weight toward the nearest enemy (0.1 - 0.2 is gentle and avoids motion sickness).")]
    [Range(0f, 0.5f)][SerializeField] private float enemyWeight = 0.15f;
    [Tooltip("Maximum horizontal and vertical distance the camera can shift toward an enemy.")]
    [SerializeField] private Vector2 maxCombatOffset = new Vector2(1.6f, 0.8f);
    [Tooltip("Smooth time (seconds) when shifting toward or away from the combat midpoint.")]
    [Range(0.1f, 2f)][SerializeField] private float combatOffsetSmoothTime = 0.7f;
    [Tooltip("Time (seconds) after no enemies are in range before returning to Default state.")]
    [Range(0f, 5f)][SerializeField] private float combatExitDelay = 1.5f;

    private CinemachineCamera cinemachineCamera;
    private CinemachinePositionComposer positionComposer;
    private Transform followProxy;

    private ContactFilter2D enemyFilter;
    private readonly List<Collider2D> enemyHits = new List<Collider2D>(16);

    private float anchorX;
    private float anchorY;
    private float anchorYVelocity;
    private float currentLookAheadX;
    private float lookAheadXVelocity;
    private float currentPeekY;
    private float peekYVelocity;
    private float peekHoldTimer;

    private int committedDirectionX = 1;
    private int pendingDirectionX = 1;
    private float turnTimer;

    private Vector2 currentCombatOffset;
    private Vector2 combatOffsetVelocity;
    private float lastCombatTime;
    private Transform closestEnemy;

    private Vector2 proxyVelocity;
    private float zoomVelocity;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        positionComposer = GetComponent<CinemachinePositionComposer>();

        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(enemyLayer);
        enemyFilter.useLayerMask = true;
        enemyFilter.useTriggers = true;
    }

    private void Start()
    {
        ResolvePlayerReferences();

        if (positionComposer != null)
        {
            positionComposer.CameraDistance = defaultCameraDistance;
            positionComposer.Damping = Vector3.zero;
            positionComposer.TargetOffset = Vector3.zero;
        }

        CreateFollowProxy();
        SnapToPlayerImmediate();
    }

    private void OnDestroy()
    {
        if (followProxy != null)
        {
            Destroy(followProxy.gameObject);
        }
    }

    private void LateUpdate()
    {
        if (player == null || cinemachineCamera == null || followProxy == null) return;

        UpdateCombatState();
        UpdateCameraZoom();
        UpdateHorizontalAnchorAndLookAhead();
        UpdateVerticalAnchor();
        UpdateVerticalPeek();
        UpdateCombatOffset();
        UpdateFollowProxyPosition();
    }

    public void SnapToPlayerImmediate()
    {
        if (player == null) return;

        Vector3 pPos = player.transform.position;
        anchorX = pPos.x;
        anchorY = pPos.y;
        committedDirectionX = player.FacingDirection != 0 ? player.FacingDirection : 1;
        pendingDirectionX = committedDirectionX;
        currentLookAheadX = committedDirectionX * lookAheadDistanceX;
        currentPeekY = 0f;
        currentCombatOffset = Vector2.zero;

        anchorYVelocity = 0f;
        lookAheadXVelocity = 0f;
        peekYVelocity = 0f;
        combatOffsetVelocity = Vector2.zero;
        proxyVelocity = Vector2.zero;

        if (followProxy != null)
        {
            followProxy.position = new Vector3(
                anchorX + baseFramingOffset.x + currentLookAheadX,
                anchorY + baseFramingOffset.y,
                pPos.z
            );
        }
    }

    private void ResolvePlayerReferences()
    {
        if (player == null) player = FindAnyObjectByType<Player>();
        if (player == null) return;

        if (inputHandler == null) inputHandler = player.InputHandler != null ? player.InputHandler : player.GetComponent<PlayerInputHandler>();
        if (playerRb == null) playerRb = player.RB != null ? player.RB : player.GetComponent<Rigidbody2D>();
    }

    private void CreateFollowProxy()
    {
        GameObject proxyObj = new GameObject("CameraFollowProxy");
        followProxy = proxyObj.transform;
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = followProxy;
        }
    }

    private void UpdateCombatState()
    {
        enemyFilter.SetLayerMask(enemyLayer);
        int hitCount = Physics2D.OverlapCircle(player.transform.position, combatDetectionRadius, enemyFilter, enemyHits);

        if (hitCount > 0)
        {
            currentState = CameraState.Fighting;
            lastCombatTime = Time.time;

            Vector3 pPos = player.transform.position;
            float minDistSqr = float.MaxValue;
            Transform nearest = null;
            bool currentStillInRange = false;
            float currentDistSqr = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = enemyHits[i];
                if (hit == null) continue;

                float distSqr = (hit.transform.position - pPos).sqrMagnitude;
                if (hit.transform == closestEnemy)
                {
                    currentStillInRange = true;
                    currentDistSqr = distSqr;
                }

                if (distSqr < minDistSqr)
                {
                    minDistSqr = distSqr;
                    nearest = hit.transform;
                }
            }

            if (currentStillInRange && nearest != closestEnemy)
            {
                if (minDistSqr < currentDistSqr * 0.6f)
                {
                    closestEnemy = nearest;
                }
            }
            else
            {
                closestEnemy = nearest;
            }
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
        if (positionComposer == null) return;

        float targetDistance = (currentState == CameraState.Fighting)
            ? defaultCameraDistance * combatZoomMultiplier
            : defaultCameraDistance;

        float smoothTime = (currentState == CameraState.Fighting) ? zoomOutTime : zoomInTime;

        positionComposer.CameraDistance = Mathf.SmoothDamp(
            positionComposer.CameraDistance,
            targetDistance,
            ref zoomVelocity,
            smoothTime
        );
    }

    private void UpdateHorizontalAnchorAndLookAhead()
    {
        float playerX = player.transform.position.x;
        float halfDeadzoneX = horizontalDeadzone * 0.5f;
        int pushDir = 0;

        if (playerX > anchorX + halfDeadzoneX)
        {
            anchorX = playerX - halfDeadzoneX;
            pushDir = 1;
        }
        else if (playerX < anchorX - halfDeadzoneX)
        {
            anchorX = playerX + halfDeadzoneX;
            pushDir = -1;
        }

        if (pushDir != 0)
        {
            if (pushDir != pendingDirectionX)
            {
                pendingDirectionX = pushDir;
                turnTimer = 0f;
            }

            turnTimer += Time.deltaTime;
            if (turnTimer >= turnCommitDelay)
            {
                committedDirectionX = pendingDirectionX;
            }
        }
        else
        {
            pendingDirectionX = committedDirectionX;
            turnTimer = 0f;
        }

        float targetLookAheadX = committedDirectionX * lookAheadDistanceX;

        if (currentState == CameraState.Fighting && enableCombatMidpoint)
        {
            targetLookAheadX *= 0.35f;
        }

        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref lookAheadXVelocity, lookAheadSmoothTime);
    }

    private void UpdateVerticalAnchor()
    {
        float playerY = player.transform.position.y;
        bool isGrounded = player.CheckIfGrounded();

        if (isGrounded)
        {
            anchorY = Mathf.SmoothDamp(anchorY, playerY, ref anchorYVelocity, platformSnapSmoothTime);
        }
        else
        {
            float upperBound = anchorY + verticalDeadzoneUp;
            float lowerBound = anchorY - verticalDeadzoneDown;

            if (playerY > upperBound)
            {
                anchorY = playerY - verticalDeadzoneUp;
                anchorYVelocity = 0f;
            }
            else if (playerY < lowerBound)
            {
                anchorY = playerY + verticalDeadzoneDown;
                anchorYVelocity = 0f;
            }
        }
    }

    private void UpdateVerticalPeek()
    {
        float targetPeekY = 0f;

        if (enableVerticalPeek && currentState == CameraState.Default && inputHandler != null && player.CheckIfGrounded())
        {
            Vector2 move = inputHandler.RawMovementInput;
            bool isStandingStill = Mathf.Abs(move.x) < 0.1f;

            if (isStandingStill && Mathf.Abs(move.y) > 0.5f)
            {
                peekHoldTimer += Time.deltaTime;
                if (peekHoldTimer >= peekHoldDelay)
                {
                    targetPeekY = move.y > 0f ? peekDistanceUp : -peekDistanceDown;
                }
            }
            else
            {
                peekHoldTimer = 0f;
            }
        }
        else
        {
            peekHoldTimer = 0f;
        }

        currentPeekY = Mathf.SmoothDamp(currentPeekY, targetPeekY, ref peekYVelocity, peekSmoothTime);
    }

    private void UpdateCombatOffset()
    {
        Vector2 targetOffset = Vector2.zero;

        if (enableCombatMidpoint && currentState == CameraState.Fighting && closestEnemy != null)
        {
            Vector3 toEnemy = closestEnemy.position - player.transform.position;
            targetOffset.x = Mathf.Clamp(toEnemy.x * enemyWeight, -maxCombatOffset.x, maxCombatOffset.x);
            targetOffset.y = Mathf.Clamp(toEnemy.y * enemyWeight, -maxCombatOffset.y, maxCombatOffset.y);
        }

        currentCombatOffset = Vector2.SmoothDamp(currentCombatOffset, targetOffset, ref combatOffsetVelocity, combatOffsetSmoothTime);
    }

    private void UpdateFollowProxyPosition()
    {
        Vector3 pPos = player.transform.position;
        bool isFallingFast = playerRb != null && playerRb.linearVelocity.y < fallVelocityThreshold;

        float fallOffset = isFallingFast ? fallLookAheadY : 0f;
        float smoothTimeY = isFallingFast ? fallCatchUpSmoothTime : verticalFollowSmoothTime;

        float targetX = anchorX + baseFramingOffset.x + currentLookAheadX + currentCombatOffset.x;
        float targetY = anchorY + baseFramingOffset.y + currentPeekY + fallOffset + currentCombatOffset.y;

        Vector3 currentPos = followProxy.position;
        float nextX = Mathf.SmoothDamp(currentPos.x, targetX, ref proxyVelocity.x, horizontalFollowSmoothTime);
        float nextY = Mathf.SmoothDamp(currentPos.y, targetY, ref proxyVelocity.y, smoothTimeY);

        followProxy.position = new Vector3(nextX, nextY, pPos.z);
    }

    private void OnDrawGizmos()
    {
        if (alwaysDrawGizmos) DrawCameraGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (!alwaysDrawGizmos) DrawCameraGizmos();
    }

    private void DrawCameraGizmos()
    {
        Transform centerTransform = player != null ? player.transform : transform;
        Vector3 pPos = centerTransform.position;
        float refX = Application.isPlaying ? anchorX : pPos.x;
        float refY = Application.isPlaying ? anchorY : pPos.y;

        if (drawDeadzoneGizmos)
        {
            float boxWidth = Mathf.Max(0.2f, horizontalDeadzone);
            float boxHeight = verticalDeadzoneUp + verticalDeadzoneDown;
            Vector3 boxCenter = new Vector3(refX, refY + (verticalDeadzoneUp - verticalDeadzoneDown) * 0.5f, pPos.z);

            Gizmos.color = new Color(0f, 0.9f, 1f, 0.85f);
            Gizmos.DrawWireCube(boxCenter, new Vector3(boxWidth, boxHeight, 0f));

            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(refX - boxWidth * 0.5f, refY, pPos.z), new Vector3(refX + boxWidth * 0.5f, refY, pPos.z));

            if (Application.isPlaying && followProxy != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(followProxy.position, 0.25f);
                Gizmos.DrawLine(pPos, followProxy.position);
            }
        }

        if (drawCombatGizmos)
        {
            Gizmos.color = currentState == CameraState.Fighting ? new Color(1f, 0.4f, 0f, 0.7f) : new Color(1f, 0.92f, 0.016f, 0.45f);
            Gizmos.DrawWireSphere(pPos, combatDetectionRadius);

            if (Application.isPlaying && closestEnemy != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(pPos, closestEnemy.position);
                Gizmos.DrawWireSphere(closestEnemy.position, 0.35f);
            }
        }
    }
}