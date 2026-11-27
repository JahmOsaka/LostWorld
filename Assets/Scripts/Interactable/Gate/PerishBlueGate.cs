using System.Collections;
using UnityEngine;

public class PerishBlueGate : BaseGate
{
    [Header("Perish Requirement & Labels")]
    [Tooltip("Uncheck temporarily if you want to test absorbing without unlocking Perish first.")]
    [SerializeField] private bool requirePerishSkill = true;
    [SerializeField] private string perishUnlockLabel = "Absorb";
    [SerializeField] private string perishExamineLabel = "Examine";

    [Header("Absorption Feel")]
    [Tooltip("Spectral tint blended onto the gate sprite while being absorbed.")]
    [SerializeField] private Color absorbTintColor = new Color(0.35f, 0.85f, 1f, 1f);
    [SerializeField] private float absorbWindupDuration = 0.16f;
    [SerializeField] private float absorbPullDuration = 0.45f;
    [Tooltip("Pull acceleration curve (starts slow, snaps rapidly into the player).")]
    [SerializeField]
    private AnimationCurve absorbPullCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0.2f),
        new Keyframe(1f, 1f, 2.8f, 0f)
    );
    [Tooltip("Peak stretch multiplier along the suction axis.")]
    [SerializeField] private float absorbStretchAmount = 0.45f;
    [Tooltip("Spin angle in degrees while being sucked in (0 = no spin).")]
    [SerializeField] private float absorbSpinDegrees = 180f;
    [Tooltip("Optional burst VFX spawned on the player when absorption completes.")]
    [SerializeField] private GameObject absorbImpactVfxPrefab;

    private void Reset()
    {
        lockedReactionText = "It's sealed by a strange energy... I can't break through it yet.";
    }

    protected override bool CanUnlock()
    {
        if (!requirePerishSkill) return true;

        GetPlayerTransform();
        return cachedPlayer != null
            && cachedPlayer.SkillController != null
            && cachedPlayer.SkillController.hasPerish;
    }

    protected override string GetPromptLabel()
    {
        return CanUnlock() ? perishUnlockLabel : perishExamineLabel;
    }

    protected override void PerformUnlock()
    {
        AbsorbByPerish(GetPlayerTransform());
    }

    public bool AbsorbByPerish(Transform absorber)
    {
        if (isUnlocked || isAnimating) return false;

        isUnlocked = true;
        HideInteractPrompt();
        GetPlayerTransform();
        cachedPlayer?.SkillController?.PlayPerishCastVfx();

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(PerishAbsorbRoutine(absorber));
        return true;
    }

    private IEnumerator PerishAbsorbRoutine(Transform absorber)
    {
        isAnimating = true;
        CacheVisualIfNeeded();

        TriggerCameraShake(absorbWindupDuration, unlockShakeMagnitude * 0.6f);
        float windupElapsed = 0f;
        while (windupElapsed < absorbWindupDuration)
        {
            windupElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(windupElapsed / Mathf.Max(0.01f, absorbWindupDuration));

            if (gateVisual != null)
            {
                Vector2 jitter = Random.insideUnitCircle * (rattleMagnitude * (0.6f + t));
                gateVisual.transform.localPosition = visualBaseLocalPos + new Vector3(jitter.x, jitter.y, 0f);
                ApplyVisualTint(absorbTintColor, t, 1f);
            }
            yield return null;
        }

        if (solidCollider != null) solidCollider.enabled = false;

        if (unlockVfxPrefab != null)
        {
            GameObject fx = Instantiate(unlockVfxPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1.5f);
        }

        if (gateVisual != null && absorbPullDuration > 0f)
        {
            Vector3 startWorldPos = gateVisual.transform.position;
            Vector3 fallbackTarget = absorber != null ? absorber.position : transform.position;
            float spinSign = (absorber != null && absorber.position.x < startWorldPos.x) ? 1f : -1f;

            float elapsed = 0f;
            while (elapsed < absorbPullDuration)
            {
                elapsed += Time.deltaTime;
                float rawT = Mathf.Clamp01(elapsed / absorbPullDuration);
                float pullT = absorbPullCurve.Evaluate(rawT);

                Vector3 targetPos = absorber != null ? absorber.position : fallbackTarget;
                gateVisual.transform.position = Vector3.LerpUnclamped(startWorldPos, targetPos, pullT);

                float shrink = 1f - pullT;
                float stretchPulse = Mathf.Sin(rawT * Mathf.PI) * absorbStretchAmount;
                float scaleX = visualBaseLocalScale.x * shrink * (1f + stretchPulse);
                float scaleY = visualBaseLocalScale.y * shrink * (1f - stretchPulse * 0.45f);
                gateVisual.transform.localScale = new Vector3(Mathf.Max(0f, scaleX), Mathf.Max(0f, scaleY), visualBaseLocalScale.z);

                if (Mathf.Abs(absorbSpinDegrees) > 0.01f)
                {
                    gateVisual.transform.localRotation = visualBaseLocalRot * Quaternion.Euler(0f, 0f, absorbSpinDegrees * pullT * spinSign);
                }

                float alpha = Mathf.Clamp01(1f - Mathf.Max(0f, (rawT - 0.35f) / 0.65f));
                ApplyVisualTint(absorbTintColor, 1f, alpha);

                yield return null;
            }

            gateVisual.SetActive(false);
        }
        else if (gateVisual != null)
        {
            gateVisual.SetActive(false);
        }

        TriggerCameraShake(unlockShakeDuration, unlockShakeMagnitude * 1.15f);
        if (absorbImpactVfxPrefab != null && absorber != null)
        {
            GameObject impactFx = Instantiate(absorbImpactVfxPrefab, absorber.position, Quaternion.identity);
            Destroy(impactFx, 1.2f);
        }

        isAnimating = false;
        animCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        Vector3 center = (solidCollider != null) ? solidCollider.bounds.center : transform.position;
        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.85f);
        Gizmos.DrawWireSphere(center, 0.35f);
    }
}