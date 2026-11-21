using System.Collections;
using UnityEngine;

public class ShortcutGate : MonoBehaviour, IInteractable
{
    public enum UnlockSide { Left, Right }

    [Header("Save System")]
    public string gateID;
    public bool isUnlocked = false;

    [Header("One-Way Direction")]
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
    [SerializeField] private string unlockLabel = "Unlock shortcut";
    [SerializeField] private string lockedLabel = "Lock from the other side";

    [Header("Game Feel (Juice)")]
    [Tooltip("Offset applied to gateVisual as it slides open (e.g., Y = 3.5 to lift upward).")]
    [SerializeField] private Vector3 openSlideOffset = new Vector3(0f, 3.5f, 0f);
    [SerializeField] private float unlockRattleDuration = 0.14f;
    [SerializeField] private float openSlideDuration = 0.45f;
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float rattleMagnitude = 0.06f;
    [SerializeField] private float lockedRattleDuration = 0.18f;
    [SerializeField] private float unlockShakeDuration = 0.14f;
    [SerializeField] private float unlockShakeMagnitude = 0.1f;
    [SerializeField] private GameObject unlockVfxPrefab;

    private Transform playerTransform;
    private Vector3 visualBaseLocalPos;
    private bool hasCachedVisualPos;
    private bool isPromptShowing;
    private bool isAnimating;
    private Coroutine animCoroutine;

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
            gateVisual.SetActive(true);
        }

        if (solidCollider != null) solidCollider.enabled = true;
    }

    private void RefreshPromptDisplay()
    {
        bool onGreenSide = IsPlayerOnUnlockSide();

        if (onGreenSide)
        {
            Transform anchor = promptAnchor != null ? promptAnchor : transform;
            InteractPromptUI.Instance?.Show(this, anchor, unlockLabel, promptKey, promptOffset);
            if (highlightEffect != null) highlightEffect.TurnOnHighlight();
        }
        else if (showLockedFeedbackOnWrongSide)
        {
            Transform anchor = promptAnchor != null ? promptAnchor : transform;
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
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        if (playerTransform == null) return false;

        float referenceX = (solidCollider != null) ? solidCollider.bounds.center.x : transform.position.x;
        return unlockableSide == UnlockSide.Left
            ? playerTransform.position.x < referenceX
            : playerTransform.position.x > referenceX;
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

        if (gateVisual != null && !hasCachedVisualPos)
        {
            visualBaseLocalPos = gateVisual.transform.localPosition;
            hasCachedVisualPos = true;
        }
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
        Vector3 center = (solidCollider != null) ? solidCollider.bounds.center : transform.position;
        float dir = (unlockableSide == UnlockSide.Left) ? -1f : 1f;

        Gizmos.color = new Color(0.15f, 1f, 0.3f, 0.85f);
        Gizmos.DrawSphere(center + new Vector3(dir * 0.65f, 0f, 0f), 0.16f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.85f);
        Gizmos.DrawSphere(center + new Vector3(-dir * 0.65f, 0f, 0f), 0.16f);
    }
}