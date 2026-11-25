using System.Collections;
using UnityEngine;

public abstract class BaseGate : MonoBehaviour, IInteractable
{
    [Header("Save System")]
    public string gateID;
    public bool isUnlocked = false;

    [Header("References")]
    public SpriteHighlight highlightEffect;
    public GameObject gateVisual;
    public Collider2D solidCollider;

    [Header("Interact HUD Prompt")]
    [SerializeField] protected string promptKey = "F";
    [Tooltip("If true, approaching while locked still shows a prompt, rattles the gate, and plays a reaction line.")]
    [SerializeField] protected bool showLockedFeedback = true;

    [Header("Locked Reaction (DialogueUI)")]
    [TextArea(2, 3)]
    [SerializeField] protected string lockedReactionText = "It won't budge.";
    [SerializeField] protected float reactionDuration = 2.5f;
    [Tooltip("(Optional) Overrides the player's default portrait for this reaction.")]
    [SerializeField] protected Sprite reactionOverridePortrait;

    [Header("Shared Feedback")]
    [SerializeField] protected float rattleMagnitude = 0.06f;
    [SerializeField] protected float lockedRattleDuration = 0.18f;
    [SerializeField] protected float unlockShakeDuration = 0.14f;
    [SerializeField] protected float unlockShakeMagnitude = 0.1f;
    [SerializeField] protected GameObject unlockVfxPrefab;

    protected Transform playerTransform;
    protected Player cachedPlayer;
    protected Vector3 visualBaseLocalPos;
    protected Vector3 visualBaseLocalScale = Vector3.one;
    protected Quaternion visualBaseLocalRot = Quaternion.identity;
    protected bool isPromptShowing;
    protected bool isAnimating;
    protected Coroutine animCoroutine;

    private SpriteRenderer[] visualRenderers;
    private Color[] originalColors;
    private bool hasCachedVisual;

    protected virtual void Awake()
    {
        CacheVisualIfNeeded();
    }

    protected virtual void OnDisable()
    {
        isPromptShowing = false;
        InteractPromptUI.Instance?.Unregister(this);
        SetHighlight(false);
    }

    protected virtual void Update()
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
        InteractPromptUI.Instance?.Unregister(this);
        SetHighlight(false);
    }

    public virtual void Interact()
    {
        if (isUnlocked || isAnimating) return;

        if (CanUnlock())
        {
            PerformUnlock();
        }
        else if (showLockedFeedback)
        {
            TriggerLockedReaction(lockedReactionText);
        }
    }

    public virtual void UnlockImmediate()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        isAnimating = false;
        isUnlocked = true;
        HideInteractPrompt();

        if (gateVisual != null) gateVisual.SetActive(false);
        if (solidCollider != null) solidCollider.enabled = false;
    }

    public virtual void ResetToLocked()
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

    protected abstract bool CanUnlock();
    protected abstract string GetPromptLabel();
    protected abstract void PerformUnlock();

    protected virtual bool ShouldShowPrompt()
    {
        return CanUnlock() || showLockedFeedback;
    }

    protected void RefreshPromptDisplay()
    {
        if (ShouldShowPrompt())
        {
            InteractPromptUI.Instance?.Register(this, GetPromptLabel(), Interact, HandleFocusChanged, promptKey);
        }
        else
        {
            InteractPromptUI.Instance?.Unregister(this);
            SetHighlight(false);
        }
    }

    protected void TriggerLockedReaction(string reactionText)
    {
        if (!string.IsNullOrEmpty(reactionText))
        {
            DialogueUI.Instance?.ShowPlayerReaction(reactionText, reactionDuration, reactionOverridePortrait);
        }

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(LockedRattleRoutine());
    }

    protected void HandleFocusChanged(bool isFocused)
    {
        SetHighlight(isFocused && isPromptShowing && !isUnlocked && !isAnimating);
    }

    protected void SetHighlight(bool active)
    {
        if (highlightEffect == null) return;
        if (active) highlightEffect.TurnOnHighlight();
        else highlightEffect.TurnOffHighlight();
    }

    protected Transform GetPlayerTransform()
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

    protected IEnumerator LockedRattleRoutine()
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

    protected void CacheVisualIfNeeded()
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

    protected void ApplyVisualTint(Color tintColor, float tintAmount, float alphaMultiplier)
    {
        if (visualRenderers == null) return;

        for (int i = 0; i < visualRenderers.Length; i++)
        {
            if (visualRenderers[i] == null) continue;
            Color c = Color.Lerp(originalColors[i], tintColor, tintAmount);
            c.a = originalColors[i].a * alphaMultiplier;
            visualRenderers[i].color = c;
        }
    }

    protected void RestoreVisualColors()
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

    protected void TriggerCameraShake(float duration, float magnitude)
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
}