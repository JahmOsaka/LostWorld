using System.Collections;
using UnityEngine;

public class ShortcutGate : MonoBehaviour, IInteractable
{
    public enum GateType { OneWayShortcut, PerishBlueGate }
    public enum UnlockSide { Left, Right }

    [Header("Gate Mode")]
    [SerializeField] private GateType gateType = GateType.OneWayShortcut;

    [Header("Save System")]
    public string gateID;
    public bool isUnlocked = false;

    [Header("One-Way Direction (For OneWayShortcut)")]
    [Tooltip("Side from which the player is allowed to unlock this gate (Green side).")]
    [SerializeField] private UnlockSide unlockableSide = UnlockSide.Left;
    [Tooltip("If true, approaching the Red side still shows a locked prompt and rattles the gate on interact.")]
    [SerializeField] private bool showLockedFeedbackOnWrongSide = true;

    [Header("References")]
    public SpriteHighlight highlightEffect;
    public GameObject gateVisual;
    public Collider2D solidCollider;

    [Header("Interact HUD Prompt")]
    [Tooltip("Optional anchor point for the HUD prompt (defaults to gate transform).")]
    [SerializeField] private Transform promptAnchor;
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private string promptKey = "F";
    [SerializeField] private string unlockLabel = "Unlock Shortcut";
    [SerializeField] private string lockedLabel = "Locked from other side";

    [Header("Perish BlueGate HUD & Absorption Feel")]
    [SerializeField] private string perishUnlockLabel = "Absorb";
    [SerializeField] private string perishMissingLabel = "Blocked";
    [Tooltip("Uncheck temporarily if you want to test absorbing without unlocking Perish first.")]
    [SerializeField] private bool requirePerishSkill = true;
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

    [Header("Shortcut Unlock Feel (For OneWayShortcut)")]
    [SerializeField] private Vector3 openSlideOffset = new Vector3(0f, 3.5f, 0f);
    [SerializeField] private float unlockRattleDuration = 0.14f;
    [SerializeField] private float openSlideDuration = 0.45f;
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float rattleMagnitude = 0.06f;
    [SerializeField] private float lockedRattleDuration = 0.18f;
    [SerializeField] private float unlockShakeDuration = 0.14f;
    [SerializeField] private float unlockShakeMagnitude = 0.1f;
    [SerializeField] private GameObject unlockVfxPrefab;

    [Header("Mushroom Wall Animation")]
    [SerializeField] private Animator wallAnimator;

    private Transform playerTransform;
    private Player cachedPlayer;
    private Vector3 visualBaseLocalPos;
    private Vector3 visualBaseLocalScale = Vector3.one;
    private Quaternion visualBaseLocalRot = Quaternion.identity;
    private SpriteRenderer[] visualRenderers;
    private Color[] originalColors;
    private bool hasCachedVisual;
    private bool isPromptShowing;
    private bool isAnimating;
    private Coroutine animCoroutine;

    private void Awake()
    {
        CacheVisualIfNeeded();
    }

    private void OnDisable()
    {
        InteractPromptUI.Instance?.Hide(this);
        isPromptShowing = false;
    }

    private void Update()
    {
        if (isPromptShowing && !isUnlocked && !isAnimating)
        {
            RefreshPromptDisplay();
        }
    }

    public void ShowInteractPrompt()
    {
        if (isUnlocked || isAnimating) return;

        isPromptShowing = true;
        RefreshPromptDisplay();
    }

    public void HideInteractPrompt()
    {
        isPromptShowing = false;
        InteractPromptUI.Instance?.Hide(this);
        if (highlightEffect != null) highlightEffect.TurnOffHighlight();
    }

