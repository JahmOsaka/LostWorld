using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("Default Profiles")]
    [SerializeField] private CharacterProfile playerProfile;

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform dialoguePanelRect;
    [SerializeField] private GameObject portraitContainer;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject nameContainer;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject continueIndicator;

    [Header("Full Dialogue Layout (Hades Style)")]
    [SerializeField] private bool expandInFullDialogue = true;
    [SerializeField] private bool borderlessPortraitInFullDialogue = true;
    [SerializeField] private Vector2 fullPanelSize = new Vector2(960f, 220f);
    [SerializeField] private Vector2 fullPanelPosOffset = new Vector2(180f, 55f);
    [SerializeField] private Vector2 fullPortraitContainerPos = new Vector2(-190f, -150f);
    [SerializeField] private float fullPortraitScale = 1.65f;
    [SerializeField] private float fullPortraitImagePosY = -256f;
    [SerializeField] private float fullNameContainerHeight = 42f;
    [SerializeField] private float fullSpeakerNameFontSize = 32f;
    [SerializeField] private float fullDialogueFontMin = 24f;
    [SerializeField] private float fullDialogueFontMax = 30f;

    [Header("Speaker Side & Slide-In Animation")]
    [SerializeField] private bool alternateLeftRightSpeakers = true;
    [SerializeField] private bool playerOnLeft = true;
    [SerializeField] private bool flipPortraitOnRightSide = true;
    [SerializeField] private float portraitSlideDistance = 120f;
    [SerializeField] private float portraitSlideDuration = 0.22f;
    [SerializeField] private float panelShiftSpeed = 14f;

    [Header("HUD Fade During Full Dialogue")]
    [SerializeField] private CanvasGroup[] hudGroupsToFade;
    [SerializeField] private float hudFadeSpeed = 10f;
    [Range(0f, 1f)][SerializeField] private float hudHiddenAlpha = 0f;

    [Header("Settings")]
    [SerializeField] private float charsPerSecond = 45f;
    [SerializeField] private float fadeSpeed = 12f;
    [SerializeField] private float defaultReactionDuration = 2.5f;
    [SerializeField] private float reopenCooldown = 0.25f;

    public bool IsDialogueActive { get; private set; }
    public bool IsQuickSpeechActive { get; private set; }
    public bool IsBlockingInteract => IsDialogueActive || (Time.unscaledTime - closedTime < reopenCooldown);

    private Coroutine unsuppressCoroutine;
    private Coroutine portraitSlideCoroutine;
    private float closedTime = -999f;

    private readonly Queue<DialogueEntry> lineQueue = new Queue<DialogueEntry>();
    private Action onDialogueComplete;
    private Coroutine typingCoroutine;
    private Coroutine autoHideCoroutine;

    private string currentFullText;
    private bool isTyping;
    private bool isVisible;
    private int openedFrame = -1;

    private Player cachedPlayer;

    private RectTransform portraitContainerRect;
    private LayoutElement portraitLayoutElement;
    private RectMask2D portraitMask;
    private LayoutElement nameLayoutElement;

    private Vector2 compactPanelSize;
    private Vector2 compactPanelAnchoredPos;
    private bool compactPortraitIgnoreLayout;
    private bool compactPortraitMaskEnabled;
    private float compactPortraitMinWidth;
    private float compactPortraitPrefWidth;
    private Vector3 compactPortraitScale;
    private Vector2 compactPortraitAnchoredPos;
    private Color compactPortraitColor = Color.white;
    private float compactNamePrefHeight;
    private float compactSpeakerNameFontSize;
    private float compactDialogueFontSize;
    private float compactDialogueFontMin;
    private float compactDialogueFontMax;
    private bool hasCachedLayout;

    private Vector2 targetPanelAnchoredPos;
    private bool hasTargetPanelPos;
    private CharacterProfile lastFullSpeaker;
    private string lastFullSpeakerKey;
    private bool lastFullWasRightSide;
    private bool hasShownFirstFullLine;

    private float[] originalHudAlphas;
    private bool[] originalHudBlocksRaycasts;
    private bool[] originalHudInteractable;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (dialoguePanelRect == null)
        {
            dialoguePanelRect = canvasGroup != null
                ? canvasGroup.GetComponent<RectTransform>()
                : GetComponent<RectTransform>();
        }

        CacheCompactLayout();
        CacheHudGroups();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (continueIndicator != null) continueIndicator.SetActive(false);
    }

    private void CacheCompactLayout()
    {
        if (hasCachedLayout) return;

        if (dialoguePanelRect != null)
        {
            compactPanelSize = dialoguePanelRect.sizeDelta;
            compactPanelAnchoredPos = dialoguePanelRect.anchoredPosition;
            targetPanelAnchoredPos = compactPanelAnchoredPos;
        }

        if (portraitContainer != null)
        {
            portraitContainerRect = portraitContainer.GetComponent<RectTransform>();
            portraitLayoutElement = portraitContainer.GetComponent<LayoutElement>();
            portraitMask = portraitContainer.GetComponent<RectMask2D>();

            if (portraitLayoutElement != null)
            {
                compactPortraitIgnoreLayout = portraitLayoutElement.ignoreLayout;
                compactPortraitMinWidth = portraitLayoutElement.minWidth;
                compactPortraitPrefWidth = portraitLayoutElement.preferredWidth;
            }

            if (portraitMask != null)
            {
                compactPortraitMaskEnabled = portraitMask.enabled;
            }
        }

        if (portraitImage != null)
        {
            RectTransform rt = portraitImage.rectTransform;
            compactPortraitScale = rt.localScale;
            compactPortraitAnchoredPos = rt.anchoredPosition;
            compactPortraitColor = portraitImage.color;
        }

        if (nameContainer != null)
        {
            nameLayoutElement = nameContainer.GetComponent<LayoutElement>();
            if (nameLayoutElement != null)
                compactNamePrefHeight = nameLayoutElement.preferredHeight;
        }

        if (speakerNameText != null)
            compactSpeakerNameFontSize = speakerNameText.fontSize;

        if (dialogueText != null)
        {
            compactDialogueFontSize = dialogueText.fontSize;
            compactDialogueFontMin = dialogueText.fontSizeMin;
            compactDialogueFontMax = dialogueText.fontSizeMax;
        }

        hasCachedLayout = true;
    }

    private void CacheHudGroups()
    {
        if (hudGroupsToFade == null || hudGroupsToFade.Length == 0) return;

        int count = hudGroupsToFade.Length;
        originalHudAlphas = new float[count];
        originalHudBlocksRaycasts = new bool[count];
        originalHudInteractable = new bool[count];

        for (int i = 0; i < count; i++)
        {
            if (hudGroupsToFade[i] == null) continue;
            originalHudAlphas[i] = hudGroupsToFade[i].alpha;
            originalHudBlocksRaycasts[i] = hudGroupsToFade[i].blocksRaycasts;
            originalHudInteractable[i] = hudGroupsToFade[i].interactable;
        }
    }

    private bool ShouldPlaceOnRightSide(CharacterProfile speaker)
    {
        if (!alternateLeftRightSpeakers || !borderlessPortraitInFullDialogue) return false;

        bool isPlayer = (playerProfile != null && speaker == playerProfile);
        return isPlayer ? !playerOnLeft : playerOnLeft;
    }

    private void ApplyLayoutMode(
        bool isFullDialogue,
        bool hasPortrait = true,
        bool onRightSide = false,
        bool playSlideAnimation = false,
        bool snapPanelPos = true)
    {
        if (!expandInFullDialogue || !hasCachedLayout) return;

        StopPortraitSlide();

        bool useBorderless = isFullDialogue && borderlessPortraitInFullDialogue;

        if (dialoguePanelRect != null)
        {
            dialoguePanelRect.sizeDelta = isFullDialogue ? fullPanelSize : compactPanelSize;

            if (isFullDialogue)
            {
                float xOffset = (useBorderless && hasPortrait)
                    ? (onRightSide ? -fullPanelPosOffset.x : fullPanelPosOffset.x)
                    : 0f;
                targetPanelAnchoredPos = compactPanelAnchoredPos + new Vector2(xOffset, fullPanelPosOffset.y);
            }
            else
            {
                targetPanelAnchoredPos = compactPanelAnchoredPos;
            }

            hasTargetPanelPos = true;
            if (snapPanelPos || panelShiftSpeed <= 0f)
            {
                dialoguePanelRect.anchoredPosition = targetPanelAnchoredPos;
            }
        }

        if (portraitLayoutElement != null)
        {
            portraitLayoutElement.ignoreLayout = useBorderless ? true : compactPortraitIgnoreLayout;
            portraitLayoutElement.minWidth = compactPortraitMinWidth;
            portraitLayoutElement.preferredWidth = compactPortraitPrefWidth;
        }

        if (portraitMask != null)
        {
            portraitMask.enabled = useBorderless ? false : compactPortraitMaskEnabled;
        }

        if (portraitContainerRect != null && useBorderless)
        {
            float containerX = onRightSide
                ? (fullPanelSize.x - fullPortraitContainerPos.x)
                : fullPortraitContainerPos.x;
            portraitContainerRect.anchoredPosition = new Vector2(containerX, fullPortraitContainerPos.y);
        }

        if (portraitImage != null)
        {
            RectTransform rt = portraitImage.rectTransform;
            Vector3 targetScale = isFullDialogue
                ? compactPortraitScale * fullPortraitScale
                : compactPortraitScale;

            if (useBorderless && onRightSide && flipPortraitOnRightSide)
            {
                targetScale.x = -Mathf.Abs(targetScale.x);
            }
            else
            {
                targetScale.x = Mathf.Abs(targetScale.x);
            }

            rt.localScale = targetScale;
            portraitImage.color = compactPortraitColor;

            Vector2 finalPos = isFullDialogue
                ? new Vector2(compactPortraitAnchoredPos.x, fullPortraitImagePosY)
                : compactPortraitAnchoredPos;

            if (useBorderless && hasPortrait && playSlideAnimation && portraitSlideDuration > 0f)
            {
                float slideDir = onRightSide ? 1f : -1f;
                Vector2 startPos = finalPos + new Vector2(slideDir * portraitSlideDistance, 0f);
                portraitSlideCoroutine = StartCoroutine(PortraitSlideInRoutine(startPos, finalPos));
            }
            else
            {
                rt.anchoredPosition = finalPos;
            }
        }

        if (nameLayoutElement != null)
        {
            nameLayoutElement.preferredHeight = isFullDialogue ? fullNameContainerHeight : compactNamePrefHeight;
        }

        if (speakerNameText != null)
        {
            speakerNameText.fontSize = isFullDialogue ? fullSpeakerNameFontSize : compactSpeakerNameFontSize;
        }

        if (dialogueText != null)
        {
            if (dialogueText.enableAutoSizing)
            {
                dialogueText.fontSizeMin = isFullDialogue ? fullDialogueFontMin : compactDialogueFontMin;
                dialogueText.fontSizeMax = isFullDialogue ? fullDialogueFontMax : compactDialogueFontMax;
            }
            else
            {
                dialogueText.fontSize = isFullDialogue ? fullDialogueFontMax : compactDialogueFontSize;
            }
        }

        if (dialoguePanelRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanelRect);
        }
    }

    private IEnumerator PortraitSlideInRoutine(Vector2 startPos, Vector2 endPos)
    {
        if (portraitImage == null) yield break;

        RectTransform rt = portraitImage.rectTransform;
        Color c = compactPortraitColor;
        float elapsed = 0f;

        rt.anchoredPosition = startPos;
        c.a = 0f;
        portraitImage.color = c;

        while (elapsed < portraitSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / portraitSlideDuration);
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);

            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, easeOut);
            c.a = Mathf.Lerp(0f, compactPortraitColor.a, easeOut);
            portraitImage.color = c;

            yield return null;
        }

        rt.anchoredPosition = endPos;
        portraitImage.color = compactPortraitColor;
        portraitSlideCoroutine = null;
    }

    private void StopPortraitSlide()
    {
        if (portraitSlideCoroutine != null)
        {
            StopCoroutine(portraitSlideCoroutine);
            portraitSlideCoroutine = null;
        }
        if (portraitImage != null)
        {
            portraitImage.color = compactPortraitColor;
        }
    }

    private void OnDisable()
    {
        StopPortraitSlide();
        RestoreHudGroupsImmediate();

        if (unsuppressCoroutine != null)
        {
            StopCoroutine(unsuppressCoroutine);
            unsuppressCoroutine = null;
        }

        if (IsDialogueActive)
        {
            SetPlayerLocked(false);
        }

        InteractPromptUI.Instance?.SetSuppressed(this, false);
    }

    private void Update()
    {
        UpdateFade();
        UpdatePanelPosition();

        if (!IsDialogueActive || Time.frameCount == openedFrame) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        bool advancePressed = kb.fKey.wasPressedThisFrame
            || kb.spaceKey.wasPressedThisFrame
            || kb.enterKey.wasPressedThisFrame
            || kb.numpadEnterKey.wasPressedThisFrame;

        if (advancePressed)
        {
            cachedPlayer?.InputHandler?.UseInteractInput();

            if (isTyping)
            {
                FinishTypingImmediately();
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    private void UpdatePanelPosition()
    {
        if (!hasTargetPanelPos || dialoguePanelRect == null || panelShiftSpeed <= 0f) return;

        if ((dialoguePanelRect.anchoredPosition - targetPanelAnchoredPos).sqrMagnitude > 0.25f)
        {
            dialoguePanelRect.anchoredPosition = Vector2.Lerp(
                dialoguePanelRect.anchoredPosition,
                targetPanelAnchoredPos,
                Time.unscaledDeltaTime * panelShiftSpeed
            );
        }
        else if (dialoguePanelRect.anchoredPosition != targetPanelAnchoredPos)
        {
            dialoguePanelRect.anchoredPosition = targetPanelAnchoredPos;
        }
    }

    public void PlayDialogue(DialogueData data, Action onComplete = null)
    {
        if (data == null || data.lines == null || data.lines.Count == 0) return;

        StopActiveCoroutines();
        if (unsuppressCoroutine != null)
        {
            StopCoroutine(unsuppressCoroutine);
            unsuppressCoroutine = null;
        }

        hasShownFirstFullLine = false;
        lastFullSpeaker = null;
        lastFullSpeakerKey = null;

        IsQuickSpeechActive = false;
        IsDialogueActive = true;
        isVisible = true;
        openedFrame = Time.frameCount;
        onDialogueComplete = onComplete;

        lineQueue.Clear();
        foreach (DialogueEntry entry in data.lines)
        {
            if (entry != null) lineQueue.Enqueue(entry);
        }

        SetPlayerLocked(true);
        InteractPromptUI.Instance?.SetSuppressed(this, true);
        DisplayNextLine();
    }

    public void ShowPlayerReaction(string text, float duration = -1f, Sprite overridePortrait = null)
    {
        ShowSpeech(playerProfile, text, duration, overridePortrait);
    }

    public void ShowSpeech(CharacterProfile speaker, string text, float duration = -1f, Sprite overridePortrait = null)
    {
        if (IsDialogueActive || string.IsNullOrEmpty(text)) return;

        StopActiveCoroutines();
        ApplyLayoutMode(false, snapPanelPos: true);

        IsQuickSpeechActive = true;
        isVisible = true;

        if (continueIndicator != null) continueIndicator.SetActive(false);

        ApplySpeakerVisuals(speaker, overridePortrait, null);
        float holdTime = duration > 0f ? duration : defaultReactionDuration;
        autoHideCoroutine = StartCoroutine(QuickSpeechRoutine(text, holdTime));
    }

    public void HideImmediate()
    {
        StopActiveCoroutines();
        StopPortraitSlide();

        if (IsDialogueActive)
        {
            IsDialogueActive = false;
            closedTime = Time.unscaledTime;
            cachedPlayer?.InputHandler?.UseInteractInput();
            SetPlayerLocked(false);
            ScheduleUnsuppressInteract();
        }

        hasShownFirstFullLine = false;
        IsQuickSpeechActive = false;
        isVisible = false;
        isTyping = false;

        if (continueIndicator != null) continueIndicator.SetActive(false);
    }

    private void DisplayNextLine()
    {
        if (lineQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueEntry entry = lineQueue.Dequeue();
        Sprite portrait = entry.overridePortrait != null
            ? entry.overridePortrait
            : (entry.speaker != null ? entry.speaker.defaultPortrait : null);

        bool onRightSide = ShouldPlaceOnRightSide(entry.speaker);
        string speakerKey = !string.IsNullOrEmpty(entry.customSpeakerName)
            ? entry.customSpeakerName
            : (entry.speaker != null ? entry.speaker.characterName : string.Empty);

        bool speakerChanged = !hasShownFirstFullLine
            || entry.speaker != lastFullSpeaker
            || speakerKey != lastFullSpeakerKey
            || onRightSide != lastFullWasRightSide;

        bool snapPanel = !hasShownFirstFullLine;

        ApplySpeakerVisuals(entry.speaker, entry.overridePortrait, entry.customSpeakerName);
        ApplyLayoutMode(true, portrait != null, onRightSide, speakerChanged, snapPanel);

        hasShownFirstFullLine = true;
        lastFullSpeaker = entry.speaker;
        lastFullSpeakerKey = speakerKey;
        lastFullWasRightSide = onRightSide;

        if (continueIndicator != null) continueIndicator.SetActive(false);

        StopActiveCoroutines();
        typingCoroutine = StartCoroutine(TypeTextRoutine(entry.text));
    }

    private void ApplySpeakerVisuals(CharacterProfile profile, Sprite overridePortrait, string customName)
    {
        Sprite portrait = overridePortrait != null
            ? overridePortrait
            : (profile != null ? profile.defaultPortrait : null);

        string displayName = !string.IsNullOrEmpty(customName)
            ? customName
            : (profile != null ? profile.characterName : string.Empty);

        if (portraitContainer != null)
        {
            portraitContainer.SetActive(portrait != null);
        }
        if (portraitImage != null && portrait != null)
        {
            portraitImage.sprite = portrait;
        }

        if (nameContainer != null)
        {
            nameContainer.SetActive(!string.IsNullOrEmpty(displayName));
        }
        if (speakerNameText != null)
        {
            speakerNameText.text = displayName;
            speakerNameText.color = profile != null ? profile.nameColor : Color.white;
        }
    }

    private IEnumerator TypeTextRoutine(string text)
    {
        isTyping = true;
        currentFullText = text ?? string.Empty;

        if (dialogueText != null)
        {
            dialogueText.text = currentFullText;
            dialogueText.maxVisibleCharacters = 0;

            float delay = charsPerSecond > 0f ? 1f / charsPerSecond : 0f;
            int totalChars = currentFullText.Length;

            if (delay <= 0f)
            {
                dialogueText.maxVisibleCharacters = totalChars;
            }
            else
            {
                float timer = 0f;
                int visible = 0;
                while (visible < totalChars)
                {
                    timer += Time.deltaTime;
                    int nextVisible = Mathf.Min(totalChars, Mathf.FloorToInt(timer * charsPerSecond));
                    if (nextVisible > visible)
                    {
                        visible = nextVisible;
                        dialogueText.maxVisibleCharacters = visible;
                    }
                    yield return null;
                }
            }
        }

        isTyping = false;
        typingCoroutine = null;

        if (IsDialogueActive && continueIndicator != null)
        {
            continueIndicator.SetActive(true);
        }
    }

    private IEnumerator QuickSpeechRoutine(string text, float holdDuration)
    {
        yield return StartCoroutine(TypeTextRoutine(text));
        yield return new WaitForSeconds(holdDuration);

        IsQuickSpeechActive = false;
        isVisible = false;
        autoHideCoroutine = null;
    }

    private void FinishTypingImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        if (dialogueText != null)
        {
            dialogueText.text = currentFullText;
            dialogueText.maxVisibleCharacters = currentFullText.Length;
        }

        if (IsDialogueActive && continueIndicator != null)
        {
            continueIndicator.SetActive(true);
        }
    }

    private void EndDialogue()
    {
        StopActiveCoroutines();
        StopPortraitSlide();

        hasShownFirstFullLine = false;
        IsDialogueActive = false;
        isVisible = false;
        closedTime = Time.unscaledTime;

        if (continueIndicator != null) continueIndicator.SetActive(false);

        cachedPlayer?.InputHandler?.UseInteractInput();
        SetPlayerLocked(false);
        ScheduleUnsuppressInteract();

        Action callback = onDialogueComplete;
        onDialogueComplete = null;
        callback?.Invoke();
    }

    private void SetPlayerLocked(bool locked)
    {
        if (cachedPlayer == null)
        {
            cachedPlayer = FindAnyObjectByType<Player>();
        }
        if (cachedPlayer == null) return;

        if (locked)
        {
            if (cachedPlayer.RB != null) cachedPlayer.RB.linearVelocity = Vector2.zero;
            cachedPlayer.StateMachine?.ChangeState(cachedPlayer.IdleState);
            cachedPlayer.enabled = false;
        }
        else
        {
            cachedPlayer.enabled = true;
        }
    }

    private void StopActiveCoroutines()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
        isTyping = false;
    }

    private void UpdateFade()
    {
        if (canvasGroup != null)
        {
            float targetAlpha = isVisible ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
        }

        UpdateHudFade();
    }

    private void UpdateHudFade()
    {
        if (hudGroupsToFade == null || originalHudAlphas == null) return;

        bool hideHud = IsDialogueActive;
        float step = Time.unscaledDeltaTime * hudFadeSpeed;

        for (int i = 0; i < hudGroupsToFade.Length; i++)
        {
            CanvasGroup group = hudGroupsToFade[i];
            if (group == null) continue;

            float targetAlpha = hideHud ? hudHiddenAlpha : originalHudAlphas[i];
            group.alpha = hudFadeSpeed > 0f
                ? Mathf.MoveTowards(group.alpha, targetAlpha, step)
                : targetAlpha;

            group.blocksRaycasts = !hideHud && originalHudBlocksRaycasts[i];
            group.interactable = !hideHud && originalHudInteractable[i];
        }
    }

    private void RestoreHudGroupsImmediate()
    {
        if (hudGroupsToFade == null || originalHudAlphas == null) return;

        for (int i = 0; i < hudGroupsToFade.Length; i++)
        {
            CanvasGroup group = hudGroupsToFade[i];
            if (group == null) continue;

            group.alpha = originalHudAlphas[i];
            group.blocksRaycasts = originalHudBlocksRaycasts[i];
            group.interactable = originalHudInteractable[i];
        }
    }

    private void ScheduleUnsuppressInteract()
    {
        if (unsuppressCoroutine != null) StopCoroutine(unsuppressCoroutine);

        if (isActiveAndEnabled)
        {
            unsuppressCoroutine = StartCoroutine(UnsuppressInteractAfterCooldown());
        }
        else
        {
            InteractPromptUI.Instance?.SetSuppressed(this, false);
        }
    }

    private IEnumerator UnsuppressInteractAfterCooldown()
    {
        yield return new WaitForSecondsRealtime(reopenCooldown);
        unsuppressCoroutine = null;

        if (!IsDialogueActive)
        {
            cachedPlayer?.InputHandler?.UseInteractInput();
            InteractPromptUI.Instance?.SetSuppressed(this, false);
        }
    }
}