using System.Collections;
using UnityEngine;

public class LeverGate : BaseGate, IActivatable
{
    public enum SlideDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [Header("Lever Gate Labels")]
    [SerializeField] private string examineLabel = "Examine";

    [Header("Slide Direction & Distance")]
    [Tooltip("Choose Up/Down for vertical gates, or Left/Right for horizontal gates.")]
    [SerializeField] private SlideDirection slideDirection = SlideDirection.Up;
    [Tooltip("Distance in local units the gate visual slides when opening.")]
    [SerializeField] private float slideDistance = 3.5f;

    [Header("Slide Animation")]
    [Tooltip("Short rattle duration before sliding open.")]
    [SerializeField] private float rattleBeforeOpenDuration = 0.12f;
    [Tooltip("Duration in seconds to slide open or closed.")]
    [SerializeField] private float slideDuration = 0.45f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("If true, hides gateVisual once fully open (automatically re-enabled when closing).")]
    [SerializeField] private bool hideVisualWhenOpen = false;

    public bool IsActivated => isUnlocked;

    private void Reset()
    {
        lockedReactionText = "It looks like it's controlled by a lever somewhere.";
    }

    protected override bool CanUnlock()
    {
        return false;
    }

    protected override string GetPromptLabel()
    {
        return examineLabel;
    }

    protected override void PerformUnlock()
    {
        Activate();
    }

    public void Activate()
    {
        if (isUnlocked) return;
        SetGateOpen(true);
    }

    public void Deactivate()
    {
        if (!isUnlocked) return;
        SetGateOpen(false);
    }

    public override void UnlockImmediate()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        isAnimating = false;
        isUnlocked = true;
        HideInteractPrompt();
        CacheVisualIfNeeded();

        if (solidCollider != null) solidCollider.enabled = false;

        if (gateVisual != null)
        {
            gateVisual.transform.localPosition = visualBaseLocalPos + GetSlideOffset();
            if (hideVisualWhenOpen) gateVisual.SetActive(false);
        }
    }

    private Vector3 GetSlideOffset()
    {
        return slideDirection switch
        {
            SlideDirection.Up => Vector3.up * slideDistance,
            SlideDirection.Down => Vector3.down * slideDistance,
            SlideDirection.Left => Vector3.left * slideDistance,
            SlideDirection.Right => Vector3.right * slideDistance,
            _ => Vector3.up * slideDistance
        };
    }

    private void SetGateOpen(bool open)
    {
        isUnlocked = open;
        if (open) HideInteractPrompt();

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(SlideGateRoutine(open));
    }

    private IEnumerator SlideGateRoutine(bool open)
    {
        isAnimating = true;
        CacheVisualIfNeeded();

        if (open)
        {
            if (rattleBeforeOpenDuration > 0f)
            {
                TriggerCameraShake(unlockShakeDuration * 0.5f, unlockShakeMagnitude * 0.5f);
                float rattleElapsed = 0f;
                Vector3 basePos = gateVisual != null ? gateVisual.transform.localPosition : visualBaseLocalPos;

                while (rattleElapsed < rattleBeforeOpenDuration)
                {
                    rattleElapsed += Time.deltaTime;
                    if (gateVisual != null)
                    {
                        Vector2 jitter = Random.insideUnitCircle * rattleMagnitude;
                        gateVisual.transform.localPosition = basePos + new Vector3(jitter.x, jitter.y, 0f);
                    }
                    yield return null;
                }

                if (gateVisual != null) gateVisual.transform.localPosition = basePos;
            }

            if (solidCollider != null) solidCollider.enabled = false;
            TriggerCameraShake(unlockShakeDuration, unlockShakeMagnitude);

            if (unlockVfxPrefab != null)
            {
                GameObject fx = Instantiate(unlockVfxPrefab, transform.position, Quaternion.identity);
                Destroy(fx, 1.2f);
            }
        }
        else
        {
            if (gateVisual != null) gateVisual.SetActive(true);
            if (solidCollider != null) solidCollider.enabled = true;
        }

        if (gateVisual != null && slideDuration > 0f)
        {
            Vector3 startLocal = gateVisual.transform.localPosition;
            Vector3 endLocal = open ? (visualBaseLocalPos + GetSlideOffset()) : visualBaseLocalPos;

            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                gateVisual.transform.localPosition = Vector3.LerpUnclamped(startLocal, endLocal, slideCurve.Evaluate(t));
                yield return null;
            }

            gateVisual.transform.localPosition = endLocal;
            if (open && hideVisualWhenOpen) gateVisual.SetActive(false);
        }
        else if (gateVisual != null)
        {
            gateVisual.transform.localPosition = open ? (visualBaseLocalPos + GetSlideOffset()) : visualBaseLocalPos;
            if (open && hideVisualWhenOpen) gateVisual.SetActive(false);
        }

        if (!open)
        {
            TriggerCameraShake(unlockShakeDuration * 0.7f, unlockShakeMagnitude * 0.7f);
        }

        isAnimating = false;
        animCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Transform visualTransform = gateVisual != null ? gateVisual.transform : transform;
        Vector3 start = visualTransform.position;
        Vector3 worldOffset = transform.TransformVector(GetSlideOffset());
        Vector3 end = start + worldOffset;

        Gizmos.color = new Color(1f, 0.35f, 0.8f, 0.9f);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(end, new Vector3(0.4f, 0.4f, 0.1f));
    }
}