    public void Interact()
    {
        if (isUnlocked || isAnimating) return;

        if (gateType == GateType.PerishBlueGate)
        {
            if (HasPerishSkill())
            {
                AbsorbByPerish(GetPlayerTransform());
            }
            else if (showLockedFeedbackOnWrongSide)
            {
                if (animCoroutine != null) StopCoroutine(animCoroutine);
                animCoroutine = StartCoroutine(LockedRattleRoutine());
            }
            return;
        }

        if (IsPlayerOnUnlockSide())
        {
            isUnlocked = true;
            HideInteractPrompt();

            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(UnlockRoutine());
        }
        else if (showLockedFeedbackOnWrongSide)
        {
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(LockedRattleRoutine());
        }
    }

    public bool AbsorbByPerish(Transform absorber)
    {
        if (gateType != GateType.PerishBlueGate || isUnlocked || isAnimating) return false;

        if (wallAnimator != null)
        {
            wallAnimator.Play("WallDown");
        }
        else if (gateVisual != null)
        {
            gateVisual.SetActive(false);
        }

        isUnlocked = true;
        HideInteractPrompt();
        cachedPlayer?.SkillController?.PlayPerishCastVfx();

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(PerishAbsorbRoutine(absorber));
        return true;
    }

    public void UnlockImmediate()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        isAnimating = false;
        isUnlocked = true;
        HideInteractPrompt();

