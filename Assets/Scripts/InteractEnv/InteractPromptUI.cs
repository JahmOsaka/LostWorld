using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance { get; private set; }

    private class PromptEntry
    {
        public object Requester;
        public string Label;
        public string KeyLabel;
        public Action OnInteract;
        public Action<bool> OnFocusChanged;
        public bool IsFocused;
    }

    [Header("HUD List References")]
    [Tooltip("Root CanvasGroup for fading the entire prompt list.")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("VerticalLayoutGroup container holding spawned InteractPromptRow items.")]
    [SerializeField] private RectTransform listContainer;
    [Tooltip("Prefab with InteractPromptRow component (text-only row, no skill icon).")]
    [SerializeField] private InteractPromptRow rowPrefab;

    [Header("Selection Indicator (Left of Selected Row)")]
    [Tooltip("Indicator container holding the [F] badge and optional Scroll/W-S hint.")]
    [SerializeField] private RectTransform selectionIndicator;
    [SerializeField] private TMP_Text keyBadgeText;
    [Tooltip("Shown only when more than 1 interactable is in range (e.g., Mouse Wheel + W/S icon).")]
    [SerializeField] private GameObject multiSelectHint;

    [Header("Animation & Input")]
    [SerializeField] private float fadeSpeed = 14f;
    [SerializeField] private float indicatorFollowSpeed = 22f;
    [SerializeField] private float scrollThreshold = 0.05f;

    private readonly List<PromptEntry> entries = new List<PromptEntry>();
    private readonly List<InteractPromptRow> rowPool = new List<InteractPromptRow>();

    private PlayerInputHandler playerInput;
    private int selectedIndex = 0;
    private bool isDirty = false;

    public bool HasActivePrompts => entries.Count > 0;

    private readonly HashSet<object> suppressors = new HashSet<object>();
    public bool IsSuppressed => suppressors.Count > 0;
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
    }

    private void Update()
    {
        if (playerInput == null)
        {
            playerInput = FindAnyObjectByType<PlayerInputHandler>();
        }

        CleanupDestroyedEntries();

        if (!IsSuppressed)
        {
            if (entries.Count > 1)
            {
                HandleSelectionInput();
            }

            if (entries.Count > 0 && playerInput != null && playerInput.InteractInput)
            {
                PromptEntry current = entries[selectedIndex];
                if (current.OnInteract != null)
                {
                    playerInput.UseInteractInput();
                    current.OnInteract.Invoke();
                }
            }
        }

        if (isDirty)
        {
            RebuildListUI();
            isDirty = false;
        }

        UpdateVisualSmooth();
    }

    public void Register(object requester, string actionLabel, Action onInteract, Action<bool> onFocusChanged = null, string keyLabel = "F")
    {
        if (requester == null) return;

        int existingIdx = FindEntryIndex(requester);
        if (existingIdx != -1)
        {
            PromptEntry entry = entries[existingIdx];
            bool changed = entry.Label != actionLabel || entry.KeyLabel != keyLabel;
            entry.Label = actionLabel;
            entry.KeyLabel = keyLabel;
            entry.OnInteract = onInteract;
            entry.OnFocusChanged = onFocusChanged;

            if (changed) isDirty = true;
            return;
        }

        entries.Add(new PromptEntry
        {
            Requester = requester,
            Label = actionLabel,
            KeyLabel = keyLabel,
            OnInteract = onInteract,
            OnFocusChanged = onFocusChanged,
            IsFocused = false
        });

        ClampSelectionAndNotifyFocus();
        isDirty = true;
    }

    public void Unregister(object requester)
    {
        int idx = FindEntryIndex(requester);
        if (idx == -1) return;

        PromptEntry removed = entries[idx];
        if (removed.IsFocused)
        {
            removed.IsFocused = false;
            removed.OnFocusChanged?.Invoke(false);
        }

        entries.RemoveAt(idx);
        ClampSelectionAndNotifyFocus();
        isDirty = true;
    }

    public bool IsSelected(object requester)
    {
        if (entries.Count == 0 || selectedIndex < 0 || selectedIndex >= entries.Count) return false;
        return entries[selectedIndex].Requester == requester;
    }

    public void Show(object requester, Transform target, string actionLabel, string keyLabel = "F", Vector3? customOffset = null)
    {
        Action fallbackInteract = null;
        if (requester is IInteractable interactable)
        {
            fallbackInteract = interactable.Interact;
        }
        Register(requester, actionLabel, fallbackInteract, null, keyLabel);
    }

    public void Hide(object requester)
    {
        Unregister(requester);
    }

    private void HandleSelectionInput()
    {
        int moveDir = 0;

        if (Mouse.current != null)
        {
            float scrollY = Mouse.current.scroll.ReadValue().y;
            if (scrollY > scrollThreshold) moveDir = -1;
            else if (scrollY < -scrollThreshold) moveDir = 1;
        }

        if (moveDir == 0 && Keyboard.current != null)
        {
            if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                moveDir = -1;
            else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                moveDir = 1;
        }

        if (moveDir != 0)
        {
            selectedIndex = (selectedIndex + moveDir + entries.Count) % entries.Count;
            ClampSelectionAndNotifyFocus();
            isDirty = true;
        }
    }

    private void ClampSelectionAndNotifyFocus()
    {
        if (entries.Count == 0)
        {
            selectedIndex = 0;
            return;
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, entries.Count - 1);

        for (int i = 0; i < entries.Count; i++)
        {
            bool shouldFocus = (i == selectedIndex);
            if (entries[i].IsFocused != shouldFocus)
            {
                entries[i].IsFocused = shouldFocus;
                entries[i].OnFocusChanged?.Invoke(shouldFocus);
            }
        }
    }

    private void RebuildListUI()
    {
        if (listContainer == null || rowPrefab == null) return;

        while (rowPool.Count < entries.Count)
        {
            InteractPromptRow newRow = Instantiate(rowPrefab, listContainer);
            rowPool.Add(newRow);
        }

        for (int i = 0; i < rowPool.Count; i++)
        {
            bool active = i < entries.Count;
            rowPool[i].gameObject.SetActive(active);

            if (active)
            {
                rowPool[i].Setup(entries[i].Label, i == selectedIndex);
            }
        }

        if (entries.Count > 0 && keyBadgeText != null)
        {
            keyBadgeText.text = entries[selectedIndex].KeyLabel;
        }

        if (multiSelectHint != null)
        {
            multiSelectHint.SetActive(entries.Count > 1);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(listContainer);
    }

    private void UpdateVisualSmooth()
    {
        float targetAlpha = (!IsSuppressed && entries.Count > 0) ? 1f : 0f;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
        }

        if (selectionIndicator != null && entries.Count > 0 && selectedIndex < rowPool.Count)
        {
            InteractPromptRow activeRow = rowPool[selectedIndex];
            if (activeRow != null && activeRow.gameObject.activeSelf && activeRow.RectTransform != null)
            {
                Vector3 pos = selectionIndicator.position;
                float targetY = activeRow.RectTransform.position.y;
                pos.y = Mathf.Lerp(pos.y, targetY, Time.unscaledDeltaTime * indicatorFollowSpeed);
                selectionIndicator.position = pos;
            }
        }
    }

    private void CleanupDestroyedEntries()
    {
        bool removedAny = false;
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            if (entries[i].Requester is UnityEngine.Object unityObj && unityObj == null)
            {
                entries.RemoveAt(i);
                removedAny = true;
            }
        }

        if (removedAny)
        {
            ClampSelectionAndNotifyFocus();
            isDirty = true;
        }
    }

    private int FindEntryIndex(object requester)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].Requester == requester) return i;
        }
        return -1;
    }

    public void SetSuppressed(object source, bool suppressed)
    {
        if (source == null) return;
        if (suppressed) suppressors.Add(source);
        else suppressors.Remove(source);
    }
}