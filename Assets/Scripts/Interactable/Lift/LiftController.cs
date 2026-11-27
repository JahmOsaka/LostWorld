using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class LiftController : MonoBehaviour, IActivatable
{
    [Header("Waypoints")]
    [Tooltip("Bottom or starting floor waypoint.")]
    [SerializeField] private Transform pointA;
    [Tooltip("Top or destination floor waypoint.")]
    [SerializeField] private Transform pointB;
    [Tooltip("Base travel speed in units per second.")]
    [SerializeField] private float speed = 4f;
    [Tooltip("Easing curve controlling acceleration and deceleration (0 to 1).")]
    [SerializeField] private AnimationCurve motionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Rider Detection (Anti-Jitter)")]
    [Tooltip("Layer mask of the Player so the lift can carry their Rigidbody2D cleanly.")]
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Height of the sensor box above the lift surface to keep downward riding smooth.")]
    [SerializeField] private float riderCheckHeight = 0.18f;
    [Tooltip("Width ratio of the sensor box relative to the lift collider (slightly narrower to ignore side bumps).")]
    [Range(0.5f, 1f)][SerializeField] private float riderCheckWidthRatio = 0.92f;

    [Header("Interact & Prompt")]
    [SerializeField] private InputActionReference interactAction;
    [Tooltip("Optional anchor point above the lift (defaults to lift transform if empty).")]
    [SerializeField] private Transform promptAnchor;
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private string promptKey = "F";
    [SerializeField] private string labelGoUp = "Up";
    [SerializeField] private string labelGoDown = "Down";

    [Header("Game Feel (Juice)")]
    [Tooltip("Child transform holding the lift sprite (used for vibration and docking dip without affecting physics).")]
    [SerializeField] private Transform visualTransform;
    [Tooltip("Delay in seconds before moving while the lift vibrates to signal startup.")]
    [SerializeField] private float startupDelay = 0.18f;
    [Tooltip("Horizontal/vertical vibration amplitude during startup.")]
    [SerializeField] private float startupVibration = 0.04f;
    [Tooltip("Vertical dip offset applied to the visual on arrival.")]
    [SerializeField] private float arrivalDipAmount = 0.08f;
    [Tooltip("Duration in seconds for the arrival dip to recover.")]
    [SerializeField] private float arrivalDipDuration = 0.16f;
    [Tooltip("Subtle camera shake duration on startup and arrival.")]
    [SerializeField] private float shakeDuration = 0.1f;
    [Tooltip("Subtle camera shake magnitude on arrival.")]
    [SerializeField] private float arrivalShakeMagnitude = 0.08f;
    [Tooltip("Optional particle prefab spawned at the lift position when docking.")]
    [SerializeField] private GameObject arrivalDustPrefab;

    [Header("Gear Animation")]
    [SerializeField] private Animator gear1Animator;
    [SerializeField] private Animator gear2Animator;

    [Header("Power / Lever Unlock")]
    [Tooltip("If false, the lift is locked until activated by a LeverInteractable. Defaults to true so existing lifts still work.")]
    [SerializeField] private bool startPowered = true;
    [Tooltip("If true, stepping on an unpowered lift shows an inspect prompt and plays a reaction line.")]
    [SerializeField] private bool showLockedFeedback = true;
    [SerializeField] private string lockedPromptLabel = "Examine";
    [TextArea(2, 3)]
    [SerializeField] private string lockedReactionText = "It's not powered yet...";
    [SerializeField] private float reactionDuration = 2.5f;
    [Tooltip("(Optional) Overrides the player's default portrait for this reaction.")]
    [SerializeField] private Sprite reactionOverridePortrait;

    public enum LiftState { AtA, Starting, Moving, AtB }
    public LiftState CurrentState { get; private set; } = LiftState.AtA;
    public bool IsMoving => CurrentState == LiftState.Starting || CurrentState == LiftState.Moving;
    public bool IsActivated { get; private set; }

    private Rigidbody2D rb;
    private Collider2D liftCollider;
    private Rigidbody2D riderRb;
    private bool isPlayerStandingOnTop;

    private Vector3 visualBaseLocalPos;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        liftCollider = GetComponent<Collider2D>();

        SetGearsSpinning(false);
        IsActivated = startPowered;

        if (visualTransform != null)
        {
            visualBaseLocalPos = visualTransform.localPosition;
        }
    }

    private void OnDisable()
    {
        InteractPromptUI.Instance?.Unregister(this);
        SetGearsSpinning(false);
    }

    private void Start()
    {
        if (pointA != null)
        {
            rb.position = pointA.position;
            transform.position = pointA.position;
        }
        CurrentState = LiftState.AtA;
    }

    private void FixedUpdate()
    {
        DetectRider();
    }

    private void Update()
    {
        if (InteractPromptUI.Instance == null) return;

        bool canInteract = isPlayerStandingOnTop && !IsMoving && (IsActivated || showLockedFeedback);
        if (canInteract)
        {
            string label = IsActivated ? GetCurrentActionLabel() : lockedPromptLabel;
            InteractPromptUI.Instance.Register(this, label, HandlePromptInteract, null, promptKey);
        }
        else
        {
            InteractPromptUI.Instance.Unregister(this);
        }
    }

    private string GetCurrentActionLabel()
    {
        if (pointA == null || pointB == null) return labelGoUp;
        bool targetIsHigher = (CurrentState == LiftState.AtA)
            ? (pointB.position.y >= pointA.position.y)
            : (pointA.position.y >= pointB.position.y);
        return targetIsHigher ? labelGoUp : labelGoDown;
    }

    private void DetectRider()
    {
        if (liftCollider == null) return;

        Bounds b = liftCollider.bounds;
        Vector2 boxCenter = new Vector2(b.center.x, b.max.y + riderCheckHeight * 0.5f);
        Vector2 boxSize = new Vector2(b.size.x * riderCheckWidthRatio, riderCheckHeight);

        Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, playerLayer);
        if (hit != null && hit.attachedRigidbody != null)
        {
            if (hit.attachedRigidbody.linearVelocity.y <= 0.5f)
            {
                riderRb = hit.attachedRigidbody;
                isPlayerStandingOnTop = true;
                return;
            }
        }

        riderRb = null;
        isPlayerStandingOnTop = false;
    }

    private void HandlePromptInteract()
    {
        if (!isPlayerStandingOnTop || IsMoving) return;

        if (IsActivated)
        {
            ToggleLift();
        }
        else if (showLockedFeedback && !string.IsNullOrEmpty(lockedReactionText))
        {
            DialogueUI.Instance?.ShowPlayerReaction(lockedReactionText, reactionDuration, reactionOverridePortrait);
            TriggerCameraShake(0.08f, 0.04f);
        }
    }

    public void Activate()
    {
        IsActivated = true;
    }

    public void Deactivate()
    {
        IsActivated = false;
    }

    public void ToggleLift()
    {
        if (!IsActivated || IsMoving || pointA == null || pointB == null) return;

        bool goToB = (CurrentState == LiftState.AtA);
        StartTravel(goToB);
    }

    public void CallToPointA()
    {
        if (!IsActivated || IsMoving || CurrentState == LiftState.AtA || pointA == null) return;
        StartTravel(false);
    }

    public void CallToPointB()
    {
        if (!IsActivated || IsMoving || CurrentState == LiftState.AtB || pointB == null) return;
        StartTravel(true);
    }

    private void StartTravel(bool toPointB)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(TravelRoutine(toPointB));
    }

    private IEnumerator TravelRoutine(bool toPointB)
    {
        CurrentState = LiftState.Starting;

        SetGearsSpinning(true);

        TriggerCameraShake(shakeDuration * 0.6f, arrivalShakeMagnitude * 0.45f);
        float startElapsed = 0f;
        while (startElapsed < startupDelay)
        {
            startElapsed += Time.deltaTime;
            if (visualTransform != null)
            {
                Vector2 jitter = Random.insideUnitCircle * startupVibration;
                visualTransform.localPosition = visualBaseLocalPos + new Vector3(jitter.x, jitter.y, 0f);
            }
            yield return null;
        }

        if (visualTransform != null)
        {
            visualTransform.localPosition = visualBaseLocalPos;
        }

        CurrentState = LiftState.Moving;
        Vector2 startPos = rb.position;
        Vector2 targetPos = toPointB ? (Vector2)pointB.position : (Vector2)pointA.position;
        float distance = Vector2.Distance(startPos, targetPos);
        float duration = Mathf.Max(0.05f, distance / Mathf.Max(0.1f, speed));

        Vector2 previousPos = rb.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return new WaitForFixedUpdate();
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = motionCurve.Evaluate(t);

            Vector2 nextPos = Vector2.LerpUnclamped(startPos, targetPos, curvedT);
            Vector2 delta = nextPos - previousPos;

            rb.MovePosition(nextPos);
            CarryRider(delta);

            previousPos = nextPos;
        }

        Vector2 finalDelta = targetPos - previousPos;
        rb.MovePosition(targetPos);
        CarryRider(finalDelta);

        CurrentState = toPointB ? LiftState.AtB : LiftState.AtA;
        SetGearsSpinning(false);

        TriggerCameraShake(shakeDuration, arrivalShakeMagnitude);

        if (arrivalDustPrefab != null)
        {
            GameObject dust = Instantiate(arrivalDustPrefab, transform.position, Quaternion.identity);
            Destroy(dust, 1f);
        }

        if (visualTransform != null && arrivalDipDuration > 0f)
        {
            float dipSign = toPointB ? 1f : -1f;
            float dipElapsed = 0f;
            while (dipElapsed < arrivalDipDuration)
            {
                dipElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(dipElapsed / arrivalDipDuration);
                float offset = Mathf.Sin(t * Mathf.PI) * arrivalDipAmount * dipSign;
                visualTransform.localPosition = visualBaseLocalPos + new Vector3(0f, offset, 0f);
                yield return null;
            }
            visualTransform.localPosition = visualBaseLocalPos;
        }

        moveCoroutine = null;
    }

    private void CarryRider(Vector2 delta)
    {
        if (riderRb == null || !isPlayerStandingOnTop) return;

        if (riderRb.linearVelocity.y <= 0.5f)
        {
            riderRb.position += delta;
        }
    }

    private void SetGearsSpinning(bool spinning)
    {
        float animSpeed = spinning ? 1f : 0f;
        if (gear1Animator != null) gear1Animator.speed = animSpeed;
        if (gear2Animator != null) gear2Animator.speed = animSpeed * 0.5f;
    }

    private void TriggerCameraShake(float duration, float magnitude)
    {
        if (GameFeelManager.Instance != null && Camera.main != null && duration > 0f)
        {
            GameFeelManager.Instance.TriggerShake(duration, magnitude, Camera.main.transform);
        }
    }

    private void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.7f);
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.2f);
            Gizmos.DrawWireSphere(pointB.position, 0.2f);
        }

        Collider2D col = liftCollider != null ? liftCollider : GetComponent<Collider2D>();
        if (col != null)
        {
            Bounds b = col.bounds;
            Vector3 boxCenter = new Vector3(b.center.x, b.max.y + riderCheckHeight * 0.5f, 0f);
            Vector3 boxSize = new Vector3(b.size.x * riderCheckWidthRatio, riderCheckHeight, 0.1f);
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.5f);
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
    }
}