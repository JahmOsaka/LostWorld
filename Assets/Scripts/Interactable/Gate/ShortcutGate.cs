using System.Collections;
using UnityEngine;

public class ShortcutGate : BaseGate
{
    public enum UnlockSide { Left, Right }

    [Header("One-Way Direction")]
    [Tooltip("Side from which the player is allowed to unlock this gate (Green side).")]
    [SerializeField] private UnlockSide unlockableSide = UnlockSide.Left;
    [SerializeField] private string openGateLabel = "Open Gate";

    [Header("Shortcut Unlock Feel")]
    [SerializeField] private Vector3 openSlideOffset = new Vector3(0f, 3.5f, 0f);
    [SerializeField] private float unlockRattleDuration = 0.14f;
    [SerializeField] private float openSlideDuration = 0.45f;
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private void Reset()
    {
        lockedReactionText = "It's locked from the other side.";
    }

    protected override bool CanUnlock()
    {
        Transform player = GetPlayerTransform();
        if (player == null) return false;

        float referenceX = (solidCollider != null) ? solidCollider.bounds.center.x : transform.position.x;
        return unlockableSide == UnlockSide.Left
            ? player.position.x < referenceX
            : player.position.x > referenceX;
    }

    protected override string GetPromptLabel()
    {
        return openGateLabel;
    }

    protected override void PerformUnlock()
    {
        isUnlocked = true;
        HideInteractPrompt();

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(UnlockRoutine());
    }

    private IEnumerator UnlockRoutine()
    {
        isAnimating = true;
        CacheVisualIfNeeded();

        TriggerCameraShake(unlockShakeDuration * 0.5f, unlockShakeMagnitude * 0.5f);
        float rattleElapsed = 0f;
        while (rattleElapsed < unlockRattleDuration)
        {
            rattleElapsed += Time.deltaTime;
            if (gateVisual != null)
            {
                Vector2 jitter = Random.insideUnitCircle * rattleMagnitude;
                gateVisual.transform.localPosition = visualBaseLocalPos + new Vector3(jitter.x, jitter.y, 0f);
            }
            yield return null;
        }

        if (gateVisual != null)
        {
            gateVisual.transform.localPosition = visualBaseLocalPos;
        }

        if (solidCollider != null) solidCollider.enabled = false;

        TriggerCameraShake(unlockShakeDuration, unlockShakeMagnitude);
        if (unlockVfxPrefab != null)
        {
            GameObject fx = Instantiate(unlockVfxPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1.2f);
        }

        if (gateVisual != null && openSlideDuration > 0f)
        {
            Vector3 startLocal = visualBaseLocalPos;
            Vector3 endLocal = visualBaseLocalPos + openSlideOffset;
            float elapsed = 0f;

            while (elapsed < openSlideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / openSlideDuration);
                gateVisual.transform.localPosition = Vector3.LerpUnclamped(startLocal, endLocal, openCurve.Evaluate(t));
                yield return null;
            }

            gateVisual.SetActive(false);
        }
        else if (gateVisual != null)
        {
            gateVisual.SetActive(false);
        }

        isAnimating = false;
        animCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        Vector3 center = (solidCollider != null) ? solidCollider.bounds.center : transform.position;
        float dir = (unlockableSide == UnlockSide.Left) ? -1f : 1f;

        Gizmos.color = new Color(0.15f, 1f, 0.3f, 0.85f);
        Gizmos.DrawSphere(center + new Vector3(dir * 0.65f, 0f, 0f), 0.16f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.85f);
        Gizmos.DrawSphere(center + new Vector3(-dir * 0.65f, 0f, 0f), 0.16f);
    }
}