        if (gateVisual != null) gateVisual.SetActive(false);
        if (solidCollider != null) solidCollider.enabled = false;
    }

    public void ResetToLocked()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        isAnimating = false;
        isUnlocked = false;
        HideInteractPrompt();

        CacheVisualIfNeeded();

        if (gateVisual != null)
        {
            gateVisual.transform.localPosition = visualBaseLocalPos;
            gateVisual.transform.localScale = visualBaseLocalScale;
            gateVisual.transform.localRotation = visualBaseLocalRot;
            RestoreVisualColors();
            gateVisual.SetActive(true);
        }

        if (solidCollider != null) solidCollider.enabled = true;
    }

    private Transform GetPlayerTransform()
    {
        if (cachedPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                cachedPlayer = playerObj.GetComponentInParent<Player>();
                playerTransform = cachedPlayer != null ? cachedPlayer.transform : playerObj.transform;
            }
        }
        return playerTransform;
    }

    private bool HasPerishSkill()
    {
        if (!requirePerishSkill) return true;

        GetPlayerTransform();
        return cachedPlayer != null
            && cachedPlayer.SkillController != null
            && cachedPlayer.SkillController.hasPerish;
    }

    private void RefreshPromptDisplay()
    {
        Transform anchor = promptAnchor != null ? promptAnchor : transform;

        if (gateType == GateType.PerishBlueGate)
        {
            if (HasPerishSkill())
            {
                InteractPromptUI.Instance?.Show(this, anchor, perishUnlockLabel, promptKey, promptOffset);
                if (highlightEffect != null) highlightEffect.TurnOnHighlight();
            }
            else if (showLockedFeedbackOnWrongSide)
            {
                InteractPromptUI.Instance?.Show(this, anchor, perishMissingLabel, promptKey, promptOffset);
                if (highlightEffect != null) highlightEffect.TurnOffHighlight();
            }
            else
            {
                InteractPromptUI.Instance?.Hide(this);
                if (highlightEffect != null) highlightEffect.TurnOffHighlight();
            }
            return;
        }

        bool onGreenSide = IsPlayerOnUnlockSide();
        if (onGreenSide)
        {
            InteractPromptUI.Instance?.Show(this, anchor, unlockLabel, promptKey, promptOffset);
            if (highlightEffect != null) highlightEffect.TurnOnHighlight();
        }
        else if (showLockedFeedbackOnWrongSide)
        {
            InteractPromptUI.Instance?.Show(this, anchor, lockedLabel, promptKey, promptOffset);
            if (highlightEffect != null) highlightEffect.TurnOffHighlight();
        }
        else
        {
            InteractPromptUI.Instance?.Hide(this);
            if (highlightEffect != null) highlightEffect.TurnOffHighlight();
        }
    }

    private bool IsPlayerOnUnlockSide()
    {
        Transform player = GetPlayerTransform();
        if (player == null) return false;

        float referenceX = (solidCollider != null) ? solidCollider.bounds.center.x : transform.position.x;
        return unlockableSide == UnlockSide.Left
            ? player.position.x < referenceX
            : player.position.x > referenceX;
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
                ApplyVisualTint(t, 1f);
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
                ApplyVisualTint(1f, alpha);

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

    private IEnumerator LockedRattleRoutine()
    {
        isAnimating = true;
        CacheVisualIfNeeded();

        TriggerCameraShake(0.08f, 0.04f);

        float elapsed = 0f;
        while (elapsed < lockedRattleDuration)
        {
            elapsed += Time.deltaTime;
            if (gateVisual != null)
            {
                float shakeX = Mathf.Sin(elapsed * 65f) * rattleMagnitude * (1f - elapsed / lockedRattleDuration);
                gateVisual.transform.localPosition = visualBaseLocalPos + new Vector3(shakeX, 0f, 0f);
            }
            yield return null;
        }

        if (gateVisual != null)
        {
            gateVisual.transform.localPosition = visualBaseLocalPos;
        }

        isAnimating = false;
        animCoroutine = null;
    }

    private void CacheVisualIfNeeded()
    {
        if (gateVisual == null)
        {
            Transform visualChild = transform.Find("Visual");
            if (visualChild != null) gateVisual = visualChild.gameObject;
        }

        if (gateVisual != null && !hasCachedVisual)
        {
            visualBaseLocalPos = gateVisual.transform.localPosition;
            visualBaseLocalScale = gateVisual.transform.localScale;
            visualBaseLocalRot = gateVisual.transform.localRotation;

            visualRenderers = gateVisual.GetComponentsInChildren<SpriteRenderer>(true);
            originalColors = new Color[visualRenderers.Length];
            for (int i = 0; i < visualRenderers.Length; i++)
            {
                originalColors[i] = visualRenderers[i].color;
            }

            hasCachedVisual = true;
        }
    }

    private void ApplyVisualTint(float tintAmount, float alphaMultiplier)
    {
        if (visualRenderers == null) return;

        for (int i = 0; i < visualRenderers.Length; i++)
        {
            if (visualRenderers[i] == null) continue;
            Color c = Color.Lerp(originalColors[i], absorbTintColor, tintAmount);
            c.a = originalColors[i].a * alphaMultiplier;
            visualRenderers[i].color = c;
        }
    }

    private void RestoreVisualColors()
    {
        if (visualRenderers == null) return;

        for (int i = 0; i < visualRenderers.Length; i++)
        {
            if (visualRenderers[i] != null)
            {
                visualRenderers[i].color = originalColors[i];
            }
        }
    }

    private void TriggerCameraShake(float duration, float magnitude)
    {
        if (GameFeelManager.Instance != null && Camera.main != null && duration > 0f)
        {
            GameFeelManager.Instance.TriggerShake(duration, magnitude, Camera.main.transform);
        }
    }

    [ContextMenu("Generate Gate ID")]
    private void GenerateGateID()
    {
        string sceneName = gameObject.scene.name;
        string shortGuid = System.Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        gateID = $"{sceneName}_Gate_{shortGuid}";
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void OnDrawGizmos()
    {
        Vector3 center = (solidCollider != null) ? solidCollider.bounds.center : transform.position;

        if (gateType == GateType.PerishBlueGate)
        {
            Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.85f);
            Gizmos.DrawWireSphere(center, 0.35f);
            return;
        }

        float dir = (unlockableSide == UnlockSide.Left) ? -1f : 1f;
        Gizmos.color = new Color(0.15f, 1f, 0.3f, 0.85f);
        Gizmos.DrawSphere(center + new Vector3(dir * 0.65f, 0f, 0f), 0.16f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.85f);
        Gizmos.DrawSphere(center + new Vector3(-dir * 0.65f, 0f, 0f), 0.16f);
    }
}