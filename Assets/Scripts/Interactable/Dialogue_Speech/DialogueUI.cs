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
    [Tooltip("Used automatically when calling ShowPlayerReaction().")]
    [SerializeField] private CharacterProfile playerProfile;

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject portraitContainer;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject nameContainer;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [Tooltip("Shown only in Blocking Dialogue mode when the current line finishes typing.")]
    [SerializeField] private GameObject continueIndicator;

    [Header("Settings")]
    [SerializeField] private float charsPerSecond = 45f;
    [SerializeField] private float fadeSpeed = 12f;
    [SerializeField] private float defaultReactionDuration = 2.5f;

    public bool IsDialogueActive { get; private set; }
    public bool IsQuickSpeechActive { get; private set; }

    private readonly Queue<DialogueEntry> lineQueue = new Queue<DialogueEntry>();
    private Action onDialogueComplete;
    private Coroutine typingCoroutine;
    private Coroutine autoHideCoroutine;

    private string currentFullText;
    private bool isTyping;
    private bool isVisible;
    private int openedFrame = -1;

    private Player cachedPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (continueIndicator != null) continueIndicator.SetActive(false);
    }

    private void OnDisable()
    {
        if (IsDialogueActive)
        {
            SetPlayerLocked(false);
            InteractPromptUI.Instance?.SetSuppressed(this, false);
        }
    }

    private void Update()
    {
        UpdateFade();

        if (!IsDialogueActive || Time.frameCount == openedFrame) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        bool advancePressed = kb.fKey.wasPressedThisFrame
            || kb.spaceKey.wasPressedThisFrame
            || kb.enterKey.wasPressedThisFrame
            || kb.numpadEnterKey.wasPressedThisFrame;

        if (advancePressed)
        {
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
    public void PlayDialogue(DialogueData data, Action onComplete = null)
    {
        if (data == null || data.lines == null || data.lines.Count == 0) return;

        StopActiveCoroutines();
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

        if (IsDialogueActive)
        {
            IsDialogueActive = false;
            SetPlayerLocked(false);
            InteractPromptUI.Instance?.SetSuppressed(this, false);
        }

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
        ApplySpeakerVisuals(entry.speaker, entry.overridePortrait, entry.customSpeakerName);

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
        IsDialogueActive = false;
        isVisible = false;

        if (continueIndicator != null) continueIndicator.SetActive(false);

        SetPlayerLocked(false);
        InteractPromptUI.Instance?.SetSuppressed(this, false);

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
        if (canvasGroup == null) return;

        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
    }
}